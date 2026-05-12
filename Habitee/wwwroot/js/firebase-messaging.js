window.habiteeFirebaseMessaging = (() => {
    let messaging = null;
    let serviceWorkerRegistration = null;
    let dotNetRef = null;
    let currentToken = null;
    let currentPermission = "default";
    let appNotificationsEnabled = false;
    let lastError = null;
    let lastMessageSummary = null;
    let isInitialized = false;
    let onMessageUnsubscribe = null;
    let swMessageListenerRegistered = false;

    function getConfig() {
        return globalThis.habiteeFirebaseConfig;
    }

    function getPreferenceKey() {
        return "habitee.notifications.enabled";
    }

    function loadEnabledPreference() {
        try {
            return localStorage.getItem(getPreferenceKey()) === "true";
        } catch {
            return false;
        }
    }

    function saveEnabledPreference(value) {
        appNotificationsEnabled = value;

        try {
            localStorage.setItem(getPreferenceKey(), value ? "true" : "false");
        } catch {
            // Ignore storage errors and keep the in-memory value.
        }
    }

    function hasFirebaseSdk() {
        return typeof firebase !== "undefined" && !!firebase.initializeApp;
    }

    function isSupported() {
        return typeof window !== "undefined"
            && "Notification" in window
            && "serviceWorker" in navigator
            && hasFirebaseSdk();
    }

    function buildStatus(overrides) {
        const baseStatus = {
            isSupported: isSupported(),
            permission: currentPermission,
            appNotificationsEnabled: appNotificationsEnabled,
            fcmTokenStatus: getTokenStatus(),
            fcmToken: currentToken,
            lastError: lastError,
            lastMessageSummary: lastMessageSummary
        };

        return Object.assign(baseStatus, overrides || {});
    }

    function getTokenStatus() {
        if (!isSupported()) {
            return "unsupported";
        }

        if (!appNotificationsEnabled) {
            return currentPermission === "denied" ? "blocked" : "disabled";
        }

        if (lastError) {
            return "error";
        }

        if (currentToken) {
            return "ready";
        }

        if (currentPermission === "denied") {
            return "blocked";
        }

        if (currentPermission !== "granted") {
            return "not-enabled";
        }

        return "missing";
    }

    function normalizePermission() {
        currentPermission = "Notification" in window ? Notification.permission : "unsupported";
        return currentPermission;
    }

    function summarizePayload(payload, source) {
        return {
            source: source || "foreground",
            title: payload?.notification?.title || payload?.data?.title || "Habitee",
            body: payload?.notification?.body || payload?.data?.body || "New push message received.",
            receivedAt: new Date().toISOString()
        };
    }

    async function ensureInitialized() {
        if (isInitialized) {
            return buildStatus();
        }

        currentPermission = normalizePermission();
        appNotificationsEnabled = loadEnabledPreference();

        if (!isSupported()) {
            lastError = hasFirebaseSdk()
                ? "Firebase Messaging is not supported in this browser."
                : "Firebase SDK scripts failed to load.";
            return buildStatus();
        }

        try {
            const config = getConfig();

            if (!firebase.apps.length) {
                firebase.initializeApp(config.firebaseConfig);
            }

            messaging = firebase.messaging();
            serviceWorkerRegistration = await navigator.serviceWorker.register("/firebase-messaging-sw.js");

            if (!swMessageListenerRegistered) {
                navigator.serviceWorker.addEventListener("message", handleServiceWorkerMessage);
                swMessageListenerRegistered = true;
            }

            if (!onMessageUnsubscribe) {
                onMessageUnsubscribe = messaging.onMessage((payload) => {
                    lastMessageSummary = summarizePayload(payload, "foreground");

                    if (dotNetRef) {
                        dotNetRef.invokeMethodAsync("HandleFirebaseMessage", lastMessageSummary);
                    }
                });
            }

            isInitialized = true;
            lastError = null;

            if (currentPermission === "granted" && appNotificationsEnabled) {
                await refreshTokenCore();
            }
        } catch (error) {
            lastError = error?.message || String(error);
        }

        return buildStatus();
    }

    async function refreshTokenCore() {
        currentPermission = normalizePermission();

        if (!isSupported()) {
            lastError = "Firebase Messaging is not supported in this browser.";
            currentToken = null;
            return buildStatus();
        }

        if (currentPermission !== "granted") {
            currentToken = null;
            return buildStatus();
        }

        if (!appNotificationsEnabled) {
            currentToken = null;
            lastError = null;
            return buildStatus();
        }

        try {
            if (!messaging || !serviceWorkerRegistration) {
                await ensureInitialized();
            }

            currentToken = await messaging.getToken({
                vapidKey: getConfig().vapidPublicKey,
                serviceWorkerRegistration
            });

            if (!currentToken) {
                lastError = "No FCM registration token was returned.";
            } else {
                lastError = null;
            }
        } catch (error) {
            currentToken = null;
            lastError = error?.message || String(error);
        }

        return buildStatus();
    }

    function handleServiceWorkerMessage(event) {
        if (event?.data?.type !== "habitee-fcm-message") {
            return;
        }

        lastMessageSummary = event.data.summary || {
            source: event.data.source || "background",
            title: "Habitee",
            body: "New push message received.",
            receivedAt: new Date().toISOString()
        };

        if (dotNetRef) {
            dotNetRef.invokeMethodAsync("HandleFirebaseMessage", lastMessageSummary);
        }
    }

    return {
        initialize: async function (reference) {
            dotNetRef = reference || dotNetRef;
            return await ensureInitialized();
        },

        getStatus: function () {
            currentPermission = normalizePermission();
            return buildStatus();
        },

        requestPermissionAndToken: async function () {
            if (!isSupported()) {
                lastError = "Firebase Messaging is not supported in this browser.";
                return buildStatus();
            }

            try {
                currentPermission = await Notification.requestPermission();
            } catch (error) {
                lastError = error?.message || String(error);
            }

            if (currentPermission === "granted") {
                saveEnabledPreference(true);
                return await refreshTokenCore();
            }

            saveEnabledPreference(false);
            currentToken = null;
            return buildStatus();
        },

        refreshToken: async function () {
            return await refreshTokenCore();
        },

        disablePush: async function () {
            saveEnabledPreference(false);
            lastError = null;

            try {
                if (messaging && typeof messaging.deleteToken === "function") {
                    await messaging.deleteToken();
                }
            } catch (error) {
                lastError = error?.message || String(error);
            }

            currentToken = null;
            return buildStatus();
        },

        copyToken: async function () {
            if (!currentToken) {
                return false;
            }

            await navigator.clipboard.writeText(currentToken);
            return true;
        }
    };
})();

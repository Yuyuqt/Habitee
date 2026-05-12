importScripts("/js/firebase-config.js");
importScripts(`https://www.gstatic.com/firebasejs/${self.habiteeFirebaseConfig.sdkVersion}/firebase-app-compat.js`);
importScripts(`https://www.gstatic.com/firebasejs/${self.habiteeFirebaseConfig.sdkVersion}/firebase-messaging-compat.js`);

firebase.initializeApp(self.habiteeFirebaseConfig.firebaseConfig);

const messaging = firebase.messaging();

messaging.onBackgroundMessage((payload) => {
    const notificationTitle = payload?.notification?.title
        || payload?.data?.title
        || "Habitee";

    const notificationOptions = {
        body: payload?.notification?.body || payload?.data?.body || "New push message received.",
        icon: payload?.notification?.icon || "/icon-192.png",
        data: {
            url: payload?.fcmOptions?.link || payload?.data?.url || "/"
        }
    };

    self.registration.showNotification(notificationTitle, notificationOptions);
    broadcastMessage(payload, "background");
});

self.addEventListener("notificationclick", (event) => {
    event.notification.close();

    const targetUrl = event.notification?.data?.url || "/";

    event.waitUntil((async () => {
        const windowClients = await clients.matchAll({
            type: "window",
            includeUncontrolled: true
        });

        for (const client of windowClients) {
            if ("focus" in client) {
                client.postMessage({
                    type: "habitee-fcm-message",
                    source: "background-click",
                    summary: {
                        source: "background-click",
                        title: event.notification?.title || "Habitee",
                        body: event.notification?.body || "",
                        receivedAt: new Date().toISOString()
                    }
                });

                client.focus();

                if ("navigate" in client) {
                    client.navigate(targetUrl);
                }

                return;
            }
        }

        if (clients.openWindow) {
            await clients.openWindow(targetUrl);
        }
    })());
});

function broadcastMessage(payload, source) {
    const summary = {
        source,
        title: payload?.notification?.title || payload?.data?.title || "Habitee",
        body: payload?.notification?.body || payload?.data?.body || "New push message received.",
        receivedAt: new Date().toISOString()
    };

    clients.matchAll({
        type: "window",
        includeUncontrolled: true
    }).then((windowClients) => {
        for (const client of windowClients) {
            client.postMessage({
                type: "habitee-fcm-message",
                source,
                summary
            });
        }
    });
}

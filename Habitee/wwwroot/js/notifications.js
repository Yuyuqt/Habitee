window.habiteeNotifications = (() => {
    let visibilityHandler = null;

    function isSupported() {
        return typeof window !== "undefined" && "Notification" in window;
    }

    function normalizePermission() {
        return isSupported() ? Notification.permission : "unsupported";
    }

    return {
        isSupported: function () {
            return isSupported();
        },

        getPermission: function () {
            return normalizePermission();
        },

        requestPermission: async function () {
            if (!isSupported()) {
                return "unsupported";
            }

            return await Notification.requestPermission();
        },

        showNotification: function (title, options) {
            if (!isSupported() || Notification.permission !== "granted") {
                return false;
            }

            const notification = new Notification(title, {
                body: options?.body ?? "",
                icon: options?.icon ?? "/icon-192.png",
                tag: options?.tag ?? undefined
            });

            notification.onclick = () => {
                window.focus();

                if (options?.url) {
                    window.location.href = options.url;
                }
            };

            return true;
        },

        registerVisibilityCallback: function (dotNetRef) {
            if (visibilityHandler) {
                document.removeEventListener("visibilitychange", visibilityHandler);
            }

            visibilityHandler = () => {
                dotNetRef.invokeMethodAsync("HandleVisibilityChanged", document.visibilityState === "visible");
            };

            document.addEventListener("visibilitychange", visibilityHandler);
            return document.visibilityState === "visible";
        },

        unregisterVisibilityCallback: function () {
            if (visibilityHandler) {
                document.removeEventListener("visibilitychange", visibilityHandler);
                visibilityHandler = null;
            }
        }
    };
})();

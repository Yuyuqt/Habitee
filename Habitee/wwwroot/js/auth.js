const SESSION_KEY = "habitee-session-user-id";

window.habiteeAuth = {
    generateSalt: function () {
        const bytes = new Uint8Array(16);
        crypto.getRandomValues(bytes);
        return btoa(String.fromCharCode(...bytes));
    },

    hashPassword: async function (password, salt) {
        const encoder = new TextEncoder();
        const payload = encoder.encode(`${salt}:${password}`);
        const hashBuffer = await crypto.subtle.digest("SHA-256", payload);
        const hashBytes = Array.from(new Uint8Array(hashBuffer));
        return hashBytes.map(byte => byte.toString(16).padStart(2, "0")).join("");
    },

    getSessionUserId: function () {
        return localStorage.getItem(SESSION_KEY);
    },

    setSessionUserId: function (userId) {
        localStorage.setItem(SESSION_KEY, userId);
    },

    clearSession: function () {
        localStorage.removeItem(SESSION_KEY);
    }
};

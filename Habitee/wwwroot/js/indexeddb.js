const DB_NAME = "HabiteeDB";
const DB_VERSION = 3;
const STORES = {
    habits: "Habits",
    logs: "Logs",
    sentReminders: "SentReminders",
    users: "Users"
};

let dbInstance = null;

function getDB() {
    return new Promise((resolve, reject) => {
        if (dbInstance) {
            resolve(dbInstance);
            return;
        }

        const request = indexedDB.open(DB_NAME, DB_VERSION);

        request.onerror = event => {
            console.error("IndexedDB error:", event.target.errorCode);
            reject("Error opening database");
        };

        request.onsuccess = event => {
            dbInstance = event.target.result;
            resolve(dbInstance);
        };

        request.onupgradeneeded = event => {
            const db = event.target.result;
            const transaction = event.target.transaction;
            upgradeDatabase(db, transaction);
        };
    });
}

function upgradeDatabase(db, transaction) {
    const habitsStore = db.objectStoreNames.contains(STORES.habits)
        ? transaction.objectStore(STORES.habits)
        : db.createObjectStore(STORES.habits, { keyPath: "id" });

    if (!habitsStore.indexNames.contains("userId")) {
        habitsStore.createIndex("userId", "userId", { unique: false });
    }

    const logsStore = db.objectStoreNames.contains(STORES.logs)
        ? transaction.objectStore(STORES.logs)
        : db.createObjectStore(STORES.logs, { keyPath: "id" });

    if (!logsStore.indexNames.contains("habitId")) {
        logsStore.createIndex("habitId", "habitId", { unique: false });
    }

    if (!logsStore.indexNames.contains("date")) {
        logsStore.createIndex("date", "date", { unique: false });
    }

    if (!logsStore.indexNames.contains("userId")) {
        logsStore.createIndex("userId", "userId", { unique: false });
    }

    const sentStore = db.objectStoreNames.contains(STORES.sentReminders)
        ? transaction.objectStore(STORES.sentReminders)
        : db.createObjectStore(STORES.sentReminders, { keyPath: "id" });

    if (!sentStore.indexNames.contains("userId")) {
        sentStore.createIndex("userId", "userId", { unique: false });
    }

    if (!sentStore.indexNames.contains("habitId")) {
        sentStore.createIndex("habitId", "habitId", { unique: false });
    }

    const usersStore = db.objectStoreNames.contains(STORES.users)
        ? transaction.objectStore(STORES.users)
        : db.createObjectStore(STORES.users, { keyPath: "id" });

    if (!usersStore.indexNames.contains("normalizedEmail")) {
        usersStore.createIndex("normalizedEmail", "normalizedEmail", { unique: true });
    }
}

function requestToPromise(request, errorMessage) {
    return new Promise((resolve, reject) => {
        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(errorMessage);
    });
}

function runTransaction(storeNames, mode, work, errorMessage) {
    return getDB().then(db => new Promise((resolve, reject) => {
        const transaction = db.transaction(storeNames, mode);

        transaction.oncomplete = () => resolve(true);
        transaction.onerror = () => reject(errorMessage);
        transaction.onabort = () => reject(errorMessage);

        work(transaction);
    }));
}

function getRecordUserId(record) {
    return record?.userId ?? record?.UserId ?? null;
}

function getRecordHabitId(record) {
    return record?.habitId ?? record?.HabitId ?? null;
}

function withUserId(record, userId) {
    return {
        ...record,
        userId
    };
}

function getRecordId(record) {
    return record?.id ?? record?.Id ?? crypto.randomUUID();
}

function createScopedId(userId, rawId) {
    const baseId = rawId || crypto.randomUUID();
    const prefix = `${userId}:`;
    return baseId.startsWith(prefix) ? baseId : `${prefix}${baseId}`;
}

async function getAllFromStore(storeName) {
    const db = await getDB();
    const transaction = db.transaction([storeName], "readonly");
    const store = transaction.objectStore(storeName);
    return await requestToPromise(store.getAll(), `Error fetching ${storeName}`);
}

async function getAllByUser(storeName, userId) {
    if (!userId) {
        return [];
    }

    const db = await getDB();
    const transaction = db.transaction([storeName], "readonly");
    const store = transaction.objectStore(storeName);
    const index = store.index("userId");
    return await requestToPromise(index.getAll(IDBKeyRange.only(userId)), `Error fetching ${storeName}`);
}

async function deleteRecordsForUser(userId) {
    const [habits, logs, sentReminders] = await Promise.all([
        getAllByUser(STORES.habits, userId),
        getAllByUser(STORES.logs, userId),
        getAllByUser(STORES.sentReminders, userId)
    ]);

    return runTransaction(
        [STORES.habits, STORES.logs, STORES.sentReminders],
        "readwrite",
        transaction => {
            const habitStore = transaction.objectStore(STORES.habits);
            const logStore = transaction.objectStore(STORES.logs);
            const sentStore = transaction.objectStore(STORES.sentReminders);

            habits.forEach(habit => habitStore.delete(habit.id));
            logs.forEach(log => logStore.delete(log.id));
            sentReminders.forEach(reminder => sentStore.delete(reminder.id));
        },
        "Error clearing user data"
    );
}

window.habiteeDb = {
    init: async function () {
        await getDB();
        return true;
    },

    getAllHabits: async function (userId) {
        return await getAllByUser(STORES.habits, userId);
    },

    saveHabit: async function (habit) {
        const db = await getDB();
        return new Promise((resolve, reject) => {
            const transaction = db.transaction([STORES.habits], "readwrite");
            const store = transaction.objectStore(STORES.habits);
            const request = store.put(habit);

            request.onsuccess = () => resolve(true);
            request.onerror = () => reject("Error saving habit");
        });
    },

    deleteHabit: async function (id, userId) {
        const [habits, logs, sentReminders] = await Promise.all([
            getAllByUser(STORES.habits, userId),
            getAllByUser(STORES.logs, userId),
            getAllByUser(STORES.sentReminders, userId)
        ]);

        const targetHabit = habits.find(habit => habit.id === id);

        if (!targetHabit) {
            return true;
        }

        return runTransaction(
            [STORES.habits, STORES.logs, STORES.sentReminders],
            "readwrite",
            transaction => {
                const habitStore = transaction.objectStore(STORES.habits);
                const logStore = transaction.objectStore(STORES.logs);
                const sentStore = transaction.objectStore(STORES.sentReminders);

                habitStore.delete(id);

                logs
                    .filter(log => getRecordHabitId(log) === id)
                    .forEach(log => logStore.delete(log.id));

                sentReminders
                    .filter(reminder => getRecordHabitId(reminder) === id)
                    .forEach(reminder => sentStore.delete(reminder.id));
            },
            "Error deleting habit"
        );
    },

    getLogs: async function (userId) {
        return await getAllByUser(STORES.logs, userId);
    },

    saveLog: async function (log) {
        const db = await getDB();
        return new Promise((resolve, reject) => {
            const transaction = db.transaction([STORES.logs], "readwrite");
            const store = transaction.objectStore(STORES.logs);
            const request = store.put(log);

            request.onsuccess = () => resolve(true);
            request.onerror = () => reject("Error saving log");
        });
    },

    exportData: async function (userId) {
        const habits = await this.getAllHabits(userId);
        const logs = await this.getLogs(userId);
        return JSON.stringify({ habits, logs });
    },

    importData: async function (userId, jsonString) {
        try {
            const data = JSON.parse(jsonString);
            await deleteRecordsForUser(userId);

            return await runTransaction(
                [STORES.habits, STORES.logs],
                "readwrite",
                transaction => {
                    const habitStore = transaction.objectStore(STORES.habits);
                    const logStore = transaction.objectStore(STORES.logs);
                    const habitIdMap = new Map();

                    if (Array.isArray(data.habits)) {
                        data.habits.forEach(habit => {
                            const originalId = getRecordId(habit);
                            const scopedHabitId = createScopedId(userId, originalId);
                            habitIdMap.set(originalId, scopedHabitId);

                            habitStore.put({
                                ...withUserId(habit, userId),
                                id: scopedHabitId
                            });
                        });
                    }

                    if (Array.isArray(data.logs)) {
                        data.logs.forEach(log => {
                            const originalHabitId = getRecordHabitId(log);
                            const scopedHabitId = originalHabitId
                                ? (habitIdMap.get(originalHabitId) || createScopedId(userId, originalHabitId))
                                : "";

                            logStore.put({
                                ...withUserId(log, userId),
                                id: createScopedId(userId, getRecordId(log)),
                                habitId: scopedHabitId
                            });
                        });
                    }
                },
                "Error importing data"
            );
        }
        catch (error) {
            console.error("Invalid JSON format", error);
            return false;
        }
    },

    wasReminderSent: async function (userId, habitId, reminderId, date) {
        const db = await getDB();
        const id = `${userId}|${habitId}|${reminderId}|${date}`;

        return await new Promise((resolve, reject) => {
            const transaction = db.transaction([STORES.sentReminders], "readonly");
            const store = transaction.objectStore(STORES.sentReminders);
            const request = store.get(id);

            request.onsuccess = () => resolve(!!request.result);
            request.onerror = () => reject("Error checking sent reminder");
        });
    },

    markReminderSent: async function (sentReminder) {
        const db = await getDB();
        const reminderToSave = {
            ...sentReminder,
            id: sentReminder.id || `${sentReminder.userId}|${sentReminder.habitId}|${sentReminder.reminderId}|${sentReminder.date}`
        };

        return new Promise((resolve, reject) => {
            const transaction = db.transaction([STORES.sentReminders], "readwrite");
            const store = transaction.objectStore(STORES.sentReminders);
            const request = store.put(reminderToSave);

            request.onsuccess = () => resolve(true);
            request.onerror = () => reject("Error saving sent reminder");
        });
    },

    getUserById: async function (userId) {
        const db = await getDB();
        const transaction = db.transaction([STORES.users], "readonly");
        const store = transaction.objectStore(STORES.users);
        return await requestToPromise(store.get(userId), "Error fetching user");
    },

    getUserByEmail: async function (normalizedEmail) {
        const db = await getDB();
        const transaction = db.transaction([STORES.users], "readonly");
        const store = transaction.objectStore(STORES.users);
        const index = store.index("normalizedEmail");
        return await requestToPromise(index.get(normalizedEmail), "Error fetching user");
    },

    getUserCount: async function () {
        const db = await getDB();
        const transaction = db.transaction([STORES.users], "readonly");
        const store = transaction.objectStore(STORES.users);
        return await requestToPromise(store.count(), "Error counting users");
    },

    saveUser: async function (user) {
        const db = await getDB();
        return new Promise((resolve, reject) => {
            const transaction = db.transaction([STORES.users], "readwrite");
            const store = transaction.objectStore(STORES.users);
            const request = store.put(user);

            request.onsuccess = () => resolve(true);
            request.onerror = () => reject("Error saving user");
        });
    },

    claimLegacyData: async function (userId) {
        const [habits, logs, sentReminders] = await Promise.all([
            getAllFromStore(STORES.habits),
            getAllFromStore(STORES.logs),
            getAllFromStore(STORES.sentReminders)
        ]);

        const habitsToClaim = habits.filter(habit => !getRecordUserId(habit));
        const logsToClaim = logs.filter(log => !getRecordUserId(log));
        const remindersToClaim = sentReminders.filter(reminder => !getRecordUserId(reminder));

        if (habitsToClaim.length === 0 && logsToClaim.length === 0 && remindersToClaim.length === 0) {
            return true;
        }

        return runTransaction(
            [STORES.habits, STORES.logs, STORES.sentReminders],
            "readwrite",
            transaction => {
                const habitStore = transaction.objectStore(STORES.habits);
                const logStore = transaction.objectStore(STORES.logs);
                const sentStore = transaction.objectStore(STORES.sentReminders);

                habitsToClaim.forEach(habit => habitStore.put(withUserId(habit, userId)));
                logsToClaim.forEach(log => logStore.put(withUserId(log, userId)));
                remindersToClaim.forEach(reminder => sentStore.put(withUserId(reminder, userId)));
            },
            "Error claiming legacy data"
        );
    }
};

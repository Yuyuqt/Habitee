const DB_NAME = 'HabiteeDB';
const DB_VERSION = 2;

let dbInstance = null;

function getDB() {
    return new Promise((resolve, reject) => {
        if (dbInstance) {
            resolve(dbInstance);
            return;
        }

        const request = indexedDB.open(DB_NAME, DB_VERSION);

        request.onerror = (event) => {
            console.error("IndexedDB error:", event.target.errorCode);
            reject("Error opening database");
        };

        request.onsuccess = (event) => {
            dbInstance = event.target.result;
            resolve(dbInstance);
        };

        request.onupgradeneeded = (event) => {
            const db = event.target.result;
            
            if (!db.objectStoreNames.contains('Habits')) {
                db.createObjectStore('Habits', { keyPath: 'id' });
            }
            
            if (!db.objectStoreNames.contains('Logs')) {
                const logsStore = db.createObjectStore('Logs', { keyPath: 'id' });
                logsStore.createIndex('habitId', 'habitId', { unique: false });
                logsStore.createIndex('date', 'date', { unique: false });
            }

            if (!db.objectStoreNames.contains('SentReminders')) {
                db.createObjectStore('SentReminders', { keyPath: 'id' });
            }
        };
    });
}

window.habiteeDb = {
    init: async function() {
        await getDB();
        return true;
    },

    getAllHabits: async function() {
        const db = await getDB();
        return new Promise((resolve, reject) => {
            const transaction = db.transaction(['Habits'], 'readonly');
            const store = transaction.objectStore('Habits');
            const request = store.getAll();

            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject("Error fetching habits");
        });
    },

    saveHabit: async function(habit) {
        const db = await getDB();
        return new Promise((resolve, reject) => {
            const transaction = db.transaction(['Habits'], 'readwrite');
            const store = transaction.objectStore('Habits');
            const request = store.put(habit);

            request.onsuccess = () => resolve(true);
            request.onerror = () => reject("Error saving habit");
        });
    },

    deleteHabit: async function(id) {
        const db = await getDB();
        return new Promise((resolve, reject) => {
            const transaction = db.transaction(['Habits', 'Logs', 'SentReminders'], 'readwrite');
            
            // Delete habit
            const habitStore = transaction.objectStore('Habits');
            habitStore.delete(id);
            
            // Delete associated logs (simplified: we'd ideally use an index cursor here)
            const logStore = transaction.objectStore('Logs');
            const logIndex = logStore.index('habitId');
            const keyRange = IDBKeyRange.only(id);
            const request = logIndex.openCursor(keyRange);
            
            request.onsuccess = (event) => {
                const cursor = event.target.result;
                if (cursor) {
                    cursor.delete();
                    cursor.continue();
                }
            };

            const sentStore = transaction.objectStore('SentReminders');
            const sentRequest = sentStore.openCursor();

            sentRequest.onsuccess = (event) => {
                const cursor = event.target.result;
                if (cursor) {
                    if (cursor.value?.HabitId === id) {
                        cursor.delete();
                    }
                    cursor.continue();
                }
            };

            transaction.oncomplete = () => resolve(true);
            transaction.onerror = () => reject("Error deleting habit");
        });
    },

    getLogs: async function() {
        const db = await getDB();
        return new Promise((resolve, reject) => {
            const transaction = db.transaction(['Logs'], 'readonly');
            const store = transaction.objectStore('Logs');
            const request = store.getAll();

            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject("Error fetching logs");
        });
    },

    saveLog: async function(log) {
        const db = await getDB();
        return new Promise((resolve, reject) => {
            const transaction = db.transaction(['Logs'], 'readwrite');
            const store = transaction.objectStore('Logs');
            const request = store.put(log);

            request.onsuccess = () => resolve(true);
            request.onerror = () => reject("Error saving log");
        });
    },

    exportData: async function() {
        const habits = await this.getAllHabits();
        const logs = await this.getLogs();
        return JSON.stringify({ habits, logs });
    },

    importData: async function(jsonString) {
        try {
            const data = JSON.parse(jsonString);
            const db = await getDB();
            
            return new Promise((resolve, reject) => {
                const transaction = db.transaction(['Habits', 'Logs', 'SentReminders'], 'readwrite');
                const habitStore = transaction.objectStore('Habits');
                const logStore = transaction.objectStore('Logs');
                const sentStore = transaction.objectStore('SentReminders');
                
                // Clear existing
                habitStore.clear();
                logStore.clear();
                sentStore.clear();
                
                // Add new
                if (data.habits) {
                    data.habits.forEach(h => habitStore.put(h));
                }
                if (data.logs) {
                    data.logs.forEach(l => logStore.put(l));
                }
                
                transaction.oncomplete = () => resolve(true);
                transaction.onerror = () => reject("Error importing data");
            });
        } catch (e) {
            console.error("Invalid JSON format");
            return false;
        }
    },

    wasReminderSent: async function(habitId, reminderId, date) {
        const db = await getDB();
        const id = `${habitId}|${reminderId}|${date}`;

        return new Promise((resolve, reject) => {
            const transaction = db.transaction(['SentReminders'], 'readonly');
            const store = transaction.objectStore('SentReminders');
            const request = store.get(id);

            request.onsuccess = () => resolve(!!request.result);
            request.onerror = () => reject("Error checking sent reminder");
        });
    },

    markReminderSent: async function(sentReminder) {
        const db = await getDB();
        const reminderToSave = {
            ...sentReminder,
            id: sentReminder.id || `${sentReminder.habitId}|${sentReminder.reminderId}|${sentReminder.date}`
        };

        return new Promise((resolve, reject) => {
            const transaction = db.transaction(['SentReminders'], 'readwrite');
            const store = transaction.objectStore('SentReminders');
            const request = store.put(reminderToSave);

            request.onsuccess = () => resolve(true);
            request.onerror = () => reject("Error saving sent reminder");
        });
    }
};

window.habiteeTimePicker = {
    open: function (inputId) {
        var input = document.getElementById(inputId);
        if (!input) {
            return;
        }

        if (typeof input.showPicker === 'function') {
            input.showPicker();
        } else {
            input.focus();
            input.click();
        }
    }
};

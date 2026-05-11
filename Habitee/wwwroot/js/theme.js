window.themeInterop = {
    init: function () {
        const theme = localStorage.getItem('habitee-theme') || 'habitee';
        const mode = localStorage.getItem('habitee-theme-mode') || 'light';
        this.applyTheme(theme, mode);
        return { theme, mode };
    },
    
    setTheme: function (theme, mode) {
        localStorage.setItem('habitee-theme', theme);
        localStorage.setItem('habitee-theme-mode', mode);
        this.applyTheme(theme, mode);
    },
    
    applyTheme: function (theme, mode) {
        const html = document.documentElement;
        
        // Use requestAnimationFrame for a slight debounce/smooth transition
        window.requestAnimationFrame(() => {
            html.setAttribute('data-theme', theme);
            html.setAttribute('data-theme-mode', mode);
        });
    }
};

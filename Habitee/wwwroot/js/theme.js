window.themeInterop = {
    validThemes: ['habitee', 'pastel', 'sakura', 'galaxy', 'cyberpunk', 'harvest', 'prism'],

    init: function () {
        const storedTheme = localStorage.getItem('habitee-theme') || 'habitee';
        const theme = this.validThemes.includes(storedTheme) ? storedTheme : 'habitee';
        const mode = localStorage.getItem('habitee-theme-mode') || 'light';
        localStorage.setItem('habitee-theme', theme);
        this.applyTheme(theme, mode);
        return { theme, mode };
    },
    
    setTheme: function (theme, mode) {
        const safeTheme = this.validThemes.includes(theme) ? theme : 'habitee';
        localStorage.setItem('habitee-theme', safeTheme);
        localStorage.setItem('habitee-theme-mode', mode);
        this.applyTheme(safeTheme, mode);
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

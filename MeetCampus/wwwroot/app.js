// Theme switching helper for Daisy UI
window.applyTheme = function(theme) {
    const htmlElement = document.documentElement;

    console.log('Applying theme:', theme);

    if (theme === 'system') {
        // Check system preference
        const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
        const systemTheme = prefersDark ? 'dark' : 'light';
        htmlElement.setAttribute('data-theme', systemTheme);
        console.log('System theme detected:', systemTheme);
    } else {
        // Apply light or dark theme explicitly
        htmlElement.setAttribute('data-theme', theme);
        console.log('Theme set to:', theme);
    }
};

// Initialize theme on page load
window.initializeTheme = function() {
    const savedTheme = localStorage.getItem('theme');
    console.log('Saved theme from localStorage:', savedTheme);

    if (savedTheme) {
        window.applyTheme(savedTheme);
    } else {
        // Default to light theme
        window.applyTheme('light');
        localStorage.setItem('theme', 'light');
    }
};

// Run initialization when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', window.initializeTheme);
} else {
    window.initializeTheme();
}

// Listen for system theme changes
window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', function() {
    const theme = localStorage.getItem('theme');
    if (theme === 'system') {
        window.applyTheme('system');
    }
});

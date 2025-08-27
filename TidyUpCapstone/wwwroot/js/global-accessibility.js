class GlobalAccessibilityManager {
    constructor() {
        this.initialized = false;
        this.init();
    }

    async init() {
        if (this.initialized) return;

        // Load user's accessibility settings on every page
        await this.loadAndApplySettings();
        this.initialized = true;

        console.log('Global accessibility settings loaded');
    }

    async loadAndApplySettings() {
        try {
            // Get user's saved accessibility settings
            const response = await fetch('/Settings/GetLanguageSettings');
            const data = await response.json();

            if (data.success && data.data) {
                // Apply all accessibility settings
                this.applyHighContrast(data.data.highContrast || false);
                this.applyLargeText(data.data.largeText || false);
                this.applyReduceMotion(data.data.reduceMotion || false);
                this.applyScreenReader(data.data.screenReader || false);

                console.log('Applied global accessibility settings:', data.data);
            }
        } catch (error) {
            console.error('Failed to load accessibility settings:', error);
        }
    }

    applyHighContrast(enabled) {
        if (enabled) {
            document.body.classList.add('high-contrast-mode');
        } else {
            document.body.classList.remove('high-contrast-mode');
        }
    }

    applyLargeText(enabled) {
        if (enabled) {
            document.body.classList.add('large-text-mode');
        } else {
            document.body.classList.remove('large-text-mode');
        }
    }

    applyReduceMotion(enabled) {
        if (enabled) {
            document.body.classList.add('reduce-motion-mode');
        } else {
            document.body.classList.remove('reduce-motion-mode');
        }
    }

    applyScreenReader(enabled) {
        if (enabled) {
            document.body.classList.add('screen-reader-mode');
        } else {
            document.body.classList.remove('screen-reader-mode');
        }
    }

    // Public method to refresh settings (called when user changes settings)
    async refresh() {
        await this.loadAndApplySettings();
    }
}

// Create global instance
window.globalAccessibility = new GlobalAccessibilityManager();

// Auto-initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        if (!window.globalAccessibility.initialized) {
            window.globalAccessibility.init();
        }
    });
} else {
    if (!window.globalAccessibility.initialized) {
        window.globalAccessibility.init();
    }
}
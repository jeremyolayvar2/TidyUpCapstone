using TidyUpCapstone.Models.DTOs.Language;

namespace TidyUpCapstone.Services
{
    public class LanguageService : ILanguageService
    {
        private readonly Dictionary<string, SupportedLanguageDto> _supportedLanguages;
        private readonly Dictionary<string, Dictionary<string, string>> _translations;

        public LanguageService()
        {
            _supportedLanguages = InitializeSupportedLanguages();
            _translations = InitializeTranslations();
        }

        public async Task<Dictionary<string, string>> GetTranslationsAsync(string languageCode)
        {
            return await Task.FromResult(
                _translations.ContainsKey(languageCode)
                    ? _translations[languageCode]
                    : _translations["en"]
            );
        }

        public async Task<List<SupportedLanguageDto>> GetSupportedLanguagesAsync()
        {
            return await Task.FromResult(_supportedLanguages.Values.ToList());
        }

        public string GetLanguageName(string languageCode)
        {
            return _supportedLanguages.ContainsKey(languageCode)
                ? _supportedLanguages[languageCode].Name
                : "Unknown";
        }

        public string GetLanguageDirection(string languageCode)
        {
            return IsRightToLeft(languageCode) ? "rtl" : "ltr";
        }

        public bool IsRightToLeft(string languageCode)
        {
            var rtlLanguages = new[] { "ar", "he", "fa", "ur" };
            return rtlLanguages.Contains(languageCode.ToLower());
        }

        private Dictionary<string, SupportedLanguageDto> InitializeSupportedLanguages()
        {
            return new Dictionary<string, SupportedLanguageDto>
            {
                { "en", new SupportedLanguageDto { Code = "en", Name = "English", NativeName = "English", IsRTL = false } },
                { "tl", new SupportedLanguageDto { Code = "tl", Name = "Filipino (Tagalog)", NativeName = "Filipino", IsRTL = false } },
                { "es", new SupportedLanguageDto { Code = "es", Name = "Spanish", NativeName = "Español", IsRTL = false } },
                { "zh", new SupportedLanguageDto { Code = "zh", Name = "Chinese", NativeName = "中文", IsRTL = false } }
            };
        }

        private Dictionary<string, Dictionary<string, string>> InitializeTranslations()
        {
            return new Dictionary<string, Dictionary<string, string>>
            {
                {
                    "en", new Dictionary<string, string>
                    {
                        { "language_accessibility_title", "Language & Accessibility" },
                        { "language_accessibility_desc", "Customize your language preferences and accessibility options" },
                        { "save_button", "Save Language & Accessibility Settings" },
                        { "language_preferences", "Language Preferences" },
                        { "accessibility_options", "Accessibility Options" },
                        { "high_contrast", "High Contrast Mode" },
                        { "high_contrast_desc", "Increase contrast for better visibility" },
                        { "large_text", "Large Text" },
                        { "large_text_desc", "Increase text size for better readability" },
                        { "reduce_motion", "Reduce Motion" },
                        { "reduce_motion_desc", "Minimize animations and transitions" },
                        { "screen_reader", "Screen Reader Support" },
                        { "screen_reader_desc", "Optimize interface for screen readers" },
                        { "display_language", "Display Language" },
                        { "timezone", "Timezone" },
                        { "settings_saved", "Settings saved successfully!" },
                        { "error_saving", "Error saving settings. Please try again." }
                    }
                },
                {
                    "tl", new Dictionary<string, string>
                    {
                        { "language_accessibility_title", "Wika at Accessibility" },
                        { "language_accessibility_desc", "I-customize ang inyong mga preference sa wika at accessibility" },
                        { "save_button", "I-save ang mga Setting ng Wika at Accessibility" },
                        { "language_preferences", "Mga Preference sa Wika" },
                        { "accessibility_options", "Mga Opsyon sa Accessibility" },
                        { "high_contrast", "Mataas na Contrast Mode" },
                        { "high_contrast_desc", "Taasan ang contrast para sa mas malinaw na nakikita" },
                        { "large_text", "Malaking Teksto" },
                        { "large_text_desc", "Palakihin ang teksto para sa mas madaling pagbasa" },
                        { "reduce_motion", "Bawasan ang Motion" },
                        { "reduce_motion_desc", "Bawasan ang mga animation at transition" },
                        { "screen_reader", "Suporta sa Screen Reader" },
                        { "screen_reader_desc", "I-optimize ang interface para sa mga screen reader" },
                        { "display_language", "Wika ng Display" },
                        { "timezone", "Timezone" },
                        { "settings_saved", "Matagumpay na na-save ang mga setting!" },
                        { "error_saving", "May error sa pag-save ng settings. Subukan ulit." }
                    }
                },
                {
                    "es", new Dictionary<string, string>
                    {
                        { "language_accessibility_title", "Idioma y Accesibilidad" },
                        { "language_accessibility_desc", "Personaliza tus preferencias de idioma y opciones de accesibilidad" },
                        { "save_button", "Guardar Configuración de Idioma y Accesibilidad" },
                        { "language_preferences", "Preferencias de Idioma" },
                        { "accessibility_options", "Opciones de Accesibilidad" },
                        { "high_contrast", "Modo de Alto Contraste" },
                        { "high_contrast_desc", "Aumentar el contraste para mejor visibilidad" },
                        { "large_text", "Texto Grande" },
                        { "large_text_desc", "Aumentar el tamaño del texto para mejor legibilidad" },
                        { "reduce_motion", "Reducir Movimiento" },
                        { "reduce_motion_desc", "Minimizar animaciones y transiciones" },
                        { "screen_reader", "Soporte para Lector de Pantalla" },
                        { "screen_reader_desc", "Optimizar interfaz para lectores de pantalla" },
                        { "display_language", "Idioma de Visualización" },
                        { "timezone", "Zona Horaria" },
                        { "settings_saved", "¡Configuración guardada exitosamente!" },
                        { "error_saving", "Error al guardar la configuración. Inténtalo de nuevo." }
                    }
                }
            };
        }
    }
}
using TidyUpCapstone.Models.DTOs.Language;

namespace TidyUpCapstone.Services
{
    public interface ILanguageService
    {
        Task<Dictionary<string, string>> GetTranslationsAsync(string languageCode);
        Task<List<SupportedLanguageDto>> GetSupportedLanguagesAsync();
        string GetLanguageName(string languageCode);
        string GetLanguageDirection(string languageCode);
        bool IsRightToLeft(string languageCode);
    }
}
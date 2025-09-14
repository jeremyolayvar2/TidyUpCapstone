using System.ComponentModel.DataAnnotations;

namespace TidyUpCapstone.Models.ViewModels
{
    public class NotificationSettingsViewModel
    {
        [Display(Name = "New Messages")]
        public bool EmailNewMessages { get; set; } = true;

        [Display(Name = "Item Updates")]
        public bool EmailItemUpdates { get; set; } = true;

        [Display(Name = "Weekly Summary")]
        public bool EmailWeeklySummary { get; set; } = false;

        [Display(Name = "Desktop Notifications")]
        public bool DesktopNotifications { get; set; } = true;

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
using TidyUpCapstone.Models.DTOs.Transactions;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Models.ViewModels
{
    public class MessagePageViewModel
    {
        public AppUser? CurrentUser { get; set; }
        public AppUser? OtherUser { get; set; }
        
        

        public int? CurrentTransactionId { get; set; }
        public TransactionStatusDto? TransactionStatus { get; set; }
        public bool HasActiveTransaction { get; set; }
        public string? TransactionItemTitle { get; set; }
        public decimal? TransactionAmount { get; set; }
    }
}
using TidyUpCapstone.Models.ViewModels.Shared;
using TidyUpCapstone.Models.Entities.Transactions;

using TidyUpCapstone.Models.DTOs.Transactions;

namespace TidyUpCapstone.Models.ViewModels.Transactions
{
    public class TransactionListViewModel
    {
        public List<TransactionDto> BuyerTransactions { get; set; } = new List<TransactionDto>();
        public List<TransactionDto> SellerTransactions { get; set; } = new List<TransactionDto>();
        public List<TransactionDto> AllTransactions { get; set; } = new List<TransactionDto>();
        public string ActiveTab { get; set; } = "all";
        public TransactionFilterDto Filter { get; set; } = new TransactionFilterDto();
        public PaginationViewModel Pagination { get; set; } = new PaginationViewModel();
        public TransactionStatsViewModel Stats { get; set; } = new TransactionStatsViewModel();
    }

    public class TransactionDetailsViewModel
    {
        public TransactionDto Transaction { get; set; } = null!;
        public ChatDto Chat { get; set; } = null!;
        public List<ChatMessageDto> Messages { get; set; } = new List<ChatMessageDto>();
        public SendMessageDto NewMessage { get; set; } = new SendMessageDto();
        public bool CanSendMessage { get; set; }
        public bool CanCompleteTransaction { get; set; }
        public bool CanCancelTransaction { get; set; }
        public bool CanReleaseEscrow { get; set; }
        public bool ShowEscrowControls { get; set; }
        public string UserRole { get; set; } = string.Empty; // "buyer" or "seller"
        public UpdateTransactionStatusDto StatusUpdate { get; set; } = new UpdateTransactionStatusDto();
    }

    public class TransactionFilterDto
    {
        public TransactionStatus? Status { get; set; }
        public DeliveryMethod? DeliveryMethod { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string? ItemTitle { get; set; }
        public string? UserRole { get; set; } // "buyer", "seller", "all"
    }

    public class TransactionStatsViewModel
    {
        public int TotalTransactions { get; set; }
        public int PendingTransactions { get; set; }
        public int CompletedTransactions { get; set; }
        public int CancelledTransactions { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal TotalEarned { get; set; }
        public decimal AverageTransactionValue { get; set; }
        public int SuccessRate { get; set; }
    }
}
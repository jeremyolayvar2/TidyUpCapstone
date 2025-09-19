namespace TidyUpCapstone.Services
{
    public interface IEscrowService
    {
        Task<(bool success, string message)> AutoEscrowOnChatAsync(int buyerId, int sellerId, decimal amount);
        Task<(bool success, string message)> ConfirmTransactionAsync(int transactionId, int userId);
        Task<(bool success, string message)> CancelTransactionAsync(int transactionId, int userId, string reason = null);
    }
}
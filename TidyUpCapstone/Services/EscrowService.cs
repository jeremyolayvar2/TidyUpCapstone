using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.Entities.Transactions;
using TidyUpCapstone.Models.Entities.Items;

namespace TidyUpCapstone.Services
{
    public class EscrowService : IEscrowService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EscrowService> _logger;

        public EscrowService(ApplicationDbContext context, ILogger<EscrowService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool success, string message)> AutoEscrowOnChatAsync(int buyerId, int sellerId, decimal amount)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingTransaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.BuyerId == buyerId && t.SellerId == sellerId &&
                                        t.TransactionStatus != TransactionStatus.Cancelled);

                if (existingTransaction != null && existingTransaction.TransactionStatus == TransactionStatus.Escrowed)
                {
                    return (true, "Transaction already escrowed");
                }

                var buyer = await _context.Users.FindAsync(buyerId);
                if (buyer == null) return (false, "Buyer not found");

                if (buyer.AvailableBalance < amount)
                {
                    return (false, $"Insufficient balance. Available: {buyer.AvailableBalance}, Required: {amount}");
                }

                var item = await _context.Items.FirstOrDefaultAsync(i => i.UserId == sellerId);

                if (item == null) return (false, "No item found for transaction");

                Models.Entities.Transactions.Transaction txn;
                if (existingTransaction == null)
                {
                    txn = new Models.Entities.Transactions.Transaction
                    {
                        BuyerId = buyerId,
                        SellerId = sellerId,
                        ItemId = item.ItemId,
                        TokenAmount = amount,
                        TransactionStatus = TransactionStatus.Escrowed,
                        DeliveryMethod = DeliveryMethod.Pickup,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Transactions.Add(txn);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    txn = existingTransaction;
                    txn.TransactionStatus = TransactionStatus.Escrowed;
                }

                buyer.TokenBalance -= amount;      // Deduct from main balance
                buyer.EscrowedBalance += amount;   // Move to escrow

                var escrow = new Escrow
                {
                    TransactionId = txn.TransactionId,
                    Amount = amount,
                    Status = EscrowStatus.Held,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Escrows.Add(escrow);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Tokens automatically escrowed");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error auto-escrowing tokens");
                return (false, "Error processing escrow");
            }
        }

        public async Task<(bool success, string message)> ConfirmTransactionAsync(int transactionId, int userId)
        {
            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var txn = await _context.Transactions
                    .Include(t => t.Buyer)
                    .Include(t => t.Seller)
                    .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

                if (txn == null) return (false, "Transaction not found");
                if (txn.TransactionStatus != TransactionStatus.Escrowed) return (false, "Transaction not in escrow");

                var isBuyer = txn.BuyerId == userId;
                var isSeller = txn.SellerId == userId;
                if (!isBuyer && !isSeller) return (false, "Not authorized");

                if (isBuyer && !txn.BuyerConfirmed)
                {
                    txn.BuyerConfirmed = true;
                }
                else if (isSeller && !txn.SellerConfirmed)
                {
                    txn.SellerConfirmed = true;
                }
                else
                {
                    return (false, "Already confirmed or cannot confirm");
                }

                if (txn.BuyerConfirmed && txn.SellerConfirmed)
                {
                    txn.Buyer.EscrowedBalance -= txn.TokenAmount;
                    txn.Seller.TokenBalance += txn.TokenAmount;

                    var escrows = await _context.Escrows
                        .Where(e => e.TransactionId == transactionId && e.Status == EscrowStatus.Held)
                        .ToListAsync();

                    foreach (var escrow in escrows)
                    {
                        escrow.Status = EscrowStatus.Released;
                    }

                    txn.TransactionStatus = TransactionStatus.Confirmed;
                    txn.CompletedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                    await dbTransaction.CommitAsync();
                    return (true, "Transaction completed successfully");
                }

                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();
                return (true, "Confirmation recorded, waiting for other party");
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                _logger.LogError(ex, "Error confirming transaction");
                return (false, "Error confirming transaction");
            }
        }

        public async Task<(bool success, string message)> CancelTransactionAsync(int transactionId, int userId, string reason = null)
        {
            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var txn = await _context.Transactions
                    .Include(t => t.Buyer)
                    .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

                if (txn == null) return (false, "Transaction not found");
                if (txn.TransactionStatus != TransactionStatus.Escrowed) return (false, "Cannot cancel transaction");

                txn.Buyer.EscrowedBalance -= txn.TokenAmount;

                var escrows = await _context.Escrows
                    .Where(e => e.TransactionId == transactionId && e.Status == EscrowStatus.Held)
                    .ToListAsync();

                foreach (var escrow in escrows)
                {
                    escrow.Status = EscrowStatus.Refunded;
                }

                txn.TransactionStatus = TransactionStatus.Cancelled;
                txn.CancelledAt = DateTime.UtcNow;
                txn.CancellationReason = reason;

                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();
                return (true, "Transaction cancelled and tokens refunded");
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                _logger.LogError(ex, "Error cancelling transaction");
                return (false, "Error cancelling transaction");
            }
        }
    }
}
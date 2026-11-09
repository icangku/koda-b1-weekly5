using Ewallet.Models;
using Microsoft.Data.SqlClient;

namespace Ewallet.Interfaces;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, SqlConnection conn, SqlTransaction trans);
    Task<IEnumerable<Transaction>> GetByWalletIdAsync(int walletId, string? filterType = null);
    Task<(decimal Income, decimal Expense)> GetSummaryByWalletIdAsync(int walletId);
}
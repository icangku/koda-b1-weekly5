using Ewallet.Models;
using Microsoft.Data.SqlClient;

namespace Ewallet.Interfaces;

public interface IWalletRepository
{
    Task<Wallet?> GetByUserIdAsync(int userId);
    Task<Wallet?> GetByNumberAsync(string walletNumber, SqlConnection? conn = null, SqlTransaction? trans = null);
    Task<int> CreateAsync(Wallet wallet, SqlConnection conn, SqlTransaction trans);
    Task UpdateBalanceAsync(int walletId, decimal newBalance, SqlConnection conn, SqlTransaction trans);
}
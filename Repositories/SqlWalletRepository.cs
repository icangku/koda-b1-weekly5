using Ewallet.Data;
using Ewallet.Interfaces;
using Ewallet.Models;
using Microsoft.Data.SqlClient;

namespace Ewallet.Repositories;

public class SqlWalletRepository : IWalletRepository
{
    private readonly IDbConnectionFactory _factory;
    public SqlWalletRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task<Wallet?> GetByUserIdAsync(int userId)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();

        string sql = @"
            SELECT w.Id, w.UserId, w.WalletNumber, w.Balance, u.Name 
            FROM Wallets w 
            JOIN Users u ON w.UserId = u.Id 
            WHERE w.UserId = @UserId";

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@UserId", userId);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Wallet
            {
                Id = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                WalletNumber = reader.GetString(2),
                Balance = reader.GetDecimal(3),
                OwnerName = reader.GetString(4)
            };
        }
        return null;
    }

    public async Task<Wallet?> GetByNumberAsync(string walletNumber, SqlConnection? conn = null, SqlTransaction? trans = null)
    {
        bool closeConn = false;
        if (conn == null)
        {
            conn = _factory.CreateConnection();
            await conn.OpenAsync();
            closeConn = true;
        }

        try
        {
            string sql = @"
                SELECT w.Id, w.UserId, w.WalletNumber, w.Balance, u.Name 
                FROM Wallets w 
                JOIN Users u ON w.UserId = u.Id 
                WHERE w.WalletNumber = @WalletNumber";

            using var cmd = new SqlCommand(sql, conn, trans);
            cmd.Parameters.AddWithValue("@WalletNumber", walletNumber);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Wallet
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    WalletNumber = reader.GetString(2),
                    Balance = reader.GetDecimal(3),
                    OwnerName = reader.GetString(4)
                };
            }
            return null;
        }
        finally
        {
            if (closeConn) await conn.DisposeAsync();
        }
    }

    public async Task<int> CreateAsync(Wallet wallet, SqlConnection conn, SqlTransaction trans)
    {
        string sql = @"
            INSERT INTO Wallets (UserId, WalletNumber, Balance) 
            OUTPUT INSERTED.Id 
            VALUES (@UserId, @WalletNumber, @Balance)";

        using var cmd = new SqlCommand(sql, conn, trans);
        cmd.Parameters.AddWithValue("@UserId", wallet.UserId);
        cmd.Parameters.AddWithValue("@WalletNumber", wallet.WalletNumber);
        cmd.Parameters.AddWithValue("@Balance", wallet.Balance);

        return (int)await cmd.ExecuteScalarAsync();
    }

    public async Task UpdateBalanceAsync(int walletId, decimal newBalance, SqlConnection conn, SqlTransaction trans)
    {
        string sql = "UPDATE Wallets SET Balance = @Balance WHERE Id = @Id";
        using var cmd = new SqlCommand(sql, conn, trans);
        cmd.Parameters.AddWithValue("@Balance", newBalance);
        cmd.Parameters.AddWithValue("@Id", walletId);

        await cmd.ExecuteNonQueryAsync();
    }
}
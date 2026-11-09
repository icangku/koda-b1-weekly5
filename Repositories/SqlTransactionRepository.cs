using Ewallet.Data;
using Ewallet.Interfaces;
using Ewallet.Models;
using Microsoft.Data.SqlClient;

namespace Ewallet.Repositories;

public class SqlTransactionRepository : ITransactionRepository
{
    private readonly IDbConnectionFactory _factory;
    public SqlTransactionRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task AddAsync(Transaction transaction, SqlConnection conn, SqlTransaction trans)
    {
        string sql = @"
            INSERT INTO Transactions (SenderWalletId, ReceiverWalletId, Type, Amount, Description, CreatedAt)
            VALUES (@SenderWalletId, @ReceiverWalletId, @Type, @Amount, @Description, @CreatedAt)";

        using var cmd = new SqlCommand(sql, conn, trans);
        cmd.Parameters.AddWithValue("@SenderWalletId", (object?)transaction.SenderWalletId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ReceiverWalletId", transaction.ReceiverWalletId);
        cmd.Parameters.AddWithValue("@Type", transaction.Type);
        cmd.Parameters.AddWithValue("@Amount", transaction.Amount);
        cmd.Parameters.AddWithValue("@Description", (object?)transaction.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CreatedAt", transaction.CreatedAt);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<Transaction>> GetByWalletIdAsync(int walletId, string? filterType = null)
    {
        var list = new List<Transaction>();
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();

        string sql = @"
            SELECT Id, SenderWalletId, ReceiverWalletId, Type, Amount, Description, CreatedAt 
            FROM Transactions 
            WHERE (SenderWalletId = @WalletId OR ReceiverWalletId = @WalletId)";

        if (filterType == "Income") sql += " AND ReceiverWalletId = @WalletId";
        else if (filterType == "Expense") sql += " AND SenderWalletId = @WalletId";

        sql += " ORDER BY CreatedAt DESC";

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@WalletId", walletId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new Transaction
            {
                Id = reader.GetInt32(0),
                SenderWalletId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                ReceiverWalletId = reader.GetInt32(2),
                Type = reader.GetString(3),
                Amount = reader.GetDecimal(4),
                Description = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                CreatedAt = reader.GetDateTime(6)
            });
        }
        return list;
    }

    public async Task<(decimal Income, decimal Expense)> GetSummaryByWalletIdAsync(int walletId)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();

        string sql = @"
            SELECT 
                ISNULL(SUM(CASE WHEN ReceiverWalletId = @WalletId THEN Amount ELSE 0 END), 0) AS TotalIncome,
                ISNULL(SUM(CASE WHEN SenderWalletId = @WalletId THEN Amount ELSE 0 END), 0) AS TotalExpense
            FROM Transactions 
            WHERE SenderWalletId = @WalletId OR ReceiverWalletId = @WalletId";

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@WalletId", walletId);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return (reader.GetDecimal(0), reader.GetDecimal(1));
        }
        return (0, 0);
    }
}
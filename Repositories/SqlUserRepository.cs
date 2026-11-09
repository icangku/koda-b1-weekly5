// Repositories/SqlUserRepository.cs
using Ewallet.Data;
using Ewallet.Interfaces;
using Ewallet.Models;
using Microsoft.Data.SqlClient;

namespace Ewallet.Repositories;

public class SqlUserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _factory;
    public SqlUserRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        var users = new List<User>();
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        
        string sql = @"
            SELECT u.Id, u.Name, u.Email, u.Phone, w.Id AS WalletId, w.WalletNumber, w.Balance 
            FROM Users u 
            LEFT JOIN Wallets w ON u.Id = w.UserId";

        using var cmd = new SqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var user = new User
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Email = reader.GetString(2),
                Phone = reader.GetString(3)
            };
            if (!reader.IsDBNull(4))
            {
                user.Wallet = new Wallet
                {
                    Id = reader.GetInt32(4),
                    UserId = user.Id,
                    WalletNumber = reader.GetString(5),
                    Balance = reader.GetDecimal(6)
                };
            }
            users.Add(user);
        }
        return users;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();

        string sql = @"
            SELECT u.Id, u.Name, u.Email, u.Phone, w.Id AS WalletId, w.WalletNumber, w.Balance 
            FROM Users u 
            LEFT JOIN Wallets w ON u.Id = w.UserId 
            WHERE u.Id = @Id";

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var user = new User
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Email = reader.GetString(2),
                Phone = reader.GetString(3)
            };
            if (!reader.IsDBNull(4))
            {
                user.Wallet = new Wallet
                {
                    Id = reader.GetInt32(4),
                    UserId = user.Id,
                    WalletNumber = reader.GetString(5),
                    Balance = reader.GetDecimal(6)
                };
            }
            return user;
        }
        return null;
    }

    public async Task<int> CreateAsync(User user, SqlConnection conn, SqlTransaction trans)
    {
        string sql = @"
            INSERT INTO Users (Name, Email, Phone) 
            OUTPUT INSERTED.Id 
            VALUES (@Name, @Email, @Phone)";

        using var cmd = new SqlCommand(sql, conn, trans);
        cmd.Parameters.AddWithValue("@Name", user.Name);
        cmd.Parameters.AddWithValue("@Email", user.Email);
        cmd.Parameters.AddWithValue("@Phone", user.Phone);

        return (int)await cmd.ExecuteScalarAsync();
    }

    public async Task<bool> UpdateAsync(User user)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();

        string sql = "UPDATE Users SET Name = @Name, Email = @Email, Phone = @Phone WHERE Id = @Id";
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Name", user.Name);
        cmd.Parameters.AddWithValue("@Email", user.Email);
        cmd.Parameters.AddWithValue("@Phone", user.Phone);
        cmd.Parameters.AddWithValue("@Id", user.Id);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }
}
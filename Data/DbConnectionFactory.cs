using Microsoft.Data.SqlClient;

namespace Ewallet.Data;

public interface IDbConnectionFactory
{
    SqlConnection CreateConnection();
}

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default") 
            ?? throw new InvalidOperationException("ConnectionString 'DefaultConnection' tidak ditemukan.");
    }

    public SqlConnection CreateConnection() => new SqlConnection(_connectionString);
}
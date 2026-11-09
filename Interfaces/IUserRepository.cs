using Ewallet.Models;
using Microsoft.Data.SqlClient;

namespace Ewallet.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<int> CreateAsync(User user, SqlConnection conn, SqlTransaction trans);
    Task<bool> UpdateAsync(User user);
}
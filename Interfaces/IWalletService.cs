using Ewallet.Dtos;

namespace Ewallet.Interfaces;

public interface IWalletService
{
    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
    Task<UserResponseDto> GetUserByIdAsync(int id);
    Task<UserResponseDto> CreateUserAsync(CreateUserDto dto);
    Task<bool> UpdateUserAsync(int id, UpdateUserDto dto);

    Task<WalletDashboardDto> GetWalletDashboardAsync(int userId);
    Task TopUpAsync(int userId, decimal amount);
    Task TransferAsync(TransferRequestDto dto);
    Task<IEnumerable<TransactionHistoryDto>> GetTransactionHistoryAsync(int userId, string? filter = null);
}
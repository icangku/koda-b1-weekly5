using System.ComponentModel.DataAnnotations;

namespace Ewallet.Dtos;

public record CreateUserDto(
    [Required, StringLength(100)] string Name,
    [Required, EmailAddress] string Email,
    [Required, Phone] string Phone
);

public record UpdateUserDto(
    [Required, StringLength(100)] string Name,
    [Required, EmailAddress] string Email,
    [Required, Phone] string Phone
);

public record UserResponseDto(int Id, string Name, string Email, string Phone, string? WalletNumber, decimal Balance);

public record TopUpRequestDto(
    [Range(0.01, double.MaxValue, ErrorMessage = "Nominal top up harus lebih besar dari 0.")] decimal Amount
);

public record TransferRequestDto(
    [Required] string SenderWalletNumber,
    [Required] string ReceiverWalletNumber,
    [Range(0.01, double.MaxValue, ErrorMessage = "Nominal transfer harus lebih besar dari 0.")] decimal Amount,
    string Description
);

public record WalletDashboardDto(
    int UserId,
    string OwnerName,
    string WalletNumber,
    decimal Balance,
    decimal TotalIncome,
    decimal TotalExpense
);

public record TransactionHistoryDto(
    int Id,
    string Type,
    string? SenderWalletNumber,
    string ReceiverWalletNumber,
    decimal Amount,
    string Description,
    DateTime CreatedAt
);
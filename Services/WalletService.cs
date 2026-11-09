using Ewallet.Data;
using Ewallet.Dtos;
using Ewallet.Exceptions;
using Ewallet.Interfaces;
using Ewallet.Models;
using Microsoft.Data.SqlClient;

namespace Ewallet.Services;

public class WalletService : IWalletService
{
    private readonly IDbConnectionFactory _factory;
    private readonly IUserRepository _userRepo;
    private readonly IWalletRepository _walletRepo;
    private readonly ITransactionRepository _transactionRepo;

    public WalletService(
        IDbConnectionFactory factory,
        IUserRepository userRepo,
        IWalletRepository walletRepo,
        ITransactionRepository transactionRepo)
    {
        _factory = factory;
        _userRepo = userRepo;
        _walletRepo = walletRepo;
        _transactionRepo = transactionRepo;
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
    {
        var users = await _userRepo.GetAllAsync();
        return users.Select(u => new UserResponseDto(
            u.Id, u.Name, u.Email, u.Phone, u.Wallet?.WalletNumber, u.Wallet?.Balance ?? 0
        ));
    }

    public async Task<UserResponseDto> GetUserByIdAsync(int id)
    {
        var u = await _userRepo.GetByIdAsync(id) ?? throw new UserNotFoundException($"User ID {id} tidak ditemukan.");
        return new UserResponseDto(u.Id, u.Name, u.Email, u.Phone, u.Wallet?.WalletNumber, u.Wallet?.Balance ?? 0);
    }

    public async Task<UserResponseDto> CreateUserAsync(CreateUserDto dto)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var trans = conn.BeginTransaction();

        try
        {
            var user = new User { Name = dto.Name, Email = dto.Email, Phone = dto.Phone };
            int userId = await _userRepo.CreateAsync(user, conn, trans);

            string walletNumber = $"WAL{userId:D3}";
            var wallet = new Wallet { UserId = userId, WalletNumber = walletNumber, Balance = 0.00m };
            await _walletRepo.CreateAsync(wallet, conn, trans);

            trans.Commit();
            return new UserResponseDto(userId, dto.Name, dto.Email, dto.Phone, walletNumber, 0.00m);
        }
        catch
        {
            trans.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateUserAsync(int id, UpdateUserDto dto)
    {
        var user = await _userRepo.GetByIdAsync(id) ?? throw new UserNotFoundException($"User ID {id} tidak ditemukan.");
        user.Name = dto.Name;
        user.Email = dto.Email;
        user.Phone = dto.Phone;
        return await _userRepo.UpdateAsync(user);
    }

    public async Task<WalletDashboardDto> GetWalletDashboardAsync(int userId)
    {
        var wallet = await _walletRepo.GetByUserIdAsync(userId) 
            ?? throw new WalletNotFoundException($"Wallet untuk User ID {userId} tidak ditemukan.");

        var (income, expense) = await _transactionRepo.GetSummaryByWalletIdAsync(wallet.Id);

        return new WalletDashboardDto(
            userId,
            wallet.OwnerName ?? "User",
            wallet.WalletNumber,
            wallet.Balance,
            income,
            expense
        );
    }

    public async Task TopUpAsync(int userId, decimal amount)
    {
        if (amount <= 0) throw new InvalidAmountException();

        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var trans = conn.BeginTransaction();

        try
        {
            var wallet = await _walletRepo.GetByUserIdAsync(userId) 
                ?? throw new WalletNotFoundException();

            decimal newBalance = wallet.Balance + amount;
            await _walletRepo.UpdateBalanceAsync(wallet.Id, newBalance, conn, trans);

            var transaction = new Transaction
            {
                SenderWalletId = null,
                ReceiverWalletId = wallet.Id,
                Type = "TopUp",
                Amount = amount,
                Description = "Top Up Saldo",
                CreatedAt = DateTime.UtcNow
            };
            await _transactionRepo.AddAsync(transaction, conn, trans);

            trans.Commit();
        }
        catch
        {
            trans.Rollback();
            throw;
        }
    }

    public async Task TransferAsync(TransferRequestDto dto)
    {
        if (dto.Amount <= 0) throw new InvalidAmountException();
        if (dto.SenderWalletNumber == dto.ReceiverWalletNumber) throw new SelfTransferException();

        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var trans = conn.BeginTransaction();

        try
        {
            var sender = await _walletRepo.GetByNumberAsync(dto.SenderWalletNumber, conn, trans)
                ?? throw new WalletNotFoundException($"Wallet pengirim '{dto.SenderWalletNumber}' tidak ditemukan.");

            var receiver = await _walletRepo.GetByNumberAsync(dto.ReceiverWalletNumber, conn, trans)
                ?? throw new WalletNotFoundException($"Wallet penerima '{dto.ReceiverWalletNumber}' tidak ditemukan.");

            if (sender.Balance < dto.Amount) throw new InsufficientBalanceException();

            await _walletRepo.UpdateBalanceAsync(sender.Id, sender.Balance - dto.Amount, conn, trans);
            await _walletRepo.UpdateBalanceAsync(receiver.Id, receiver.Balance + dto.Amount, conn, trans);

            var transaction = new Transaction
            {
                SenderWalletId = sender.Id,
                ReceiverWalletId = receiver.Id,
                Type = "Transfer",
                Amount = dto.Amount,
                Description = string.IsNullOrWhiteSpace(dto.Description) ? "Transfer Dana" : dto.Description,
                CreatedAt = DateTime.UtcNow
            };
            await _transactionRepo.AddAsync(transaction, conn, trans);

            trans.Commit();
        }
        catch
        {
            trans.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<TransactionHistoryDto>> GetTransactionHistoryAsync(int userId, string? filter = null)
    {
        var wallet = await _walletRepo.GetByUserIdAsync(userId) 
            ?? throw new WalletNotFoundException();

        var list = await _transactionRepo.GetByWalletIdAsync(wallet.Id, filter);
        var allUsers = await _userRepo.GetAllAsync();
        var walletLookup = allUsers.Where(u => u.Wallet != null).ToDictionary(u => u.Wallet!.Id, u => u.Wallet!.WalletNumber);

        return list.Select(t => new TransactionHistoryDto(
            t.Id,
            t.Type,
            t.SenderWalletId.HasValue ? walletLookup.GetValueOrDefault(t.SenderWalletId.Value, "-") : "-",
            walletLookup.GetValueOrDefault(t.ReceiverWalletId, "-"),
            t.Amount,
            t.Description,
            t.CreatedAt
        ));
    }
}
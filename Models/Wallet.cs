namespace Ewallet.Models;

public class Wallet
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string WalletNumber { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string? OwnerName { get; set; }
}
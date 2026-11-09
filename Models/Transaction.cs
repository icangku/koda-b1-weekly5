namespace Ewallet.Models;

public class Transaction
{
    public int Id { get; set; }
    public int? SenderWalletId { get; set; }
    public int ReceiverWalletId { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
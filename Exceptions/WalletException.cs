namespace Ewallet.Exceptions;

public class WalletException : Exception
{
    public WalletException(string message) : base(message) { }
}

public class WalletNotFoundException : WalletException
{
    public WalletNotFoundException(string message = "Wallet tidak ditemukan.") : base(message) { }
}

public class UserNotFoundException : WalletException
{
    public UserNotFoundException(string message = "User tidak ditemukan.") : base(message) { }
}

public class InsufficientBalanceException : WalletException
{
    public InsufficientBalanceException(string message = "Saldo tidak mencukupi.") : base(message) { }
}

public class InvalidAmountException : WalletException
{
    public InvalidAmountException(string message = "Nominal harus lebih besar dari 0.") : base(message) { }
}

public class SelfTransferException : WalletException
{
    public SelfTransferException(string message = "Tidak dapat melakukan transfer ke wallet sendiri.") : base(message) { }
}
using Ewallet.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ewallet.Controllers.Mvc;

public class TransactionsController : Controller
{
    private readonly IWalletService _walletService;
    public TransactionsController(IWalletService walletService) => _walletService = walletService;

    public async Task<IActionResult> Index(int id, string? filter)
    {
        var list = await _walletService.GetTransactionHistoryAsync(id, filter);
        ViewBag.UserId = id;
        ViewBag.CurrentFilter = filter;
        return View(list);
    }
}
using Ewallet.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ewallet.Controllers.Mvc;

public class HomeController : Controller
{
    private readonly IWalletService _walletService;
    public HomeController(IWalletService walletService) => _walletService = walletService;

    public async Task<IActionResult> Index()
    {
        var users = await _walletService.GetAllUsersAsync();
        return View(users);
    }
}
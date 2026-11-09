using Ewallet.Dtos;
using Ewallet.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ewallet.Controllers.Mvc;

public class WalletsController : Controller
{
    private readonly IWalletService _walletService;
    public WalletsController(IWalletService walletService) => _walletService = walletService;

    public async Task<IActionResult> Dashboard(int id)
    {
        try
        {
            var data = await _walletService.GetWalletDashboardAsync(id);
            return View(data);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Index", "Home");
        }
    }

    public async Task<IActionResult> TopUp(int id)
    {
        var data = await _walletService.GetWalletDashboardAsync(id);
        ViewBag.UserId = id;
        ViewBag.WalletNumber = data.WalletNumber;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> TopUp(int id, decimal amount)
    {
        try
        {
            await _walletService.TopUpAsync(id, amount);
            TempData["Success"] = "Top Up Berhasil!";
            return RedirectToAction("Dashboard", new { id });
        }
        catch (Exception ex)
        {
            ViewBag.UserId = id;
            ViewBag.Error = ex.Message;
            return View();
        }
    }

    public async Task<IActionResult> Transfer(int id)
    {
        var data = await _walletService.GetWalletDashboardAsync(id);
        ViewBag.UserId = id;
        ViewBag.SenderWalletNumber = data.WalletNumber;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Transfer(int id, string receiverWalletNumber, decimal amount, string description)
    {
        var data = await _walletService.GetWalletDashboardAsync(id);
        try
        {
            var dto = new TransferRequestDto(data.WalletNumber, receiverWalletNumber, amount, description);
            await _walletService.TransferAsync(dto);
            TempData["Success"] = "Transfer Berhasil!";
            return RedirectToAction("Dashboard", new { id });
        }
        catch (Exception ex)
        {
            ViewBag.UserId = id;
            ViewBag.SenderWalletNumber = data.WalletNumber;
            ViewBag.Error = ex.Message;
            return View();
        }
    }
}
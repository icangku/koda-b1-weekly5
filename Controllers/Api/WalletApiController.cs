using Ewallet.Dtos;
using Ewallet.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ewallet.Controllers.Api;

[ApiController]
[Route("api/wallets")]
public class WalletsApiController : ControllerBase
{
    private readonly IWalletService _walletService;
    public WalletsApiController(IWalletService walletService) => _walletService = walletService;

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetWallet(int userId) => Ok(await _walletService.GetWalletDashboardAsync(userId));

    [HttpPost("{userId}/topups")]
    public async Task<IActionResult> TopUp(int userId, [FromBody] TopUpRequestDto dto)
    {
        await _walletService.TopUpAsync(userId, dto.Amount);
        return Ok(new { message = "Top Up Berhasil" });
    }
}
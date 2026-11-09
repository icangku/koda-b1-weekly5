using Ewallet.Dtos;
using Ewallet.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ewallet.Controllers.Api;

[ApiController]
[Route("api/transfers")]
public class TransfersApiController : ControllerBase
{
    private readonly IWalletService _walletService;
    public TransfersApiController(IWalletService walletService) => _walletService = walletService;

    [HttpPost]
    public async Task<IActionResult> Transfer([FromBody] TransferRequestDto dto)
    {
        await _walletService.TransferAsync(dto);
        return Ok(new { message = "Transfer Berhasil" });
    }
}
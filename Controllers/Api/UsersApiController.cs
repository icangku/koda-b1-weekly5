using Ewallet.Dtos;
using Ewallet.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ewallet.Controllers.Api;

[ApiController]
[Route("api/users")]
public class UsersApiController : ControllerBase
{
    private readonly IWalletService _walletService;
    public UsersApiController(IWalletService walletService) => _walletService = walletService;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _walletService.GetAllUsersAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) => Ok(await _walletService.GetUserByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var created = await _walletService.CreateUserAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        await _walletService.UpdateUserAsync(id, dto);
        return NoContent();
    }

    [HttpGet("{id}/transactions")]
    public async Task<IActionResult> GetTransactions(int id, [FromQuery] string? filter)
    {
        return Ok(await _walletService.GetTransactionHistoryAsync(id, filter));
    }
}
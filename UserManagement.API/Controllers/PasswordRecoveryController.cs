using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.Features.Users.Commands;
using UserManagement.Application.Services;

namespace UserManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PasswordRecoveryController : ControllerBase
{
    private readonly PasswordRecoveryService passwordRecoveryService;

    public PasswordRecoveryController(PasswordRecoveryService passwordRecoveryService)
    {
        this.passwordRecoveryService = passwordRecoveryService;
    }

    [HttpPost("request-recovery")]
    public async Task<IActionResult> RequestPasswordRecovery([FromBody] PasswordRecoveryRequest request)
    {
        try
        {
            await passwordRecoveryService.SendPasswordRecoveryEmailAsync(request.Email);
            return Ok("Password recovery email sent.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }
}
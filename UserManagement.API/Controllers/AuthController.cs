using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.Dtos;
using UserManagement.Application.Features.Users.Commands;

namespace UserManagement.API.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IMediator mediator;

    public AuthController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthDto authDto)
    {
        var command = new LoginUserCommand
        {
            Email = authDto.Email,
            Password = authDto.Password,
        };

        var token = await mediator.Send(command);
        return Ok(new { Token = token });
    }

}
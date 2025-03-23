using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.Features.Users.Commands;
using UserManagement.Application.Features.Users.Queries;

namespace UserManagement.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator mediator;

    public UsersController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await mediator.Send(new GetAllUsersQuery());
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await mediator.Send(new GetUserByIdQuery(id));
        return Ok(user);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        var result = await mediator.Send(command);
        return CreatedAtAction(nameof(GetUserById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("User ID mismatch.");
        }

        var result = await mediator.Send(command);
        return result ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var result = await mediator.Send(new DeleteUserCommand(id));
        return result ? NoContent() : NotFound();
    }
}
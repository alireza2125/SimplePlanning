using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

using SimplePlanning.Server.Data;
using SimplePlanning.Server.Services;
using SimplePlanning.Shared.Models;

namespace SimplePlanning.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class IdentityController : ControllerBase
{
    private readonly DataContext _dataContext;
    private readonly IdentityService _identityService;

    public IdentityController(DataContext dataContext, IdentityService identityService)
    {
        _dataContext = dataContext;
        _identityService = identityService;
    }

    [HttpGet]
    public ActionResult<UserModel> Check() =>
        _identityService.User is { } user ? user : Unauthorized();

    [HttpPost]
    public async ValueTask<ActionResult<UserModel>> SignupAsync(IdentityLoginRequest model,
        CancellationToken cancellationToken = default)
    {
        if (await _dataContext.Users.AnyAsync(x => x.Email == model.Email, cancellationToken).ConfigureAwait(false))
        {
            ModelState.AddModelError<IdentityLoginRequest>(x => x.Email, "Email exist");
            return BadRequest(ModelState);
        }

        UserModel entity = new()
        {
            Email = model.Email, Password = model.Password
        };
        await _dataContext.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await _dataContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(Check), entity);
    }
}

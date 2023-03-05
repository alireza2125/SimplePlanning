using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using SimplePlanning.Server.Data;
using SimplePlanning.Shared.Models;

namespace SimplePlanning.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly DataContext _dataContext;

    public UsersController(DataContext dataContext) => _dataContext = dataContext;

    [HttpGet("[action]")]
    public IAsyncEnumerable<UserModel> FindAsync(string email) =>
        _dataContext.Users.AsNoTracking().Where(x => x.Email.Contains(email)).Take(10).AsAsyncEnumerable();

    [HttpGet("{id:guid}")]
    public async ValueTask<ActionResult<UserModel?>> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dataContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);

    [HttpGet("{email}")]
    public async ValueTask<ActionResult<UserModel?>> GetAsync(string email,
        CancellationToken cancellationToken = default) =>
        await _dataContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken)
            .ConfigureAwait(false);
}

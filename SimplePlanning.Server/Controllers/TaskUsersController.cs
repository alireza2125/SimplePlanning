using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using SimplePlanning.Server.Data;
using SimplePlanning.Shared.Models;

namespace SimplePlanning.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TaskUsersController : ControllerBase
{
    private readonly DataContext _dataContext;

    public TaskUsersController(DataContext dataContext) => _dataContext = dataContext;

    [HttpGet("{taskId:guid}")]
    public IAsyncEnumerable<TaskUserModel> GetAsync(Guid taskId) =>
        _dataContext.TaskUsers.AsNoTracking().Where(x => x.TaskId == taskId).AsAsyncEnumerable();

    [HttpPost("[action]")]
    public async ValueTask<IActionResult> AddAsync(TaskUserModel taskUser,
        CancellationToken cancellationToken = default)
    {
        if (await _dataContext.TaskUsers
                .AnyAsync(x =>
                        x.TaskId == taskUser.TaskId && x.UserId == taskUser.UserId,
                    cancellationToken)
                .ConfigureAwait(false))
        {
            return BadRequest();
        }

        if (!await _dataContext.Tasks
                .AnyAsync(x => x.Id == taskUser.TaskId, cancellationToken)
                .ConfigureAwait(false))
        {
            return BadRequest();
        }

        if (!await _dataContext.Users
                .AnyAsync(x =>
                        x.Id == taskUser.UserId,
                    cancellationToken)
                .ConfigureAwait(false))
        {
            return BadRequest();
        }

        await _dataContext.AddAsync(taskUser, cancellationToken).ConfigureAwait(false);
        await _dataContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("[action]")]
    public async ValueTask<IActionResult> DeleteAsync(TaskUserModel taskUser,
        CancellationToken cancellationToken = default)
    {
        var taskModel = await _dataContext.TaskUsers
            .FirstOrDefaultAsync(x =>
                    x.TaskId == taskUser.TaskId && x.UserId == taskUser.UserId,
                cancellationToken)
            .ConfigureAwait(false);
        if (taskModel == null)
        {
            return NotFound();
        }

        _dataContext.Remove(taskModel);
        await _dataContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return NoContent();
    }
}

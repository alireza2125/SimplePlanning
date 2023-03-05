using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using SimplePlanning.Server.Data;
using SimplePlanning.Shared.Models;

namespace SimplePlanning.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly DataContext _dataContext;

    public TasksController(DataContext context) => _dataContext = context;

    [HttpGet]
    public IAsyncEnumerable<TaskModel> GetAllAsync(Guid projectId) =>
        _dataContext.Tasks.Where(x => x.ProjectId == projectId).AsNoTracking().AsAsyncEnumerable();

    [HttpGet("{id:guid}")]
    public async ValueTask<ActionResult<TaskModel>> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dataContext.Tasks.FindAsync(new object[]
                {
                    id
                },
                cancellationToken)
            .ConfigureAwait(false) is { } task
            ? task
            : NotFound();

    [HttpPut]
    public async ValueTask<IActionResult> UpdateAsync(TaskModel taskModel)
    {
        _dataContext.Entry(taskModel).State = EntityState.Modified;

        try
        {
            await _dataContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _dataContext.Tasks.AnyAsync(e => e.Id == taskModel.Id).ConfigureAwait(false))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [HttpPost]
    public async ValueTask<ActionResult<Guid>> AddAsync(TaskModel taskModel)
    {
        if (!await _dataContext.Projects.AnyAsync(x => x.Id == taskModel.ProjectId).ConfigureAwait(false))
        {
            return BadRequest();
        }

        await _dataContext.Tasks.AddAsync(taskModel).ConfigureAwait(false);
        await _dataContext.SaveChangesAsync();

        return taskModel.Id;
    }

    [HttpDelete("{id:guid}")]
    public async ValueTask<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var taskModel = await _dataContext.Tasks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using SimplePlanning.Server.Data;
using SimplePlanning.Shared.Models;

namespace SimplePlanning.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly DataContext _context;

    public ProjectsController(DataContext context) =>
        _context = context;

    [HttpGet]
    public IAsyncEnumerable<ProjectModel> GetAsync() =>
        _context.Projects.AsNoTracking().AsAsyncEnumerable();

    [HttpGet("{id}")]
    public async ValueTask<ActionResult<ProjectModel>> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var projectModel = await _context.Projects.FindAsync(new object[]
                {
                    id
                },
                cancellationToken)
            .ConfigureAwait(false);
        return projectModel == null ? NotFound() : projectModel;
    }

    [HttpPut]
    public async ValueTask<IActionResult> UpdateAsync(ProjectModel projectModel,
        CancellationToken cancellationToken = default)
    {
        _context.Entry(projectModel).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Projects.AnyAsync(e => e.Id == projectModel.Id, cancellationToken)
                    .ConfigureAwait(false))
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

    // POST: api/Projects
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async ValueTask<ActionResult<Guid>> CreateAsync(ProjectModel projectModel,
        CancellationToken cancellationToken = default)
    {
        _context.Projects.Add(projectModel);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return projectModel.Id;
    }

    // DELETE: api/Projects/5
    [HttpDelete("{id:guid}")]
    public async ValueTask<IActionResult> DeleteAsync(Guid id)
    {
        var projectModel = await _context.Projects.FindAsync(id);
        if (projectModel == null)
        {
            return NotFound();
        }

        _context.Projects.Remove(projectModel);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        return NoContent();
    }
}

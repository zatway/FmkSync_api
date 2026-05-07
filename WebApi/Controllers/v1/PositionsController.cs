using Application.Interfaces;
using Application.Common;
using Application.Organization.Commands.DeletePosition;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Controllers.v1;

[ApiController]
[Route("api/v1/positions")]
public class PositionsController(IKomSyncContext context, IMediator mediator) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? departmentId, CancellationToken cancellationToken)
    {
        var q = context.Positions.AsNoTracking();
        if (departmentId.HasValue)
            q = q.Where(p => p.DepartmentId == departmentId.Value);

        var items = await q
            .OrderBy(p => p.Name)
            .Select(p => new { id = p.Id, name = p.Name, departmentId = p.DepartmentId })
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    public record CreatePositionBody(string Name, Guid DepartmentId);

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePositionBody body, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(body.Name))
            return BadRequest("Укажите название");
        if (SystemAdminProtection.IsProtectedPositionName(body.Name))
            return BadRequest("Системная должность недоступна для создания/изменения.");
        var depExists = await context.Departments.AnyAsync(d => d.Id == body.DepartmentId, cancellationToken);
        if (!depExists)
            return BadRequest("Подразделение не найдено");
        if (await SystemAdminProtection.IsProtectedDepartmentIdAsync(context, body.DepartmentId, cancellationToken))
            return BadRequest("Нельзя добавлять должности в системное подразделение.");

        var entity = new Position
        {
            Name = body.Name.Trim(),
            DepartmentId = body.DepartmentId
        };
        context.Positions.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
        return Ok(new { id = entity.Id, name = entity.Name, departmentId = entity.DepartmentId });
    }

    public record DeletePositionBody(Guid? ReassignToPositionId = null, bool DeleteAllUsers = false);

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromBody] DeletePositionBody? body,
        CancellationToken cancellationToken)
    {
        var ok = await mediator.Send(
            new DeletePositionCommand(id, body?.ReassignToPositionId, body?.DeleteAllUsers ?? false),
            cancellationToken);
        return ok ? NoContent() : NotFound();
    }
}

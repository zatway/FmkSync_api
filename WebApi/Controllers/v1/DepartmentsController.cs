using Application.Interfaces;
using Application.Common;
using Application.Organization.Commands.DeleteDepartment;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Controllers.v1;

[ApiController]
[Route("api/v1/departments")]
public class DepartmentsController(IKomSyncContext context, IMediator mediator) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var items = await context.Departments
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .Select(d => new { id = d.Id, name = d.Name })
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    public record CreateDepartmentBody(string Name);

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentBody body, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(body.Name))
            return BadRequest("Укажите название");
        if (SystemAdminProtection.IsProtectedDepartmentName(body.Name))
            return BadRequest("Системное подразделение недоступно для создания/изменения.");

        var entity = new Department { Name = body.Name.Trim() };
        context.Departments.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
        return Ok(new { id = entity.Id, name = entity.Name });
    }

    public record DeleteDepartmentBody(
        Guid? ReassignToDepartmentId = null,
        Guid? PositionIdForReassignedUsers = null,
        bool DeleteAllUsers = false);

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromBody] DeleteDepartmentBody? body,
        CancellationToken cancellationToken)
    {
        var ok = await mediator.Send(
            new DeleteDepartmentCommand(
                id,
                body?.ReassignToDepartmentId,
                body?.PositionIdForReassignedUsers,
                body?.DeleteAllUsers ?? false),
            cancellationToken);
        return ok ? NoContent() : NotFound();
    }
}

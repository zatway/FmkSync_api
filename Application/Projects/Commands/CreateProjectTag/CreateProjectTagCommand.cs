using Application.DTO.Projects;
using MediatR;

namespace Application.Projects.Commands.CreateProjectTag;

public record CreateProjectTagCommand(Guid ProjectId, string Name) : IRequest<ProjectTagDto>;

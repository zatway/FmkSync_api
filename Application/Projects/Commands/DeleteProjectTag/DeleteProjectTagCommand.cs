using MediatR;

namespace Application.Projects.Commands.DeleteProjectTag;

public record DeleteProjectTagCommand(Guid ProjectId, Guid TagId) : IRequest<bool>;

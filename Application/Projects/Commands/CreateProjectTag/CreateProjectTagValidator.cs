using FluentValidation;

namespace Application.Projects.Commands.CreateProjectTag;

public class CreateProjectTagValidator : AbstractValidator<CreateProjectTagCommand>
{
    public CreateProjectTagValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(80);
    }
}

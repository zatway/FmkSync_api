using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities.Common;

namespace Domain.Entities;

public class Tag : BaseEntity
{
    /// <summary>
    /// Название тега (уникально в рамках проекта).
    /// </summary>
    [Required, MaxLength(80)]
    public string Name { get; set; } = null!;

    public Guid ProjectId { get; set; }

    [ForeignKey(nameof(ProjectId))]
    public Project Project { get; set; } = null!;

    public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
}
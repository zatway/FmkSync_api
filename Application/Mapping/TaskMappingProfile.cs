using Application.DTO.TaskComment;
using Application.DTO.TaskHistory;
using Application.DTO.Tasks;
using Application.DTO.Attachments;
using Application.Common;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapping;

public class TaskMappingProfile : AutoMapper.Profile
{
    public TaskMappingProfile()
    {
        CreateMap<CreateTaskRequest, ProjectTask>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatorId, opt => opt.Ignore())
            .ForMember(d => d.History, opt => opt.Ignore())
            .ForMember(d => d.Comments, opt => opt.Ignore())
            .ForMember(d => d.Watchers, opt => opt.Ignore())
            .ForMember(d => d.TaskNumber, opt => opt.Ignore())
            .ForMember(d => d.SortOrder, opt => opt.Ignore())
            .ForMember(d => d.Project, opt => opt.Ignore())
            .ForMember(d => d.Assignee, opt => opt.Ignore())
            .ForMember(d => d.Responsible, opt => opt.Ignore())
            .ForMember(d => d.Creator, opt => opt.Ignore())
            .ForMember(d => d.ParentTask, opt => opt.Ignore())
            .ForMember(d => d.SubTasks, opt => opt.Ignore())
            .ForMember(d => d.StatusColumn, opt => opt.Ignore());

        CreateMap<UpdateTaskRequest, ProjectTask>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Watchers, opt => opt.Ignore())
            .ForMember(dest => dest.AssigneeId, opt => opt.Ignore())
            .ForMember(dest => dest.StatusColumn, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((_, _, srcMember) => srcMember != null));

        CreateMap<AssignUserRequest, ProjectTask>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((_, _, srcMember) => srcMember != null));

        CreateMap<TaskComment, TaskCommentDto>()
            .ForMember(d => d.AuthorName, opt => opt.MapFrom(s => s.User != null ? s.User.FullName : "—"))
            .ForMember(d => d.AuthorHasAvatar, opt => opt.MapFrom(s => s.User != null && s.User.Avatar != null))
            .ForMember(d => d.Attachments, opt => opt.Ignore())
            .ForMember(d => d.Replies, opt => opt.Ignore());

        CreateMap<TaskCommentAttachment, CommentAttachmentDto>()
            .ConstructUsing(a => new CommentAttachmentDto(
                FileIdCodec.TaskCommentAttachment(a.Id),
                a.FileName,
                a.ContentType,
                a.SizeBytes,
                a.CreatedAt
            ));

        CreateMap<TaskHistory, TaskHistoryDto>()
            .ForMember(d => d.ChangedByName, opt => opt.MapFrom(s =>
                s.ChangedByDisplayName
                ?? (s.ChangedBy != null ? s.ChangedBy.FullName : null)))
            .ForMember(d => d.ChangedByHasAvatar, opt => opt.MapFrom(s =>
                s.ChangedBy != null && s.ChangedBy.Avatar != null));

        CreateMap<User, TaskAssigneeDto>()
            .ConstructUsing(u => new TaskAssigneeDto(u.Id, u.FullName, null, u.Avatar != null));

        CreateMap<ProjectTaskStatusColumn, TaskStatusColumnDto>();

        CreateMap<ProjectTask, TaskShortDto>()
            .ForMember(d => d.Key, opt => opt.MapFrom(s =>
                s.Project != null ? $"{s.Project.Key}-{s.TaskNumber}" : s.TaskNumber.ToString()))
            .ForMember(d => d.Assignee, opt => opt.MapFrom(s => s.Assignee))
            .ForMember(d => d.Responsible, opt => opt.MapFrom(s => s.Responsible))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.StatusColumn));

        CreateMap<ProjectTask, TaskDetailedDto>()
            .ForMember(d => d.Key, opt => opt.Ignore())
            .ForMember(d => d.Comments, opt => opt.Ignore())
            .ForMember(d => d.History, opt => opt.Ignore())
            .ForMember(d => d.Watchers, opt => opt.Ignore())
            .ForMember(d => d.FileAttachments, opt => opt.Ignore())
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.StatusColumn))
            .ForMember(d => d.Creator, opt => opt.MapFrom(s => s.Creator));
    }
}

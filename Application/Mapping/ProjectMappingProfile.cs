using Application.DTO.Projects;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapping
{
    public class ProjectMappingProfile : Profile
    {
        private static string NormalizeHexColorWithHash(string? color)
        {
            if (string.IsNullOrWhiteSpace(color)) return "#0000ff";
            var c = color.Trim();
            if (!c.StartsWith("#", StringComparison.Ordinal)) c = $"#{c}";
            // allow 3-digit shorthand (#abc) -> expand to 6 (#aabbcc)
            if (c.Length == 4)
                c = $"#{c[1]}{c[1]}{c[2]}{c[2]}{c[3]}{c[3]}";
            return c.Length > 7 ? c[..7] : c;
        }

        public ProjectMappingProfile()
        {
            CreateMap<CreateProjectRequest, Project>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => src.Icon ?? ""))
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => NormalizeHexColorWithHash(src.Color)))
                .ForMember(dest => dest.Tags, opt => opt.Ignore());

            CreateMap<UpdateProjectRequest, Project>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => src.Icon ?? ""))
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Color == null ? null : NormalizeHexColorWithHash(src.Color)))
                .ForMember(dest => dest.Tags, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Project, ProjectBriefDto>()
                .ForMember(d => d.OwnerName, opt => opt.MapFrom(s => s.Owner != null ? s.Owner.FullName : "Не назначен"))
                .ForMember(d => d.OwnerHasAvatar, opt => opt.MapFrom(s => s.Owner != null && s.Owner.Avatar != null))
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => src.Icon ?? ""))
                .ForMember(dest => dest.IsArchived, opt => opt.MapFrom(src => src.IsArchived));
            
            CreateMap<Project, MemberDto>();

            CreateMap<ProjectComment, ProjectCommentDto>()
                .ForMember(d => d.Author, opt => opt.MapFrom(s => s.Author))
                .ForMember(d => d.Replies, opt => opt.MapFrom(s => s.Children));

            CreateMap<User, AuthorDto>()
                .ConstructUsing(u => new AuthorDto(u.Id, u.FullName, u.Email, u.Avatar != null));

            CreateMap<ProjectHistory, ProjectHistoryEntryDto>()
                .ConstructUsing(s => new ProjectHistoryEntryDto(
                    s.Id,
                    s.Field,
                    s.OldValue,
                    s.NewValue,
                    s.ChangedBy != null
                        ? new ChangedByDto(s.ChangedBy.Id, s.ChangedBy.FullName, s.ChangedBy.Avatar != null)
                        : new ChangedByDto(
                            Guid.Empty,
                            string.IsNullOrWhiteSpace(s.ChangedByDisplayName) ? "—" : s.ChangedByDisplayName!,
                            false),
                    s.CreatedAt));

            CreateMap<User, ChangedByDto>()
                .ConstructUsing(u => new ChangedByDto(u.Id, u.FullName, u.Avatar != null));
        }
    }
}
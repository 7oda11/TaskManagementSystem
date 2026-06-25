using AutoMapper;
using TaskManagementSystem.Core.Aggregates;
using TaskManagementSystem.Services.DTOs.Auth;
using TaskManagementSystem.Services.DTOs.Task;

namespace TaskManagementSystem.Services.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ── User ───────────────────────────────────────────────
            CreateMap<User, UserProfileDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAT))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

            // ── TaskItem ───────────────────────────────────────────
            CreateMap<TaskItem, TaskItemDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAT))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()));
        }
    }
}

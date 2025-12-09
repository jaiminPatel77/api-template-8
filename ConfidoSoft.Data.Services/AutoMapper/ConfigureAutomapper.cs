using AutoMapper;
using ConfidoSoft.Data.Domain.DBModels;
using ConfidoSoft.Data.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConfidoSoft.Data.Services.AutoMapper
{
    /// <summary>
    /// Auto mapper helper to register mapper for various DBModel to Dto or memory model. 
    /// </summary>
    public static class ConfigureAutomapper
    {
        /// <summary>
        /// Add mapping for models.
        /// </summary>
        /// <param name="cfg"></param>
        public static void ConfigureDBModels(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<User, UserDto>();
            cfg.CreateMap<UserDto, User>();           

            cfg.CreateMap<User, UserDto>()
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.UserProfile.Image))
                .ForMember(dest => dest.Roles, opt => opt.Ignore());
            cfg.CreateMap<UserDto, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email)) //Map email and user name always!
                .ForMember(dest => dest.UserProfile, opt => opt.Ignore()); //Handle it manually
            cfg.CreateMap<User, UserWithPermissionsDto>()
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.UserProfile.Image))
                .ForMember(dest => dest.Roles, opt => opt.Ignore());

            cfg.CreateMap<User, UserLookUpDto>();
            cfg.CreateMap<User, UserListDto>();

            cfg.CreateMap<User, UserProfileDto>();
            cfg.CreateMap<UserProfileDto, User>();            

            cfg.CreateMap<Role, RoleDto>();
            cfg.CreateMap<RoleDto, Role>();

            cfg.CreateMap<PermissionDto, RolePermission>();
            cfg.CreateMap<RolePermission, PermissionDto>();

        }

    }
}

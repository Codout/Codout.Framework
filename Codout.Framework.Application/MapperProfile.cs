using AutoMapper;
using Codout.Framework.Domain;
using Codout.Framework.Dto;

namespace Codout.Framework.Application
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap(typeof(Entity<>), typeof(EntityDto<>)).ReverseMap();
        }
    }
}

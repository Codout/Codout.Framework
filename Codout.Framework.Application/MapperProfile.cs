﻿using AutoMapper;
using Codout.Framework.Api.Client;
using Codout.Framework.Domain;

namespace Codout.Framework.Application;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap(typeof(Entity<>), typeof(EntityDto<>)).ReverseMap();
    }
}
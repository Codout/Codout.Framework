﻿using System;
using System.Linq;

namespace Codout.Framework.Common.Extensions;

public static class Types
{
    public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
    {
        var interfaceTypes = givenType.GetInterfaces();

        if (interfaceTypes.Any(it => it.IsGenericType && it.GetGenericTypeDefinition() == genericType))
            return true;

        if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            return true;

        var baseType = givenType.BaseType;

        return baseType != null && IsAssignableToGenericType(baseType, genericType);
    }
}
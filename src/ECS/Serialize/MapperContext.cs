﻿using Friflo.Json.Fliox.Mapper;

namespace Friflo.Engine.ECS.Serialize;

public class MapperContextEntityStore : IMapperContext
{
    public EntityStore      store;
    public Entity           entity;
    public ComponentType    componentType;
}
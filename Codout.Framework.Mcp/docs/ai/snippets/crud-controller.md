```csharp
public sealed class EntityController
    : CrudController<IEntityRepository, Entity, EntityViewModel>
{
    public EntityController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override async Task BindAsync(EntityViewModel model)
    {
        await base.BindAsync(model);
        // carregar lookups
    }

    protected override Task<EntityViewModel> ToModelAsync(Entity entity)
        => Task.FromResult(new EntityViewModel());

    protected override Task<Entity> ToDomainAsync(EntityViewModel model, Entity entity)
        => Task.FromResult(entity);
}
```
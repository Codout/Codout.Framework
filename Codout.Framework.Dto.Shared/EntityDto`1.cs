namespace Codout.Framework.Api.Client;

public class EntityDto<TId> : IEntityDto<TId>
{
    public TId Id { get; set; }
}
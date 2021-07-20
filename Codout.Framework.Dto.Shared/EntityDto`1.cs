namespace Codout.Framework.Api.Dto
{
    public class EntityDto<TId> : IEntityDto<TId>
    {
        public TId Id { get; set; }
    }
}

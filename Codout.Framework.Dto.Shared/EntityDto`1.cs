namespace Codout.Framework.Dto
{
    public class EntityDto<TId> : IEntityDto<TId>
    {
        public TId Id { get; set; }
    }
}

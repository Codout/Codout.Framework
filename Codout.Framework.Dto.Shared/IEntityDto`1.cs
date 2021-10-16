namespace Codout.Framework.Dto
{
    public interface IEntityDto<TId>
    {
        TId Id { get; set; }
    }
}

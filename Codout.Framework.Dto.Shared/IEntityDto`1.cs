namespace Codout.Framework.Api.Dto
{
    public interface IEntityDto<TId>
    {
        TId Id { get; set; }
    }
}

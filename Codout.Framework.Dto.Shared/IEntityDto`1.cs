namespace Codout.Framework.Api.Client;

public interface IEntityDto<TId>
{
    TId Id { get; set; }
}
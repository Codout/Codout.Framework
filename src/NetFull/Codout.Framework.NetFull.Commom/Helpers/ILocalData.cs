namespace Codout.Framework.NetFull.Commom.Helpers
{
    public interface ILocalData
    {
        object this[object key] { get; set; }
        int Count { get; }
        void Clear();
    }
}
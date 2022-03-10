using Newtonsoft.Json;

namespace Codout.Zenvia.Models
{
    public abstract class BaseObject
    {
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        protected T FromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}

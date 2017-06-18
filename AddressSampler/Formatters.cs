using Newtonsoft.Json;

namespace AddressSampler
{
    public static class Formatters
    {
        public static string AsJson(this object value)
        {
            return JsonConvert.SerializeObject(value, Formatting.Indented);
        }
    }
}
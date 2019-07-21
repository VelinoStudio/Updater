using Newtonsoft.Json;
namespace VelinoStudio.Updater
{
    public class Json
    {
        public static string JsonSerialize(object obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj, Formatting.Indented);
            }
            catch { }
            return null;
        }

        public static T JsonDeserialize<T>(string json) where T : class
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch { }
            return null;
        }
    }
}

using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace WeatherApp.Ui.Tests.BoilerPlate.Json;

public static class JsonConvert
{
    public static string SerializeObject<T>(T objectToSerialize,
        SeriliazationStrategy strategy = SeriliazationStrategy.SystemTextJson)
    {
        switch (strategy)
        {
            case SeriliazationStrategy.NewtonSoft:
                return Newtonsoft.Json.JsonConvert.SerializeObject(objectToSerialize, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented,
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    }
                });
            case SeriliazationStrategy.SystemTextJson:
                try
                {
                    return JsonSerializer.Serialize(objectToSerialize, new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        
                    });
                }
                catch (NotSupportedException e)
                {
                    Console.WriteLine(e);
                    throw;
                }


            default:
                throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
        }
    }

    public static T? DeserializeObject<T>(string objectToDeserialize,
        SeriliazationStrategy strategy = SeriliazationStrategy.SystemTextJson)
    {
        switch (strategy)
        {
            case SeriliazationStrategy.NewtonSoft:
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(objectToDeserialize, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented,
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    }
                });


            case SeriliazationStrategy.SystemTextJson:
                return JsonSerializer.Deserialize<T>(objectToDeserialize, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });
            default:
                throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
        }
    }
}

public enum SeriliazationStrategy
{
    SystemTextJson     ,
    NewtonSoft
}
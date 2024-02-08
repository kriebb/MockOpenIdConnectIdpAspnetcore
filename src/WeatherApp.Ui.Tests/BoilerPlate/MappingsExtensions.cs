using WeatherApp.Ui.Tests.BoilerPlate.Json;
using WireMock.Admin.Mappings;

namespace WeatherApp.Ui.Tests.BoilerPlate;

public static class MappingsExtensions
{
    public static MappingModel ToMapping(this string mappingJsonToLoad)
    {
        return JsonConvert.DeserializeObject<MappingModel>(mappingJsonToLoad, SeriliazationStrategy.NewtonSoft);
    }

    public static IEnumerable<MappingModel> ToMappings(this IEnumerable<string> mappingsJsonToLoad)
    {
        foreach (var mappingJsonToLoad in mappingsJsonToLoad)
        {
            var result =
                JsonConvert.DeserializeObject<MappingModel>(mappingJsonToLoad, SeriliazationStrategy.NewtonSoft);
            if (result != null)
                yield return result;
        }
    }
}
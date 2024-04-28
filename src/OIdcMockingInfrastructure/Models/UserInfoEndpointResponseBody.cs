using System.Text.Json.Serialization;

namespace OIdcMockingInfrastructure.Models;

public sealed record UserInfoEndpointResponseBody(
    [property: JsonPropertyName("@odata.context")] string ODataContext,
    [property: JsonPropertyName("sub")] string Id,
    [property: JsonPropertyName("name")] string DisplayName,
    [property: JsonPropertyName("given_name")] string GivenName,
    [property: JsonPropertyName("family_name")] string Surname,
    [property: JsonPropertyName("preferred_username")] string UserPrincipalName,
    [property: JsonPropertyName("email")] string Mail,
    [property: JsonPropertyName("phone_number")] string? MobilePhone,
    [property: JsonPropertyName("locale")] string? PreferredLanguage
);
using System.Text.Json.Serialization;

namespace OIdcMockingInfrastructure.Models;

//https://learn.microsoft.com/en-us/entra/identity-platform/userinfo
public sealed record UserInfoEndpointResponseBody(
    [property: JsonPropertyName("@odata.context")] string ODataContext,
    [property: JsonPropertyName("businessPhones")] string[] BusinessPhones,
    [property: JsonPropertyName("displayName")] string DisplayName,
    [property: JsonPropertyName("givenName")] string GivenName,
    [property: JsonPropertyName("jobTitle")] string? JobTitle,
    [property: JsonPropertyName("mail")] string Mail,
    [property: JsonPropertyName("mobilePhone")] string? MobilePhone,
    [property: JsonPropertyName("officeLocation")] string? OfficeLocation,
    [property: JsonPropertyName("preferredLanguage")] string? PreferredLanguage,
    [property: JsonPropertyName("surname")] string Surname,
    [property: JsonPropertyName("userPrincipalName")] string UserPrincipalName,
    [property: JsonPropertyName("id")] string Id
);
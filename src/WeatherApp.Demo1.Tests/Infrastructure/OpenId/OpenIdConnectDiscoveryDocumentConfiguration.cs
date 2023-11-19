using System.Text.Json.Serialization;

namespace WeatherApp.Demo2.Tests.Infrastructure.OpenId;

public record OpenIdConnectDiscoveryDocumentConfiguration(
    [property: JsonPropertyName("issuer")] string Issuer, 
    [property: JsonPropertyName("authorization_endpoint")] string AuthorizationEndpoint, 
    [property: JsonPropertyName("token_endpoint")] string TokenEndpoint, 
    [property: JsonPropertyName("device_authorization_endpoint")] string DeviceAuthorizationEndpoint, 
    [property: JsonPropertyName("userinfo_endpoint")] string UserinfoEndpoint, 
    [property: JsonPropertyName("mfa_challenge_endpoint")] string MfaChallengeEndpoint, 
    [property: JsonPropertyName("jwks_uri")] string JwksUri, 
    [property: JsonPropertyName("registration_endpoint")] string RegistrationEndpoint, 
    [property: JsonPropertyName("revocation_endpoint")] string RevocationEndpoint, 
    [property: JsonPropertyName("scopes_supported")] string[] ScopesSupported, 
    [property: JsonPropertyName("response_types_supported")] string[] ResponseTypesSupported, 
    [property: JsonPropertyName("code_challenge_methods_supported")] string[] CodeChallengeMethodsSupported, 
    [property: JsonPropertyName("response_modes_supported")] string[] ResponseModesSupported, 
    [property: JsonPropertyName("subject_types_supported")] string[] SubjectTypesSupported, 
    [property: JsonPropertyName("id_token_signing_alg_values_supported")] string[] IdTokenSigningAlgValuesSupported, 
    [property: JsonPropertyName("token_endpoint_auth_methods_supported")] string[] TokenEndpointAuthMethodsSupported, 
    [property: JsonPropertyName("claims_supported")] string[] ClaimsSupported, 
    [property: JsonPropertyName("request_uri_parameter_supported")] bool RequestUriParameterSupported);
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace WeatherApp.Demo.Tests.Infrastructure.OpenId;

public record OpenIdConnectDiscoveryDocumentConfiguration(
    [property: JsonProperty("issuer")] string Issuer, 
    [property: JsonProperty("authorization_endpoint")] string AuthorizationEndpoint, 
    [property: JsonProperty("token_endpoint")] string TokenEndpoint, 
    [property: JsonProperty("device_authorization_endpoint")] string DeviceAuthorizationEndpoint, 
    [property: JsonProperty("userinfo_endpoint")] string UserinfoEndpoint, 
    [property: JsonProperty("mfa_challenge_endpoint")] string MfaChallengeEndpoint, 
    [property: JsonProperty("jwks_uri")] string JwksUri, 
    [property: JsonProperty("registration_endpoint")] string RegistrationEndpoint, 
    [property: JsonProperty("revocation_endpoint")] string RevocationEndpoint, 
    [property: JsonProperty("scopes_supported")] string[] ScopesSupported, 
    [property: JsonProperty("response_types_supported")] string[] ResponseTypesSupported, 
    [property: JsonProperty("code_challenge_methods_supported")] string[] CodeChallengeMethodsSupported, 
    [property: JsonProperty("response_modes_supported")] string[] ResponseModesSupported, 
    [property: JsonProperty("subject_types_supported")] string[] SubjectTypesSupported, 
    [property: JsonProperty("id_token_signing_alg_values_supported")] string[] IdTokenSigningAlgValuesSupported, 
    [property: JsonProperty("token_endpoint_auth_methods_supported")] string[] TokenEndpointAuthMethodsSupported, 
    [property: JsonProperty("claims_supported")] string[] ClaimsSupported, 
    [property: JsonProperty("request_uri_parameter_supported")] bool RequestUriParameterSupported);
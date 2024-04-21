namespace OIdcMockingInfrastructure.OpenId;

public static class OpenIdConnectDiscoveryDocumentConfigurationFactory
{
    public static OpenIdConnectDiscoveryDocumentConfiguration Create(string issuer)
    {
        var discoveryDocument = new OpenIdConnectDiscoveryDocumentConfiguration(

            Issuer: issuer,
            AuthorizationEndpoint: "http://i.do.not.exist/authorize",
            TokenEndpoint: "http://i.do.not.exist/oauth/token",
            DeviceAuthorizationEndpoint: "http://i.do.not.exist/oauth/device/code",
            UserinfoEndpoint: "https://i.do.not.exist/userinfo",
            MfaChallengeEndpoint: "https://i.do.not.exist/mfa/challenge",
            JwksUri: "https://i.do.not.exist/.well-known/jwks.json",
            RegistrationEndpoint: "https://i.do.not.exist/oidc/register",
            RevocationEndpoint: "https://i.do.not.exist/oauth/revoke",
            ScopesSupported: new[]
            {
                "openid",
                "profile",
                "offline_access",
                "weatherforecast:read"
            },
            ResponseTypesSupported: new[]
            {
                "code",
                "token",
                "id_token",
                "code token",
                "code id_token",
                "token id_token",
                "code token id_token"
            },
            CodeChallengeMethodsSupported: new[]
            {
                "S256",
                "plain"
            },
            ResponseModesSupported: new[]
            {
                "query",
                "fragment",
                "form_post"
            },
            SubjectTypesSupported: new[]
            {
                "public"
            },
            IdTokenSigningAlgValuesSupported: new[]
            {
                "HS256",
                "RS256"
            },
            TokenEndpointAuthMethodsSupported: new[]
            {
                "client_secret_basic",
                "client_secret_post"
            },
            ClaimsSupported: new[]
            {
                "aud",
                "exp",
                "iat",
                "iss",
                "sub",
                "nbf",
                "scope",
                "country"
            },
            RequestUriParameterSupported: false
        );

        return discoveryDocument;
    }
}
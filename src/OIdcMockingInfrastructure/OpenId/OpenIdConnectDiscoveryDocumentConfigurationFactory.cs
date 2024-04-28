namespace OIdcMockingInfrastructure.OpenId;

public static class OpenIdConnectDiscoveryDocumentConfigurationFactory
{
    public static OpenIdConnectDiscoveryDocumentConfiguration Create(string issuer)
    {
        var discoveryDocument = new OpenIdConnectDiscoveryDocumentConfiguration(

            Issuer: issuer,
            AuthorizationEndpoint: Consts.Authority+"/authorize",
            TokenEndpoint: Consts.Authority+"/oauth/token",
            DeviceAuthorizationEndpoint: Consts.Authority+"/oauth/device/code",
            UserinfoEndpoint: Consts.Authority+"/userinfo",
            MfaChallengeEndpoint:Consts.Authority+"/mfa/challenge",
            JwksUri: Consts.Authority+"/.well-known/jwks.json",
            RegistrationEndpoint: Consts.Authority+"/oidc/register",
            RevocationEndpoint: Consts.Authority+"/oauth/revoke",
            ScopesSupported: new[]
            {
                "openid",
                "profile",
                "offline_access",
                "email",
                "concert:ticket:buy"
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
                "country",
                "email"
            },
            RequestUriParameterSupported: false
        );

        return discoveryDocument;
    }
}
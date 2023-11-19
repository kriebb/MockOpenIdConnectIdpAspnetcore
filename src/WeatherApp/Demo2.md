//DEMO 2: Ensure inspection of the content of the authorization Attribute
- Program.cs
- On method OnAddAuthentication

.AddJwtBearer(o =>
{
    o.MapInboundClaims = false;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        NameClaimType = "sub",
    };
});

Run test: Now we see that the Signature fails =>         
Authentication Failed. Result: IDX10500: Signature validation failed. No security keys were provided to validate the signature. Failure:

SignatureValidator = (token, parameters) => new JsonWebToken(token)
Authentication Failed. Result: IDX10253: RequireSignedTokens property on ValidationParameters is set 
=> remove the SignatureValidator and mock the openidprovider

=> Go to the test
Run the test after you added the AddAuthorization and add the Authority attribute

should be
HttpStatusCode.OK
    but was
HttpStatusCode.Forbidden

Let us add that data

Consts.cs
```

    public const string ScopeClaimType = "scope";
    public const string ScopeClaimValidValue = "weatherforecast:read";

    public const string CountryClaimType = "country";
    public const string CountryClaimValidValue = "Belgium";

```

AccessTokenParameters

```

        new(Consts.ScopeClaimType, Consts.ScopeClaimValidValue),
        new(Consts.CountryClaimType,
            Consts.CountryClaimValidValue)

    };


```
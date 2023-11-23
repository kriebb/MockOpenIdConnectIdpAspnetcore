- Step 1 Write the test for a valid accessToken
Explain what the test does:

- we will create default valid accesstokenparameters class
- we will add the accesstoken to the header of the message using the JwtBearerCustomAccessTokenHandler

WeatherForecastControllerTests

  - Use Snippet Demo2_1_Test2

- Step 2 AccessToken Parameters in the test class 

  - Use Snippet Demo2_2_AccessTokenParameters

  - Discuss what parameters are needed for a valid accesstoken:

    - Signing Certificate => valid Certificate
    - Audience: for who is the token
    - Issuer: who gives out the token
    - What does the token of data (sub) = id


- Step 3. ConstClass
 
  - I gather the data in the const class. Lets make sure that the AccessTokenParamaters are filled with correct data.

  - Use Snippet Demo2_3_Const_Basic
 
  - Note that there is a Selfsigned Pem Certificate Factory ready to use. => Infrastructure / Security

  - Let us have a peek into the selfsigned accesstoken pem certificate

- Step 4 Create AccessToken

  - We have all the data to create an accesstoken. Lets add it to the request.

  - You can see the JwtBearerAccessTokenFactory that will create the bearer token, using the accesstokenparameters


- Step 5 Run the test.

  - What the output of the test

        Generated the following encoded accesstoken
        Authentication Failed. Result: IDX10204: Unable to validate issuer. validationParameters.ValidIssuer 

  - Run the first test => No header and still works.

        We miss configuration at the server. Lets configure our application.


- Step 6 Ensure inspection of the content of the authorization Attribute
 
    Use Snippet Demo2_4_Program.AddAuthentication

- Step 7 Run test
  
    Now we see that the Signature fails =>         

      Authentication Failed. Result: IDX10500: Signature validation failed. No security keys were provided to validate the signature. Failure:

     - Use Demo2_5_SignatureValidator

           Authentication Failed. Result: IDX10253: RequireSignedTokens property on ValidationParameters is set 
      
     - Remove the SignatureValidator and mock the openidconfiguration in the test

- Step 8 Specify Public Key
 
      We need to specify the public key so the signature can be validated.

     - In the ServerSetupFixte 

     - Use Demo2_6_PostConfigureJwtBearerOptions

       Go into the ConfigForMockedOpenIdConnectServer class. Discuss
       
       - Will generate the OpenidConfiguration when it is asked
           Using the class MockingOpenIdProviderMessageHandler

     - Use Demo2_7_ConstAddition
        - Define the url where the openidconfiguration can be fetched
        - Define the object that should be returned when the url is called

        - Go into the OpenIdConnectDiscoveryDocumentConfigurationFactory.Create method. => It is there thet the object is returned but ValidCertificate is converted to JWKS when the JWKS endpoint is fetched.


    - After configuraiton is done: 
- Step 9 Run the test again.
= Fails! On the production is not yet an issuer/audience specified!. Lets specify that in the ServerSetupFixture

          Authentication Failed. Result: IDX10208: Unable to validate audience. 

    - Use Demo2_8_DefineConfiguration

- Step 10 Run test again
    Succeeds!


- Step 11 

We are going to add validation on data in the claim. There should be 2 claims in the token:

    - the country and the scope
        - country: the service should only be called from Belgium
        - scope: you need to have access to the get-operation

    - Got to WeatherForecastController.cs
        - Use Demo2_9_AuthorizeBelgium
        - Use Demo2_10_AuathorizeGetOperation
    - Go to Program.cs
        - Use Demo2_11_ProgramAddAuthorization

- Step 12 Does are test still succeed?

should be
HttpStatusCode.OK
    but was
HttpStatusCode.Forbidden

Let us add that data

    - Navigate to Consts.cs
        - Use Demo2_12_ContstsAdditionalData
    - Navigate to AccessTokenParameters
        - Use Demo2_13_AddClaimsToToken

- Step 13: Run the test again

  - Test should succeed
  - Note in reality, you want to write another test, because the requirements changed
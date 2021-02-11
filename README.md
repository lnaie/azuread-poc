
## AzureAD Integration POC

The sample is an integration POC of AzureAD with a blazore app and an asp.net core api.

This is an attempt to configure authentication and authorization middleware in the asp.net core 3.1 API project to be able to authorize users from 2 Identity Services:
* one is the AzureAD
* second one is an Identity Server (IDS) 4/5. 
The plan is to use JWT bearer tokens in both cases to call the API endpoints.

The IDS users are authenticated in a mobile app against the IDS. The AzureAD users are authenticated in an admin SPA against the AzureAD. The web api has public endpoints for IDS users and management endpoints for AzureAD users.

Also note that the mobile and IDS apps are not included.

### AzureAD Application Setup

Login into Azure portal and click on "Azure Active Directory" feature.

From the left menu side, click on "App registrations" and register your app (e.g with a name 'App-MD').
Then click on the app just created and that should take you to the app management page.

Go to "Authentication" and make sure the is a platform defined for SPA with:
* Set Redirect URIs to https://localhost:44358/authentication/login-callback
* Set Front-channel logout URL to https://localhost:44358/authentication/logout
* Implicit grant and hybrid flows: access and id tokens.
* Supported account types: I used single tenant
* Allow public client flows: No

Go to "API permission" and:
* Click btn Add permission, choose Microsoft Graph, then Delegated permissions, then select "openid" from OpenId permissions list, then save.

Go to "Expose an API" and:
* Generate the Application ID URI.
* Create and enable the scope "api.access" that can be consented by Admin and users.

Go to "App roles" and:
* Create admin role with: display name 'App-MD-Admin', allowed member types 'Users/Groups', value 'admin' and enabled.
* Create user role with: display name 'App-MD-User', allowed member types 'Users/Groups', value 'user' and enabled.

Go back to "Azure Active Directory" feature.

From the left menu side, click on "Enterprise applications", and click your app from the list (e.g with a name 'App-MD').

Go to "Properties" and ensure that:
* Enabled for users to sign-in? Yes
* User assignment required? Yes
* Visible to users? Yes

At this point you need to add the users manually to the app for them to be able to login into the ManagementDashboard brazor app with their AzureAD account.
Go to "Users and groups" and:
* Add the users you need and assign them in one of the 2 roles created before: App-MD-Admin, App-MD-User.

### Blazor ManagementDashboard Setup

Setup AzureAD credentials in the Blazor app configuration file: ```ManagementDashboard/wwwroot/appsettings.json```.

Go back to "Azure Active Directory", then choose "App registrations" from left menu, and you'll land in "Overview".
From here you'll need the Tenant and Client IDs.

Go back to Visual Studio, open the file ```appsettings.json``` and:
* Set Authority with ```https://login.microsoftonline.com/[tenant_id]```
* Set ClientId


### Asp.Net Core IdentityApi Setup

Setup AzureAD credentials in the API confiuration file: ```IdentityApi/appsettings.json```.
Configure the same ClientId and TenantId credentials that were used to configure the ManagementDashboard app.



### Test cases (2012-02)

The authentication of ManagementDashboard (MD) to AzureAD works and the token is ```v2```.

The token created by the MD to call API endpoints turns out to be ```v1```. Not sure why.

* One AzureAD middleware in IdentityApi:
  * Using default "Bearer" authentication scheme => WORKS:
    * ```IdentityApi/Startup.cs```, line: ```var azureAdBearerScheme = "Bearer";```
    * ```ManagementDashboard/Infra/ApiAuthorizationMessageHandler.cs```, line: ```var authenticationScheme =  "Bearer";```
  * Using custom "AzureADBearer" authentication scheme => FAILS with 401 in IdentityApi middleware:
    * ```IdentityApi/Startup.cs```, line: ```var azureAdBearerScheme = "AzureADBearer";```
    * ```ManagementDashboard/Infra/ApiAuthorizationMessageHandler.cs```, line: ```var authenticationScheme =  "AzureADBearer";```
    * And these are the logs:
        ```
        dbug: Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerHandler[9]
              AuthenticationScheme: AzureADBearer was not authenticated.
        dbug: Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerHandler[9]
              AuthenticationScheme: AzureADBearer was not authenticated.
        info: Microsoft.AspNetCore.Authorization.DefaultAuthorizationService[2]
              Authorization failed.
        dbug: Microsoft.Identity.Web.Resource.JwtBearerMiddlewareDiagnostics[0]
              Begin OnChallengeAsync.
        dbug: Microsoft.Identity.Web.Resource.JwtBearerMiddlewareDiagnostics[0]
              End OnChallengeAsync.
        info: Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerHandler[12]
              AuthenticationScheme: AzureADBearer was challenged.
        ```

# Web API authentication via Azure AD B2C

## Azure AD B2C

Application myapp:
  
    Include web app / web API (Yes)
    Allow implicit flow (Yes)
    Published scopes: my_api - Access My Web API - https://<my-tenant>.onmicrosoft.com/myapp/my_api

Application myappswagger:

    Include web app / web API (Yes)
    Allow implicit flow (Yes)
    Reply URL: https://www.mysite.com/MyApp/swagger/ui/o2c-html
    API access: my_api, user_impersonation, permssions_write

## C# files

    https://github.com/Azure-Samples/active-directory-b2c-dotnet-webapp-and-webapi/tree/master/TaskService
    https://knowyourtoolset.com/2015/08/secure-web-apis-with-swagger-swashbuckle-and-oauth2-part-2/

    Install: Microsoft.AspNet.WebApi.Core, Microsoft.AspNet.OwinSelfHosting, Microsoft.Owin.Cors, Swashbuckle.Core, AntiXSS

    static class Program
    {
        [STAThread]
        static void Main(String[] args)
        {
            try
            {
                //System.ServiceModel.ServiceHost host = new System.ServiceModel.ServiceHost(typeof(Service));
                //host.Open();
                string url = "https://+:443/MyApp";
                WebApp.Start(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Program.Main()", ex);
                Console.WriteLine("Program.Main()", ex.InnerException);
                Environment.Exit(1);
            }
        }
    }


[Startup.cs](Startup.cs)

[OpenIdConnectCachingSecurityTokenProvider.cs](OpenIdConnectCachingSecurityTokenProvider.cs)

## Permissions
    netsh http add sslcert ipport=0.0.0.0:443 certhash=<certificate-thumb-print> appid={application-assembly-id}
    netsh http add urlacl url=https://+:443/MyApp/ user=<my-username>

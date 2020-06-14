# SignalR

## Install Microsoft.AspNet.SignalR

## Startup.cs

        public void Configuration(IAppBuilder app)
        {
            // SignalR Scaleout with SQL Server
            // Install-Package Microsoft.AspNet.SignalR.SqlServer
            // GlobalHost.DependencyResolver.UseSqlServer(System.Configuration.ConfigurationManager.ConnectionStrings["signalrdb"].ConnectionString);

            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
            //app.MapSignalR();

            // Branch the pipeline here for requests that start with "/signalr"
            app.Map("/signalr", map =>
            {
                // Setup the CORS middleware to run before SignalR.
                // By default this will allow all origins. You can 
                // configure the set of origins and/or http verbs by
                // providing a cors options with a different policy.
                map.UseCors(CorsOptions.AllowAll);
                var hubConfiguration = new HubConfiguration
                {
                    // You can enable JSONP by uncommenting line below.
                    // JSONP requests are insecure but some older browsers (and some
                    // versions of IE) require JSONP to work cross domain
                    // EnableJSONP = true
                };
                // Run the SignalR pipeline. We're not using MapSignalR
                // since this branch already runs under the "/signalr"
                // path.
                map.RunSignalR(hubConfiguration);
            });
        }

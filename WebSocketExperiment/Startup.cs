using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebSocketExperiment.Handler;
using WebSocketExperiment.Helpers;

namespace WebSocketExperiment
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseWebSockets();

            app.Use(async (context, next) =>
            {                
                if (context.Request.Path == "/ws")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {                        
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        var locker = new object();
                        
                        if (Program.OpenedConnections >= 3 && Program.Clients.Count < 3)
                        {
                            Program.OpenedConnections = 0;
                            Program.Clients.RemoveAll(x=>true);
                        }
                        else if (Program.OpenedConnections > 3)
                        {                            
                            context.Response.StatusCode = 400;
                            return;
                        }
                        lock (locker)
                        {
                            Program.Clients.Add(webSocket);
                            Program.OpenedConnections++;
                            Helpers.Helpers.SetGuid(webSocket);
                        }
                        if (Program.OpenedConnections == 3)
                        {
                            Program.Game = new Game.Game();
                        }
                        await RequestHandler.Echo(context, webSocket);                        
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }

            });

            app.UseMvc();
        }
    }
}

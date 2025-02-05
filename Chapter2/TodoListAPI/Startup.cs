
using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using TodoListAPI.Models;
using TodoListAPI.Services;
using TodoListAPI.Repository;
using TodoListAPI.BackGroundWorker;
using TodoListAPI.BackGroundWorker.MessageHandler;
using TodoListAPI.BackGroundWorker.Message;

namespace TodoListAPI
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
            // Setting configuration for protected web api
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(Configuration);

            services.AddSingleton<IConfiguration>(Configuration);
            // Uncomment this section if you would like to validate ID tokens for allowed tenantIds
            // services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            // {
            //     options.Events.OnTokenValidated = async context =>
            //     {
            //         string[] allowedTenants = { /* list of tenant IDs */ };
            //         string tenantId = ((JwtSecurityToken)context.SecurityToken).Claims.FirstOrDefault(x => x.Type == "tid" || x.Type == "http://schemas.microsoft.com/identity/claims/tenantid")?.Value;
            //         if (!allowedTenants.Contains(tenantId))
            //         {
            //             throw new UnauthorizedAccessException("This tenant is not authorized");
            //         }
            //     };
            // });

            // Creating policies that wraps the authorization requirements
            services.AddAuthorization();

            services.AddDbContext<TodoContext>(opt => opt.UseInMemoryDatabase("TodoList"));
            services.AddSingleton<INotifier>(new Notifier());
            services.AddSingleton<IGraphAuthService>(new GraphAuthService());
            services.AddControllers();
            
            // Allowing CORS for all domains and methods for the purpose of sample
            services.AddCors(o => o.AddPolicy("default", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));            
            services.AddHttpClient<IZoomAuthService, ZoomAuthService>();
            services.AddHttpClient<IAADAuthService, AADAuthService>();
            services.AddSingleton<IUserRepository>(new InMemoryUserRepository());
            services.AddHttpClient<FetchZoomUserMessageHandler, FetchZoomUserMessageHandler>();
            services.AddHttpClient<FetchZoomChannelsForUserHandler, FetchZoomChannelsForUserHandler>();
            services.AddHttpClient<FetchParticipantsForZoomChannelHandler, FetchParticipantsForZoomChannelHandler>();
            services.AddHttpClient<FetchChatMessageForChannelHandler, FetchChatMessageForChannelHandler>();
            services.AddHttpClient<ImportChatMessagesIntoTeamsHandler, ImportChatMessagesIntoTeamsHandler>();
            services.AddHttpClient<FetchGraphUserHandler, FetchGraphUserHandler>();
            RegisterMessageHandlersWithNotifier(services);
        }

        private void RegisterMessageHandlersWithNotifier(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var notifier = serviceProvider.GetService<INotifier>();
            notifier.AddMessageHandler(MessageConstants.ZoomLoginMessageType, 
                serviceProvider.GetService<FetchZoomUserMessageHandler>());
            notifier.AddMessageHandler(MessageConstants.FetchZoomChannelsForUserMessageType,
                serviceProvider.GetService<FetchZoomChannelsForUserHandler>());
            notifier.AddMessageHandler(MessageConstants.FetchParticipantsForZoomChannelUserMessageType,
                serviceProvider.GetService<FetchParticipantsForZoomChannelHandler>());
            notifier.AddMessageHandler(MessageConstants.FetchChatMessagesForZoomChannel,
                serviceProvider.GetService<FetchChatMessageForChannelHandler>());
            notifier.AddMessageHandler(MessageConstants.ImportChatMessagesForZoomChannelIntoTeams,
                serviceProvider.GetService<ImportChatMessagesIntoTeamsHandler>());
            notifier.AddMessageHandler(MessageConstants.FetchAADUser,
                serviceProvider.GetService<FetchGraphUserHandler>());
            notifier.StartPolingAsync();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                // Since IdentityModel version 5.2.1 (or since Microsoft.AspNetCore.Authentication.JwtBearer version 2.2.0),
                // Personal Identifiable Information is not written to the logs by default, to be compliant with GDPR.
                // For debugging/development purposes, one can enable additional detail in exceptions by setting IdentityModelEventSource.ShowPII to true.
                // Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
                app.UseDeveloperExceptionPage();
            }
            else
            {
                Console.WriteLine("in prod" + Configuration["AppClientId"]);
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCors("default");
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
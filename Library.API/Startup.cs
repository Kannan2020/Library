using Library.API.Entities;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Library.API.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json.Serialization;
// using NLog.Extensions.Logging;
namespace Library.API
{
    public class Startup
    {
        public static IConfigurationRoot Configuration;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appSettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        [System.Obsolete]
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(setupAction => {
                // Restrict unsupport media type
                setupAction.ReturnHttpNotAcceptable = true;
                // For Accepting XML formate response ( need to add Package Microsoft.AspNet.Core.mvc.formatters.xml )
                setupAction.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
                // For Accepting XML formate request
                setupAction.InputFormatters.Add(new XmlDataContractSerializerInputFormatter());
            })
            //Response property for Camel Case
            .AddJsonOptions(options=> {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });

            // register the DbContext on the container, getting the connection string from
            // appSettings (note: use this during development; in a production environment,
            // it's better to store the connection string in an environment variable)
            var connectionString = Configuration["connectionStrings:libraryDBConnectionString"];
            services.AddDbContext<LibraryContext>(o => {
                o.UseSqlServer(connectionString, providerOption => providerOption.CommandTimeout(60));
                // o.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            // register the repository
            services.AddScoped<ILibraryRepository, LibraryRepository>();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper, UrlHelper>(implementationFactory =>
            {
                var actionContext = implementationFactory.GetService<IActionContextAccessor>().ActionContext;
                return new UrlHelper(actionContext);
            });

             services.AddTransient<IPropertyMappingService, PropertyMappingService>();
             services.AddTransient<ITypeHelperService, TypeHelperService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        [System.Obsolete]
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            ILoggerFactory loggerFactory, LibraryContext libraryContext)
        {
            loggerFactory.AddConsole();
            // need to install Microsoft.Extensions.logging.debug package
            loggerFactory.AddDebug(LogLevel.Information);
            // loggerFactory.AddProvider(new NLog.Extensions.Logging.NLogLoggerProvider());
            // loggerFactory.AddNLog();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(appBuilder=>
                {
                    appBuilder.Run(async context =>
                    {
                        var exceptionHandlerFeature=context.Features.Get<IExceptionHandlerFeature>();
                        if(exceptionHandlerFeature!=null)
                        {
                            var logger = loggerFactory.CreateLogger("Global Exception Logger");
                            logger.LogError(500,exceptionHandlerFeature.Error, exceptionHandlerFeature.Error.Message);
                        }
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("Something went worngplease try again");

                    });
                });
            }
            AutoMapper.Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Author, AuthorDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.GetCurrentAge()));
                cfg.CreateMap<Book, BookDto>();
                cfg.CreateMap<AuthorForCreateDto, Author>();
                cfg.CreateMap<CreateBookDto, Book>();
                cfg.CreateMap<BookForUpdateDto,Book>().ReverseMap();
            });
            libraryContext.EnsureSeedDataForContext();

            app.UseMvc();
        }
    }
}

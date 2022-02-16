using AutoMapper;
using CourseLibrary.API.DbContexts;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using System;

namespace CourseLibrary.API
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
           services.AddControllers(setupAction =>
           {
               setupAction.ReturnHttpNotAcceptable = true; //si está en falso, la API retornará respuestas en formato default si un media type requerido no es soportado 
               
           })
           .AddNewtonsoftJson(setupAction =>
            {
                setupAction.SerializerSettings.ContractResolver =
                    new CamelCasePropertyNamesContractResolver();
            })
           .AddXmlDataContractSerializerFormatters()
           .ConfigureApiBehaviorOptions(setupAction =>
           {
               setupAction.InvalidModelStateResponseFactory = context =>
               {
                   //Crea un objeto que detalla el problema
                   var problemDetailsFactory = context.HttpContext.RequestServices
                       .GetService<ProblemDetailsFactory>();
                   var problemDetails = problemDetailsFactory.CreateValidationProblemDetails(
                       context.HttpContext,
                       context.ModelState);

                   //añade información adicional no añadida por default
                   problemDetails.Detail = "See the errors field for details.";
                   problemDetails.Instance = context.HttpContext.Request.Path;

                   //encontrar el codigo de estado usar
                   var actionExecutingContext = 
                       context as Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext;

                   //Si hay errores relacionados al modelo y todos los argumentos fueron
                   //correctamente pasados o convertidos, entonces estamos tratando con 
                   //errores de validacion
                   if((context.ModelState.ErrorCount > 0) &&
                   (actionExecutingContext?.ActionArguments.Count ==
                   context.ActionDescriptor.Parameters.Count))
                   {
                       problemDetails.Type = "https://courseLibrary.com/modelvalidationproblem";
                       problemDetails.Status = StatusCodes.Status422UnprocessableEntity;
                       problemDetails.Title = "One or more validation errors ocurred.";

                       return new UnprocessableEntityObjectResult(problemDetails)
                       {
                           ContentTypes = { "application/problem+json" }
                       };
                   };

                   //Si uno de los argumentos no fué correctamente encontrado o no puede ser
                   //convertido, estamos tratando con entrada no convertible o null.
                   problemDetails.Status = StatusCodes.Status400BadRequest;
                   problemDetails.Title = "One or more errors on input ocurred.";
                   return new BadRequestObjectResult(problemDetails)
                   {
                       ContentTypes = { "application/problem+json" }
                   };
               };
               //setupAction.InvalidModelStateResponseFactory = context =>
               //{
               //    var problemDetails = new ValidationProblemDetails(context.ModelState)
               //    {
               //        Type = "https://courselibrary.com/modelvalidationproblem",
               //        Title = "One or more model validation errors ocurred.",
               //        Status = StatusCodes.Status422UnprocessableEntity,
               //        Detail = "See the errors property for details.",
               //        Instance = context.HttpContext.Request.Path
               //    };

               //    problemDetails.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);

               //    return new UnprocessableEntityObjectResult(problemDetails)
               //    {
               //        ContentTypes = { "application/problem+json" }
               //    };
               //};
           });

            //register PropertyMappingService
            services.AddTransient<IPropertyMappingService, PropertyMappingService>();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
             
            services.AddScoped<ICourseLibraryRepository, CourseLibraryRepository>();

            services.AddDbContext<CourseLibraryContext>(options =>
            {
                options.UseSqlServer(
                    @"Server=(localdb)\mssqllocaldb;Database=CourseLibraryDB;Trusted_Connection=True;");
            }); 
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected fault happened. Try again later");
                    });
                });
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

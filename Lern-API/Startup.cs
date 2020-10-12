using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentMigrator.Runner;
using FluentValidation.AspNetCore;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.Helpers;
using Lern_API.Helpers.Database;
using Lern_API.Helpers.JWT;
using Lern_API.Helpers.Swagger;
using Lern_API.Migrations;
using Lern_API.Models;
using Lern_API.Repositories;
using Lern_API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using PetaPoco;
using PetaPoco.Providers;

namespace Lern_API
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration.Config = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Ajout du système de communication avec la base de données
            services.AddScoped<IDatabase, Database>(ctx => new Database(
                Configuration.GetConnectionString(),
                new PostgreSQLDatabaseProvider(),
                new ConventionMapper
                {
                    InflectTableName = (inflector, s) => inflector.Pluralise(inflector.Underscore(s)),
                    InflectColumnName = (inflector, s) => inflector.Camelise(s)
                }
            ));

            // Ajout du système de migration de bases de données
            services.AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddPostgres()
                    .WithGlobalConnectionString(Configuration.GetConnectionString())
                    .ScanIn(typeof(InitializeDatabase).Assembly).For.Migrations());

            // Ajout des contrôleurs applicatifs
            services.AddControllers()
                // Configuration de la validation des modèles
                .AddFluentValidation(fv =>
                {
                    fv.RegisterValidatorsFromAssemblyContaining<UserValidator>();
                    fv.RegisterValidatorsFromAssemblyContaining<LoginRequestValidator>();
                    fv.ImplicitlyValidateChildProperties = true;
                })
                // Configuration de la réponse à un modèle invalide
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = c => new BadRequestObjectResult(new
                    {
                        Errors = c.ModelState.Values.Where(v => v.Errors.Count > 0).SelectMany(v => v.Errors).Select(v => v.ErrorMessage)
                    });
                });

            // Ajout de la compression des réponses
            services.AddResponseCompression(options =>
            {
                options.MimeTypes = Configuration.GetList("GzipSupportedMimeTypes");
            });

            // Ajout de l'auto-génération de Swagger
            services.AddSwaggerGen(options =>
            {
                var info = new OpenApiInfo
                {
                    Title = "Lern. API",
                    Version = "1.0-SNAPSHOT",
                    Description = ""
                };

                options.SwaggerDoc("v1", info);

                options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme.
                                    <br><br>Enter your token in the text input below.",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
                options.IncludeXmlComments(xmlPath);

                options.OperationFilter<AddAuthHeaderOperationFilter>();
                options.SchemaFilter<ReadOnlyPropertiesFilter>();
            });

            // Ajout des en-têtes CORS
            services.AddCors();

            // Ajout des repository
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUserRepository, UserRepository>();

            // Ajout des services
            services.AddScoped(typeof(IService<,>), typeof(Service<,>));
            services.AddScoped<IUserService, UserService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddLog4Net();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles(new StaticFileOptions(new SharedOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Content"))
            }));

            app.UseCors(options =>
            {
                options.AllowAnyOrigin();
                options.AllowAnyHeader();
                options.AllowAnyMethod();
            });

            app.UseSwagger().UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lern. API");
            });

            app.UseRouting();
            app.UseAuthentication();
            app.UseResponseCompression();

            app.UseMiddleware<JwtMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.EnsureDatabase();

            app.Migrate();
        }
    }
}

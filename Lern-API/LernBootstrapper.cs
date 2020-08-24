using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Security.Claims;
using FluentMigrator.Runner;
using JWT.Exceptions;
using Lern_API.Models;
using Lern_API.Repositories;
using Lern_API.Utilities;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using PetaPoco;
using PetaPoco.Providers;

namespace Lern_API
{
    public class LernBootstrapper : DefaultNancyBootstrapper
    {
        private ILogger Log { get; }
        private IDatabase Database { get; }
        private IMigrationRunner MigrationRunner { get; }
        private bool IsProduction { get; }

        private int GzipMinimumBytes { get; } = Configuration.Get<int>("GzipMinimumBytes");
        private IEnumerable<string> GzipSupportedMimeTypes { get; } = Configuration.GetList("GzipSupportedMimeTypes");

        public LernBootstrapper(IMigrationRunner migrationRunner, bool isProduction = false) :
            this(Logger.GetLogger(typeof(LernBootstrapper)),
                DatabaseConfiguration.Build()
                    .UsingConnectionString(Configuration.GetConnectionString())
                    .UsingProvider<PostgreSQLDatabaseProvider>()
                    .UsingDefaultMapper<ConventionMapper>(m => m.InflectTableName = (inflector, s) => inflector.Pluralise(inflector.Underscore(s)))
                    .Create())
        {
            MigrationRunner = migrationRunner;
            IsProduction = isProduction;
        }

        public LernBootstrapper(ILogger log, IDatabase database)
        {
            Log = log;
            Database = database;
        }

        // TODO: uniformiser les commentaires
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            pipelines.BeforeRequest.AddItemToStartOfPipeline(ctx =>
            {
                if (ctx != null)
                {
                    Log.Request(ctx.Request.GetHashCode(), ctx.Request.Method, ctx.Request.Path, ctx.Request.UserHostAddress, ctx.Request.Headers.UserAgent);
                }

                return null;
            });

            pipelines.AfterRequest.AddItemToEndOfPipeline(ctx =>
            {
                if (ctx != null)
                {
                    Log.Response(ctx.Request.GetHashCode(), ctx.Response.StatusCode);
                }
            });

            var secretKey = Configuration.Get<string>("SecretKey");

            // Ajout de la gestion de l'authentification par token JWT
            StatelessAuthentication.Enable(pipelines, new StatelessAuthenticationConfiguration(ctx =>
            {
                var auth = ctx.Request.Headers.Authorization;

                // Vérifie si la demande d'authentification est valide, sinon termine la fonction
                if (string.IsNullOrWhiteSpace(auth) || !auth.StartsWith("Bearer ", StringComparison.InvariantCulture))
                    return null;

                var jwtToken = auth.Substring(7);

                try
                {
                    var identity = JwtHelper.Decode<Identity>(jwtToken, secretKey);

                    return new ClaimsPrincipal(identity);
                }
                catch (TokenExpiredException)
                {
                    Log.Info("Token expiré");
                }
                catch (SignatureVerificationException)
                {
                    Log.Info("La signature du token est invalide");
                }
                catch (Exception)
                {
                    // Si le token n'est pas correctement formatté
                }

                return null;
            }));

            Log.Info("Initialisation terminée");
        }

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            // Ajoute les headers CORS aux réponses du serveur
            pipelines?.AfterRequest.AddItemToEndOfPipeline(ctx =>
            {
                ctx.Response.WithHeader("Access-Control-Allow-Origin", "*")
                    .WithHeader("Access-Control-Allow-Methods", "POST,GET,PUT,DELETE,HEAD,OPTIONS")
                    .WithHeader("Access-Control-Allow-Headers", "Accept, Origin, Content-type, Authorization");
            });
            
            // Ajout de la compression gzip pour minimiser l'impact sur la bande passante
            pipelines?.AfterRequest.AddItemToEndOfPipeline(ctx =>
            {
                // Vérifie si le client accepte les réponses gzip
                if (!ctx.Request.Headers.AcceptEncoding.Any(x => x.Contains("gzip")))
                    return;

                // N'applique le gzip que sur les requêtes qui se sont déroulées correctement
                if (ctx.Response.StatusCode != HttpStatusCode.OK)
                    return;

                // Vérifie que la configuration autorise le gzip sur ce contenu
                if (!GzipSupportedMimeTypes.Any(x => x == ctx.Response.ContentType || ctx.Response.ContentType.StartsWith($"{x};")))
                    return;

                // Vérifie que la requête ne soit pas plus petite que le minimum configuré, si possible
                if (ctx.Response.Headers.TryGetValue("Content-Length", out var contentLength))
                {
                    var length = long.Parse(contentLength);

                    if (length < GzipMinimumBytes)
                        return;
                }

                // Compresse le contenu de la requête
                ctx.Response.Headers["Content-Encoding"] = "gzip";
                ctx.Response.Headers.Remove("Content-Length");
                var contents = ctx.Response.Contents;

                ctx.Response.Contents = responseStream =>
                {
                    using var compression = new GZipStream(responseStream, CompressionMode.Compress);
                    contents(compression);
                };
            });
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            container.Register(Log);

            var connected = false;

            try
            {
                Retry.Do(() =>
                {
                    Log.Info("Tentative de connexion à la base de données...");

                    Database.Execute("SELECT 1");
                    connected = true;

                    Log.Info("Connexion à la base de données réussie");
                }, TimeSpan.FromSeconds(5));
            }
            catch (AggregateException e)
            {
                Log.Error($"Les tentatives de connexion à la base de données ont échoué : {e.InnerExceptions.Last().Message}");

                if (IsProduction)
                {
                    Environment.Exit(1);
                }
            }

            if (MigrationRunner != null && connected)
            {
                if (!MigrationRunner.HasMigrationsToApplyUp())
                {
                    Log.Info("La base de données est à jour");
                }
                else
                {
                    Log.Info("Migration de la base de données vers une nouvelle version...");

                    MigrationRunner.MigrateUp();

                    Log.Info("Migration terminée");
                }
            }

            container.Register(Database);

            // Ajout des Repository
            container.Register<UserRepository>();
        }
    }
}

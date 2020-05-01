using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Security.Claims;
using JWT;
using Lern_API.Models;
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
        private Logger Log { get; } = Logger.GetLogger(typeof(LernBootstrapper));

        private int GzipMinimumBytes { get; } = Configuration.Get<int>("GzipMinimumBytes");
        private IEnumerable<string> GzipSupportedMimeTypes { get; } = Configuration.GetList("GzipSupportedMimeTypes");

        // TODO: uniformiser les commentaires
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            Log.Info("Initialisation en cours");

            base.ApplicationStartup(container, pipelines);

            Log.Info("Connexion à la base de données");

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
                    var identity = JwtHelper.Decode<Identity>(jwtToken, Configuration.Get<string>("SecretKey"));

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

            var host = Configuration.Get<string>("DbHost");
            var username = Configuration.Get<string>("DbUsername");
            var password = Configuration.Get<string>("DbPassword");
            var db = Configuration.Get<string>("DbDatabase");
            var port = Configuration.Get<int>("DbPort");

            var database = DatabaseConfiguration.Build()
                .UsingConnectionString($"Host={host};Username={username};password={password};database={db};port={port}")
                .UsingProvider<PostgreSQLDatabaseProvider>()
                .UsingDefaultMapper<ConventionMapper>(m =>
                {
                    m.InflectTableName = (inflector, s) => inflector.Pluralise(inflector.Underscore(s));
                })
                .Create();

            container.Register(database);
        }
    }
}

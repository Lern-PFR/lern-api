using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Security.Claims;
using Lern_API.Models;
using Lern_API.Utilities;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;

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
                    var user = JwtHelper.Decode<User>(jwtToken, Configuration.Get<string>("SecretKey"));

                    return new ClaimsPrincipal(user);
                }
                catch (Exception)
                {
                    // Si le token n'a pas pu être décodé ou n'est pas valide, termine la fonction
                    return null;
                }
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

                // Vérifie que la configuration autorise la gzip sur ce contenu
                if (!GzipSupportedMimeTypes.Any(x => x == ctx.Response.ContentType || ctx.Response.ContentType.StartsWith($"{x};")))
                    return;

                // Vérifie que la requête ne soit pas plus petite que le minimum configuré
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
                    using (var compression = new GZipStream(responseStream, CompressionMode.Compress))
                    {
                        contents(compression);
                    }
                };
            });
        }

        // Masquage des répertoires non autorisés
        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            // We don't call "base" here to prevent auto-discovery of types/dependencies
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Lern_API.Utilities;
using Microsoft.Extensions.Configuration;
using Nancy;
using Nancy.Testing;
using Xunit;

namespace Lern_API.Tests.Modules
{
    public class HelloWorldModuleShould
    {
        [Fact]
        public async Task Return_Status_OK()
        {
            var browser = new Browser(new LernBootstrapper());

            var result = await browser.Get("/hello", with =>
            {
                with.HttpRequest();
            });

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task Return_Hello_World()
        {
            var browser = new Browser(new LernBootstrapper());

            var result = await browser.Get("/hello", with =>
            {
                with.HttpRequest();
            });

            Assert.Equal("Hello, world!", result.Body.AsString());
        }

        [Fact]
        public async Task Return_404_On_False_Route()
        {
            var browser = new Browser(new LernBootstrapper());

            var result = await browser.Get("/false_route", with =>
            {
                with.HttpRequest();
            });

            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task Has_Gzip_Compression()
        {
            Configuration.Config = new ConfigurationBuilder().AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("GzipMinimumBytes", "0"),
                new KeyValuePair<string, string>("GzipSupportedMimeTypes:0", "text/plain")
            }).Build();

            var browser = new Browser(new LernBootstrapper(), d => d.Header("Accept-Encoding", "gzip"));

            var result = await browser.Get("/hello", with =>
            {
                with.HttpRequest();
            });

            Assert.Contains("Content-Encoding", result.Headers);
            Assert.Equal("gzip", result.Headers["Content-Encoding"]);

            var stream = new GZipStream(result.Body.AsStream(), CompressionMode.Decompress);
            using var reader = new StreamReader(stream);

            Assert.Equal("Hello, world!", await reader.ReadToEndAsync());
        }

        [Fact]
        public async Task Use_Gzip_Only_When_OK()
        {
            Configuration.Config = new ConfigurationBuilder().AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("GzipMinimumBytes", "0"),
                new KeyValuePair<string, string>("GzipSupportedMimeTypes:0", "text/plain")
            }).Build();

            var browser = new Browser(new LernBootstrapper(), d => d.Header("Accept-Encoding", "gzip"));

            var result = await browser.Get("/false_route", with =>
            {
                with.HttpRequest();
            });

            Assert.DoesNotContain("Content-Encoding", result.Headers);

            await Assert.ThrowsAsync<InvalidDataException>(async () =>
            {
                var stream = new GZipStream(result.Body.AsStream(), CompressionMode.Decompress);
                using var reader = new StreamReader(stream);

                await reader.ReadToEndAsync();
            });
        }

        [Fact]
        public async Task Use_Gzip_Only_When_Configured()
        {
            Configuration.Config = new ConfigurationBuilder().AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("GzipMinimumBytes", "0"),
                new KeyValuePair<string, string>("GzipSupportedMimeTypes:0", "")
            }).Build();

            var browser = new Browser(new LernBootstrapper(), d => d.Header("Accept-Encoding", "gzip"));

            var result = await browser.Get("/hello", with =>
            {
                with.HttpRequest();
            });

            Assert.DoesNotContain("Content-Encoding", result.Headers);

            await Assert.ThrowsAsync<InvalidDataException>(async () =>
            {
                var stream = new GZipStream(result.Body.AsStream(), CompressionMode.Decompress);
                using var reader = new StreamReader(stream);

                await reader.ReadToEndAsync();
            });
        }

        [Fact]
        public async Task Use_Gzip_Only_When_Client_Can()
        {
            Configuration.Config = new ConfigurationBuilder().AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("GzipMinimumBytes", "0"),
                new KeyValuePair<string, string>("GzipSupportedMimeTypes:0", "text/plain")
            }).Build();

            var browser = new Browser(new LernBootstrapper());

            var result = await browser.Get("/hello", with =>
            {
                with.HttpRequest();
            });

            Assert.DoesNotContain("Content-Encoding", result.Headers);

            await Assert.ThrowsAsync<InvalidDataException>(async () =>
            {
                var stream = new GZipStream(result.Body.AsStream(), CompressionMode.Decompress);
                using var reader = new StreamReader(stream);

                await reader.ReadToEndAsync();
            });
        }
    }
}

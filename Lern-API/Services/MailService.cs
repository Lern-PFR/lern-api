using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentEmail.Core;
using FluentEmail.Core.Models;
using Lern_API.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Lern_API.Services
{
    public interface IMailService
    {
        Task<SendResponse> SendEmailAsync<T>(User recipient, string subject, string template, T model, CancellationToken token = default);
        Task<SendResponse> SendEmailAsync<T>(string recipientName, string recipientAddress, string subject, string template, T model, CancellationToken token = default);
    }

    public class MailService : IMailService
    {
        private readonly IServiceProvider _serviceProvider;

        public MailService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<SendResponse> SendEmailAsync<T>(User recipient, string subject, string template, T model, CancellationToken token = default)
        {
            return await SendEmailAsync($"{recipient.Firstname} {recipient.Lastname}", recipient.Email, subject, template, model, token);
        }

        public async Task<SendResponse> SendEmailAsync<T>(string recipientName, string recipientAddress, string subject, string template, T model, CancellationToken token = default)
        {
            using var scope = _serviceProvider.CreateScope();

            // This method is untestable because of this line
            // Unfortunately, this line is required for the IFluentEmailFactory to be retrieved without crashing the entire server
            return await scope.ServiceProvider.GetRequiredService<IFluentEmailFactory>()
                .Create()
                .To(recipientAddress, recipientName)
                .Subject(subject)
                .UsingTemplateFromFile(Path.Combine("Templates", $"{template}.liquid"), model)
                .SendAsync(token);
        }
    }
}

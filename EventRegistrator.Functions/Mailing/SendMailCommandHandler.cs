using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace EventRegistrator.Functions.Mailing
{
    public static class SendMailCommandHandler
    {
        [FunctionName("SendMailCommandHandler")]
        public static async Task Run([ServiceBusTrigger(SendMailQueueName, AccessRights.Listen, Connection = "ServiceBusEndpoint")]SendMailCommand command, TraceWriter log)
        {
            log.Info($"C# ServiceBus queue trigger function processed message: {command}");

            var msg = new SendGridMessage
            {
                From = new SendGrid.Helpers.Mail.EmailAddress(command.Sender?.Email, command.Sender?.Name),
                Subject = command.Subject,
                PlainTextContent = command.ContentPlainText,
                HtmlContent = command.ContentHtml
            };

            msg.AddTos(command.To.Select(to => new SendGrid.Helpers.Mail.EmailAddress(to.Email, to.Name)).ToList());

            // send mail
            var apiKey = Environment.GetEnvironmentVariable("SendGrid_ApiKey");
            var client = new SendGridClient(apiKey);
            var response = await client.SendEmailAsync(msg);
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Accepted)
            {
                log.Warning($"ComposeAndSendMailCommandHandler status {response.StatusCode}, Body {await response.Body.ReadAsStringAsync()}");
            }
        }

        public const string SendMailQueueName = "SendMailCommands";
    }
}
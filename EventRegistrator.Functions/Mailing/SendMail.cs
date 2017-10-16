using EventRegistrator.Functions.Infrastructure.DomainEvents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EventRegistrator.Functions.Mailing
{
    public static class SendMail
    {
        public const string SendMailCommandsQueueName = "SendMailCommands";

        [FunctionName("SendMail")]
        public static async Task Run([ServiceBusTrigger(SendMailCommandsQueueName, AccessRights.Listen, Connection = "ServiceBusEndpoint")]SendMailCommand command, TraceWriter log)
        {
            log.Info($"C# ServiceBus queue trigger function processed message: {command}");

            var logTask = DomainEventPersistor.Log(command);

            var msg = new SendGridMessage
            {
                From = new EmailAddress("registration@leapinlindy.ch", "Leapin' Lindy Registration"),
                Subject = "Test: Bestätigung",
                HtmlContent = "<p>Ihr seid dabei!</p>"
            };

            var recipients = command.Recipients.Select(rcp => new EmailAddress(rcp.Mail, rcp.Name)).ToList();
            msg.AddTos(recipients);

            var apiKey = Environment.GetEnvironmentVariable("SendGrid_ApiKey");
            var client = new SendGridClient(apiKey);
            var response = await client.SendEmailAsync(msg);
            await logTask;
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Accepted)
            {
                log.Warning($"SendMail status {response.StatusCode}, Body {await response.Body.ReadAsStringAsync()}");
            }
        }
    }
}
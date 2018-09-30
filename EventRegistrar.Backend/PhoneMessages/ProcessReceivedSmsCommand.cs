using EventRegistrar.Backend.Infrastructure.ServiceBus;
using MediatR;

namespace EventRegistrar.Backend.PhoneMessages
{
    public class ProcessReceivedSmsCommand : IRequest, IQueueBoundMessage
    {
        public string QueueName => "processreceivedsms";
        public TwilioSms Sms { get; set; }
    }
}
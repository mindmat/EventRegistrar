using System.Reflection;
using System.Text;

using MediatR;

using Microsoft.Azure.ServiceBus;

using Newtonsoft.Json;

using SimpleInjector;

namespace EventRegistrar.Backend.Infrastructure.ServiceBus;

public class MessageQueueReceiver : IDisposable
{
    private readonly Container _container;
    private readonly ILogger _logger;
    private readonly IMediator _mediator;
    private readonly IList<QueueClient> _queueClients = new List<QueueClient>();
    private readonly MethodInfo _typedProcessMethod;
    private readonly string _serviceBusEndpoint;

    public MessageQueueReceiver(IMediator mediator,
                                ILogger logger,
                                Container container,
                                IConfiguration configuration)
    {
        _mediator = mediator;
        _logger = logger;
        _container = container;
        _typedProcessMethod = typeof(MessageQueueReceiver).GetMethod(nameof(ProcessTypedMessage),
                                                                     BindingFlags.NonPublic | BindingFlags.Instance)!;
        _serviceBusEndpoint = Environment.GetEnvironmentVariable("ServiceBusEndpoint")
                           ?? configuration.GetValue<string>("ServiceBusEndpoint");
    }

    public void Dispose()
    {
        foreach (var queueClient in _queueClients)
        {
            queueClient.CloseAsync().Wait();
        }
    }

    public void RegisterMessageHandlers()
    {
        if (string.IsNullOrEmpty(_serviceBusEndpoint))
        {
            _logger.LogWarning("No ServiceBusEndpoint configured");
            return;
        }

        var genericQueueClient = new QueueClient(_serviceBusEndpoint, CommandQueue.CommandQueueName);

        // Configure the MessageHandler Options in terms of exception handling, number of concurrent messages to deliver etc.
        var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
                                    {
                                        // Maximum number of Concurrent calls to the callback `ProcessMessagesAsync`, set to 1 for simplicity.
                                        // Set it according to how many messages the application wants to process in parallel.
                                        MaxConcurrentCalls = 1,

                                        // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                                        // False value below indicates the Complete will be handled by the User Callback as seen in `ProcessMessagesAsync`.
                                        AutoComplete = true
                                    };

        // Register the function that will process messages
        genericQueueClient.RegisterMessageHandler(ProcessGenericMessage, messageHandlerOptions);
        _queueClients.Add(genericQueueClient);
    }

    private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs arg)
    {
        _logger.LogError(arg.Exception,
                         $"Error while processing a message from queue. Endpoint: {0}, Action: {1}, ClientId {2}, EntityPath {3}",
                         arg.ExceptionReceivedContext.Endpoint,
                         arg.ExceptionReceivedContext.Action,
                         arg.ExceptionReceivedContext.ClientId,
                         arg.ExceptionReceivedContext.EntityPath);
        return Task.CompletedTask;
    }

    private async Task ProcessGenericMessage(Message message, CancellationToken cancellationToken)
    {
        var messageDecoded = Encoding.UTF8.GetString(message.Body);
        var commandMessage = JsonConvert.DeserializeObject<CommandMessage>(messageDecoded);
        var commandType = Type.GetType(commandMessage.CommandType);

        var typedMethod = _typedProcessMethod.MakeGenericMethod(commandType);
        await (Task)typedMethod.Invoke(this,
                                       new object[] { commandMessage.CommandSerialized, cancellationToken, CommandQueue.CommandQueueName });
    }

    private async Task ProcessTypedMessage<TRequest>(string commandSerialized,
                                                     CancellationToken cancellationToken,
                                                     string queueName)
        where TRequest : IRequest
    {
        var request = JsonConvert.DeserializeObject<TRequest>(commandSerialized);
        using (new EnsureExecutionScope(_container))
        {
            _container.GetInstance<SourceQueueProvider>().SourceQueueName = queueName;
            await _mediator.Send(request, cancellationToken);
        }
    }
}
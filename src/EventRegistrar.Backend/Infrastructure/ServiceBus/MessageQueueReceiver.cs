using System.Reflection;

using Azure.Messaging.ServiceBus;

using Newtonsoft.Json;

using SimpleInjector;

namespace EventRegistrar.Backend.Infrastructure.ServiceBus;

public class MessageQueueReceiver : IAsyncDisposable
{
    private readonly Container _container;
    private readonly ServiceBusClient _client;
    private readonly ILogger _logger;
    private readonly IMediator _mediator;
    private readonly MethodInfo _typedProcessMethod;
    private ServiceBusProcessor? _processor;

    public MessageQueueReceiver(IMediator mediator,
                                ILogger logger,
                                Container container,
                                ServiceBusClient client)
    {
        _mediator = mediator;
        _logger = logger;
        _container = container;
        _client = client;
        _typedProcessMethod = typeof(MessageQueueReceiver).GetMethod(nameof(ProcessTypedMessage),
                                                                     BindingFlags.NonPublic | BindingFlags.Instance)!;
    }

    public async void StartReceiveLoop()
    {
        if (_processor != null)
        {
            return;
        }

        var options = new ServiceBusProcessorOptions
                      {
                          MaxConcurrentCalls = 1,
                          AutoCompleteMessages = true
                      };
        _processor = _client.CreateProcessor(CommandQueue.CommandQueueName, options);
        _processor.ProcessMessageAsync += ProcessMessage;
        _processor.ProcessErrorAsync += ProcessError;

        await _processor.StartProcessingAsync();
    }

    private Task ProcessError(ProcessErrorEventArgs arg)
    {
        _logger.LogError(arg.Exception,
                         $"Error while processing a message from queue. Namespace: {0}, Identifier: {1}, ErrorSource: {2}, ClientId {3}, Exception: {4}, EntityPath {5}",
                         arg.FullyQualifiedNamespace,
                         arg.Identifier,
                         arg.ErrorSource,
                         arg.Exception.Message,
                         arg.EntityPath);
        return Task.CompletedTask;
    }

    private async Task ProcessMessage(ProcessMessageEventArgs arg)
    {
        //var messageDecoded = Encoding.UTF8.GetString(arg.Body);
        var commandMessage = JsonConvert.DeserializeObject<CommandMessage>(arg.Message.Body.ToString());
        if (commandMessage?.CommandType == null)
        {
            throw new ArgumentException($"Invalid message: {arg.Message.Body}");
        }

        var commandType = Type.GetType(commandMessage.CommandType);
        if (commandType == null)
        {
            throw new ArgumentException($"Unknown command type: {commandType}");
        }

        var typedMethod = _typedProcessMethod.MakeGenericMethod(commandType);
        await (Task)typedMethod.Invoke(this,
                                       new object[] { commandMessage.CommandSerialized, CommandQueue.CommandQueueName, arg.CancellationToken });
    }

    private async Task ProcessTypedMessage<TRequest>(string commandSerialized,
                                                     string queueName,
                                                     CancellationToken cancellationToken)
        where TRequest : IRequest
    {
        var request = JsonConvert.DeserializeObject<TRequest>(commandSerialized);
        using (new EnsureExecutionScope(_container))
        {
            _container.GetInstance<SourceQueueProvider>().SourceQueueName = queueName;
            await _mediator.Send(request, cancellationToken);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_processor != null)
        {
            await _processor.StopProcessingAsync();
            await _processor.DisposeAsync();
        }
    }
}
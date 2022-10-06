using System.Text;

using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Payments.Files.Fetch;

public class FetchBankStatementsFileCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class FetchBankStatementsFileCommandHandler : IRequestHandler<FetchBankStatementsFileCommand>
{
    private readonly IRepository<RawBankStatementsFile> _files;
    private readonly FetchBankStatementsFilesConfiguration _configuration;
    private readonly SecretReader _secretReader;
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;

    public FetchBankStatementsFileCommandHandler(IRepository<RawBankStatementsFile> files,
                                                 FetchBankStatementsFilesConfiguration configuration,
                                                 SecretReader secretReader,
                                                 IEventBus eventBus,
                                                 ILogger logger)
    {
        _files = files;
        _configuration = configuration;
        _secretReader = secretReader;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<Unit> Handle(FetchBankStatementsFileCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var key = await _secretReader.GetSecret(_configuration.KeyName, cancellationToken);
            if (key == null)
            {
                return Unit.Value;
            }

            var keyString = key.Replace("\\r\\n", Environment.NewLine);
            var bytes = Encoding.ASCII.GetBytes(keyString);
            var stream = new MemoryStream(bytes);

            var connectionInfo = new Renci.SshNet.ConnectionInfo(_configuration.Server,
                                                                 _configuration.ContractIdentifier,
                                                                 new Renci.SshNet.PrivateKeyAuthenticationMethod(_configuration.ContractIdentifier,
                                                                                                                 new Renci.SshNet.PrivateKeyFile(stream, _configuration.Passphrase)));
            using var client = new Renci.SshNet.SftpClient(connectionInfo);
            client.Connect();
            var result = client.ListDirectory(_configuration.Directory);
            foreach (var fileReady in result.Where(fil => IsNew(fil.FullName)))
            {
                var content = client.ReadAllBytes(fileReady.FullName);
                var rawFile = new RawBankStatementsFile
                              {
                                  Id = Guid.NewGuid(),
                                  Server = _configuration.Server,
                                  ContractIdentifier = _configuration.ContractIdentifier,
                                  Filename = fileReady.FullName,
                                  Imported = DateTimeOffset.Now,
                                  Content = content
                              };
                await _files.InsertOrUpdateEntity(rawFile, cancellationToken);
                _eventBus.Publish(new BankStatementsFileImported
                                  { EventId = command.EventId, BankStatementsFileId = rawFile.Id });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }

        foreach (var pendingFile in _files.Where(fil => fil.Processed == null))
        {
            _eventBus.Publish(new BankStatementsFileImported
                              { EventId = command.EventId, BankStatementsFileId = pendingFile.Id });
        }

        return Unit.Value;
    }

    private bool IsNew(string fullName)
    {
        return !_files.Any(bsf => bsf.Filename == fullName);
    }
}
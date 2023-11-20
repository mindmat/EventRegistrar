using System.Text;

using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Payments.Files.Fetch;

public class FetchBankStatementsFileCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class FetchBankStatementsFileCommandHandler(IRepository<RawBankStatementsFile> files,
                                                   FetchBankStatementsFilesConfiguration configuration,
                                                   SecretReader secretReader,
                                                   IEventBus eventBus,
                                                   ILogger logger)
    : IRequestHandler<FetchBankStatementsFileCommand>
{
    public async Task Handle(FetchBankStatementsFileCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var key = await secretReader.GetSecret(configuration.KeyName, cancellationToken);
            if (key == null)
            {
                return;
            }

            var keyString = key.Replace("\\r\\n", Environment.NewLine);
            var bytes = Encoding.ASCII.GetBytes(keyString);
            var stream = new MemoryStream(bytes);

            var connectionInfo = new Renci.SshNet.ConnectionInfo(configuration.Server,
                                                                 configuration.ContractIdentifier,
                                                                 new Renci.SshNet.PrivateKeyAuthenticationMethod(configuration.ContractIdentifier,
                                                                                                                 new Renci.SshNet.PrivateKeyFile(stream, configuration.Passphrase)));
            using var client = new Renci.SshNet.SftpClient(connectionInfo);
            client.Connect();
            var result = client.ListDirectory(configuration.Directory);
            foreach (var fileReady in result.Where(fil => IsNew(fil.FullName)))
            {
                var content = client.ReadAllBytes(fileReady.FullName);
                var rawFile = new RawBankStatementsFile
                              {
                                  Id = Guid.NewGuid(),
                                  Server = configuration.Server,
                                  ContractIdentifier = configuration.ContractIdentifier,
                                  Filename = fileReady.FullName,
                                  Imported = DateTimeOffset.Now,
                                  Content = content
                              };
                await files.InsertOrUpdateEntity(rawFile, cancellationToken);
                eventBus.Publish(new BankStatementsFileImported
                                 {
                                     EventId = command.EventId,
                                     BankStatementsFileId = rawFile.Id
                                 });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }

        foreach (var pendingFile in files.Where(fil => fil.Processed == null))
        {
            eventBus.Publish(new BankStatementsFileImported
                             {
                                 EventId = command.EventId,
                                 BankStatementsFileId = pendingFile.Id
                             });
        }
    }

    private bool IsNew(string fullName)
    {
        return !files.Any(bsf => bsf.Filename == fullName);
    }
}
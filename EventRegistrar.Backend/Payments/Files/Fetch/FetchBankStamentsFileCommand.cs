using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

using MediatR;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Renci.SshNet;

namespace EventRegistrar.Backend.Payments.Files.Fetch
{
    public class FetchBankStamentsFileCommand : IRequest, IEventBoundRequest
    {
        public Guid EventId { get; set; }
    }

    public class FetchBankStamentsFileCommandHandler : IRequestHandler<FetchBankStamentsFileCommand>
    {
        private readonly IRepository<RawBankStatementsFile> _files;
        private readonly FetchBankStatementsFilesConfiguration _configuration;
        private readonly IEventBus _eventBus;
        private readonly ILogger _logger;
        private readonly string _keyVaultUri;

        public FetchBankStamentsFileCommandHandler(IRepository<RawBankStatementsFile> files,
                                                   FetchBankStatementsFilesConfiguration configuration,
                                                   IConfiguration appConfiguration,
                                                   IEventBus eventBus,
                                                   ILogger logger)
        {
            _files = files;
            _configuration = configuration;
            _eventBus = eventBus;
            _logger = logger;
            _keyVaultUri = appConfiguration["KeyVaultUri"];
        }

        public async Task<Unit> Handle(FetchBankStamentsFileCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var cred =
                    //new Azure.Identity.DefaultAzureCredential(true);
                    new ManagedIdentityCredential();
                var keyVaultClient = new SecretClient(new Uri(_keyVaultUri), cred);
                var key = await keyVaultClient.GetSecretAsync(_configuration.KeyName);
                var keyString = key.Value.Value.Replace("\\r\\n", Environment.NewLine);
                var bytes = Encoding.ASCII.GetBytes(keyString);
                var stream = new MemoryStream(bytes);

                var connectionInfo = new ConnectionInfo(_configuration.Server,
                                                        _configuration.ContractIdentifier,
                                                        new PrivateKeyAuthenticationMethod(_configuration.ContractIdentifier,
                                                        new PrivateKeyFile(stream, _configuration.Passphrase)));
                using var client = new SftpClient(connectionInfo);
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
                    _eventBus.Publish(new BankStatementsFileImported { EventId = command.EventId, BankStatementsFileId = rawFile.Id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            foreach (var pendingFile in _files.Where(fil => fil.Processed == null))
            {
                _eventBus.Publish(new BankStatementsFileImported { EventId = command.EventId, BankStatementsFileId = pendingFile.Id });
            }
            return Unit.Value;
        }

        private bool IsNew(string fullName)
        {
            return !_files.Any(bsf => bsf.Filename == fullName);
        }
    }
}

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
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

        public async Task<Unit> Handle(FetchBankStamentsFileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var cred =
                    //new Azure.Identity.DefaultAzureCredential(true);
                    new ManagedIdentityCredential();
                var keyVaultClient = new KeyClient(new Uri(_keyVaultUri), cred);
                var key = await keyVaultClient.GetKeyAsync(_configuration.KeyName);
                var keyStream = new MemoryStream(key.Value.Key.D);
                var connectionInfo = new ConnectionInfo(_configuration.Server,
                                                        22,
                                                        _configuration.ContractIdentifier,
                                                        new PrivateKeyAuthenticationMethod(_configuration.ContractIdentifier,
                                                                                           new PrivateKeyFile(keyStream, _configuration.Passphrase)));
                using (var client = new SftpClient(connectionInfo))
                {
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
                        await _files.InsertOrUpdateEntity(rawFile);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            foreach (var pendingFile in _files.Where(fil => fil.Processed == null))
            {
                _eventBus.Publish(new BankStatementsFileImported { BankStatementsFileId = pendingFile.Id });
            }
            return Unit.Value;
        }

        private bool IsNew(string fullName)
        {
            return !_files.Any(bsf => bsf.Filename == fullName);
        }
    }
}

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Payments.Files.Fetch
{
    public class ProcessFetchedBankStatementsFileCommand : IRequest, IEventBoundRequest
    {
        public Guid RawBankStatementFileId { get; set; }

        public Guid EventId { get; set; }
    }

    public class ProcessFetchedBankStatementsFileCommandHandler : IRequestHandler<ProcessFetchedBankStatementsFileCommand>
    {
        private readonly IRepository<RawBankStatementsFile> _files;
        private readonly IRequestHandler<SavePaymentFileCommand> _savePaymentFileCommandHandler;

        public ProcessFetchedBankStatementsFileCommandHandler(IRepository<RawBankStatementsFile> files,
                                                              IRequestHandler<SavePaymentFileCommand> savePaymentFileCommandHandler)
        {
            _files = files;
            _savePaymentFileCommandHandler = savePaymentFileCommandHandler;
        }

        public async Task<Unit> Handle(ProcessFetchedBankStatementsFileCommand command, CancellationToken cancellationToken)
        {
            var file = await _files.FirstAsync(fil => fil.Id == command.RawBankStatementFileId, cancellationToken);
            if (file.Processed != null)
            {
                return Unit.Value;
            }
            file.Processed = DateTimeOffset.Now;

            var savePaymentFileCommand = new SavePaymentFileCommand
            {
                FileStream = new MemoryStream(file.Content),
                ContentType = GetContentType(file.Filename),
                EventId = command.EventId
            };
            return await _savePaymentFileCommandHandler.Handle(savePaymentFileCommand, cancellationToken);
        }

        private static string GetContentType(string filename)
        {
            if (filename == null)
            {
                return null;
            }
            if (filename.EndsWith(".xml"))
            {
                return "text/xml";
            }
            if (filename.EndsWith(".tar.gz"))
            {
                return "application/x-gzip";
            }
            throw new ArgumentException($"Unknown file extension {filename}");
        }
    }
}

namespace EventRegistrar.Backend.Payments.Files.Fetch;

public class ProcessFetchedBankStatementsFileCommand : IRequest, IEventBoundRequest
{
    public Guid RawBankStatementFileId { get; set; }
    public Guid EventId { get; set; }
}

public class ProcessFetchedBankStatementsFileCommandHandler(IRepository<RawBankStatementsFile> files,
                                                            IRequestHandler<SavePaymentFileCommand> savePaymentFileCommandHandler)
    : IRequestHandler<ProcessFetchedBankStatementsFileCommand>
{
    public async Task Handle(ProcessFetchedBankStatementsFileCommand command, CancellationToken cancellationToken)
    {
        var file = await files.FirstAsync(fil => fil.Id == command.RawBankStatementFileId, cancellationToken);
        if (file.Processed != null)
        {
            return;
        }

        file.Processed = DateTimeOffset.Now;

        var savePaymentFileCommand = new SavePaymentFileCommand
                                     {
                                         FileStream = new MemoryStream(file.Content),
                                         ContentType = GetContentType(file.Filename),
                                         EventId = command.EventId
                                     };
        await savePaymentFileCommandHandler.Handle(savePaymentFileCommand, cancellationToken);
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
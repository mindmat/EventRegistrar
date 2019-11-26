using System.Collections.Generic;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using MediatR;

namespace EventRegistrar.Backend.Payments.Files.Fetch
{
    public class ProcessBankStatementsFileWhenFetched : IEventToCommandTranslation<BankStatementsFileImported>
    {
        public IEnumerable<IRequest> Translate(BankStatementsFileImported e)
        {
            yield return new ProcessFetchedBankStatementsFileCommand { RawBankStatementFileId = e.BankStatementsFileId };
        }
    }
}

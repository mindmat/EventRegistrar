﻿using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Mailing
{
    public class DeleteMailCommandHandler : IRequestHandler<DeleteMailCommand>
    {
        private readonly IRepository<Mail> _mails;

        public DeleteMailCommandHandler(IRepository<Mail> mails)
        {
            _mails = mails;
        }

        public async Task<Unit> Handle(DeleteMailCommand command, CancellationToken cancellationToken)
        {
            var mailToDelete = await _mails.FirstAsync(mail => mail.Id == command.MailId, cancellationToken);
            mailToDelete.Discarded = true;

            return Unit.Value;
        }
    }
}
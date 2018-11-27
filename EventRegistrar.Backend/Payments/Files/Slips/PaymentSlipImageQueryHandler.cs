using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Payments.Files.Slips
{
    public class PaymentSlipImageQueryHandler : IRequestHandler<PaymentSlipImageQuery, FileContentResult>
    {
        private readonly IQueryable<PaymentSlip> _slips;

        public PaymentSlipImageQueryHandler(IQueryable<PaymentSlip> slips)
        {
            _slips = slips;
        }

        public async Task<FileContentResult> Handle(PaymentSlipImageQuery query, CancellationToken cancellationToken)
        {
            var slip = await _slips.FirstAsync(slp => slp.EventId == query.EventId
                                                   && slp.Id == query.PaymentSlipId, cancellationToken);

            return new FileContentResult(slip.FileBinary, slip.ContentType) { FileDownloadName = slip.Filename };
        }
    }
}
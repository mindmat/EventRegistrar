using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Payments.Files.Slips;

public class PaymentSlipImageQuery : IRequest<FileContentResult>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid PaymentSlipId { get; set; }
}

public class PaymentSlipImageQueryHandler(IQueryable<PaymentSlip> slips) : IRequestHandler<PaymentSlipImageQuery, FileContentResult>
{
    public async Task<FileContentResult> Handle(PaymentSlipImageQuery query, CancellationToken cancellationToken)
    {
        var slip = await slips.FirstAsync(slp => slp.EventId == query.EventId
                                              && slp.Id == query.PaymentSlipId, cancellationToken);

        return new FileContentResult(slip.FileBinary, slip.ContentType) { FileDownloadName = slip.Filename };
    }
}
using EventRegistrar.Backend.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Payments.Files.Slips;

public class PaymentSlipImageQuery : IRequest<FileContentResult>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid PaymentSlipId { get; set; }
}

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
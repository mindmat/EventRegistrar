using EventRegistrar.Backend.Mailing.Compose;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Mailing.Templates;

//public class QrCodeImageQuery : IRequest<byte[]>, IEventBoundRequest
//{
//    public Guid EventId { get; set; }
//    public Guid RegistrationId { get; set; }
//}

//public class QrCodeImageQueryHandler(IQueryable<Registration> registrations,
//                                     MailComposer mailComposer)
//    : IRequestHandler<QrCodeImageQuery, byte[]>
//{
//    public async  Task<byte[]> Handle(QrCodeImageQuery query, CancellationToken cancellationToken)
//    {
//        var registration = await registrations.FirstAsync(reg => reg.EventId == query.EventId
//                                                              && reg.Id == query.RegistrationId,
//                                                          cancellationToken);
//        return await mailComposer.GenerateQrCode(registration);
//    }
//}
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.Responses;

namespace EventRegistrar.Backend.Hosting;

public class HostingQuery : IRequest<HostingOffersAndRequests>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class HostingOffersQueryHandler : IRequestHandler<HostingQuery, HostingOffersAndRequests>
{
    private readonly HostingMappingReader _hostingMappingReader;
    private readonly IQueryable<Registration> _registrations;

    public HostingOffersQueryHandler(HostingMappingReader hostingMappingReader,
                                     IQueryable<Registration> registrations)
    {
        _hostingMappingReader = hostingMappingReader;
        _registrations = registrations;
    }

    public async Task<HostingOffersAndRequests> Handle(HostingQuery query, CancellationToken cancellationToken)
    {
        var hostingMappings = await _hostingMappingReader.GetHostingMappings(query.EventId, cancellationToken);
        var result = new HostingOffersAndRequests();
        if (hostingMappings.QuestionOptionId_Offer == null
         && hostingMappings.QuestionOptionId_Request == null)
        {
            return new HostingOffersAndRequests();
        }

        var registrations = await _registrations.Where(reg => reg.EventId == query.EventId
                                                           && reg.State != RegistrationState.Cancelled
                                                           && reg.Responses!.Any(
                                                                  rsp => (hostingMappings.QuestionOptionId_Offer != null && rsp.QuestionOptionId == hostingMappings.QuestionOptionId_Offer)
                                                                      || (hostingMappings.QuestionOptionId_Request != null && rsp.QuestionOptionId == hostingMappings.QuestionOptionId_Request)))
                                                .Include(reg => reg.Responses)
                                                .OrderBy(reg => reg.IsOnWaitingList)
                                                .ThenByDescending(reg => reg.AdmittedAt)
                                                .ToListAsync(cancellationToken);
        if (hostingMappings.QuestionOptionId_Offer != null)
        {
            result.Offers = registrations.Where(reg => IsOptionTicked(reg.Responses!, hostingMappings.QuestionOptionId_Offer))
                                         .Select(reg => new HostingOffer
                                                        {
                                                            RegistrationId = reg.Id,
                                                            DisplayName = $"{reg.RespondentFirstName} {reg.RespondentLastName}",
                                                            Email = reg.RespondentEmail,
                                                            Phone = reg.Phone,
                                                            Language = reg.Language,
                                                            State = reg.State,
                                                            IsOnWaitingList = reg.IsOnWaitingList ?? false,
                                                            AdmittedAt = reg.AdmittedAt,
                                                            Location = GetResponseString(reg.Responses!, hostingMappings.QuestionId_Offer_Location),
                                                            CountTotal = GetResponseString(reg.Responses!, hostingMappings.QuestionId_Offer_CountTotal),
                                                            CountShared = GetResponseString(reg.Responses!, hostingMappings.QuestionId_Offer_CountShared),
                                                            Comment = GetResponseString(reg.Responses!, hostingMappings.QuestionId_Offer_Comment)
                                                        })
                                         .ToList();
        }

        if (hostingMappings.QuestionOptionId_Request != null)
        {
            result.Requests = registrations.Where(reg => IsOptionTicked(reg.Responses!, hostingMappings.QuestionOptionId_Request))
                                           .Select(reg => new HostingRequest
                                                          {
                                                              RegistrationId = reg.Id,
                                                              DisplayName = $"{reg.RespondentFirstName} {reg.RespondentLastName}",
                                                              Email = reg.RespondentEmail,
                                                              Phone = reg.Phone,
                                                              Language = reg.Language,
                                                              State = reg.State,
                                                              IsOnWaitingList = reg.IsOnWaitingList ?? false,
                                                              AdmittedAt = reg.AdmittedAt,
                                                              HostingPartner = GetResponseString(reg.Responses!, hostingMappings.QuestionId_Request_Partner),
                                                              ShareOkWithPartner = IsOptionTicked(reg.Responses!, hostingMappings.QuestionOptionId_Request_ShareOkWithPartner),
                                                              ShareOkWithRandom = IsOptionTicked(reg.Responses!, hostingMappings.QuestionOptionId_Request_ShareOkWithRandom),
                                                              TravelByCar = IsOptionTicked(reg.Responses!, hostingMappings.QuestionOptionId_Request_TravelByCar),
                                                              Comment = GetResponseString(reg.Responses!, hostingMappings.QuestionId_Request_Comment)
                                                          })
                                           .ToList();
        }

        return result;
    }

    private static bool IsOptionTicked(IEnumerable<Response> responses, Guid? questionOptionId)
    {
        return questionOptionId != null && responses.Any(rsp => rsp.QuestionOptionId == questionOptionId);
    }

    private static string? GetResponseString(IEnumerable<Response> responses, Guid? questionId)
    {
        return questionId == null
                   ? null
                   : responses.FirstOrDefault(rsp => rsp.QuestionId == questionId)?.ResponseString;
    }
}

public class HostingOffersAndRequests
{
    public IEnumerable<HostingOffer> Offers { get; set; } = null!;
    public IEnumerable<HostingRequest> Requests { get; set; } = null!;
}

public class HostingOffer
{
    public Guid RegistrationId { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? Language { get; set; }
    public string? Phone { get; set; }
    public RegistrationState? State { get; set; }
    public bool IsOnWaitingList { get; set; }
    public DateTimeOffset? AdmittedAt { get; set; }
    public string? Location { get; set; }
    public string? CountTotal { get; set; }
    public string? CountShared { get; set; }
    public string? Comment { get; set; }
}

public class HostingRequest
{
    public Guid RegistrationId { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? Language { get; set; }
    public string? Phone { get; set; }
    public RegistrationState State { get; set; }
    public bool IsOnWaitingList { get; set; }
    public DateTimeOffset? AdmittedAt { get; set; }
    public string? HostingPartner { get; set; }
    public bool ShareOkWithPartner { get; set; }
    public bool ShareOkWithRandom { get; set; }
    public bool TravelByCar { get; set; }
    public string? Comment { get; set; }
}
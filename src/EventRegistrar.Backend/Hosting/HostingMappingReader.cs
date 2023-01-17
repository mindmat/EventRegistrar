using EventRegistrar.Backend.RegistrationForms.Questions;
using EventRegistrar.Backend.RegistrationForms.Questions.Mappings;

namespace EventRegistrar.Backend.Hosting;

public class HostingMappingReader
{
    private readonly IQueryable<Question> _questions;
    private readonly IQueryable<QuestionOption> _questionOptions;

    private readonly IReadOnlyCollection<QuestionMappingType> _hostingQuestionMappings = new[]
                                                                                         {
                                                                                             QuestionMappingType.HostingOffer_Location,
                                                                                             QuestionMappingType.HostingOffer_CountTotal,
                                                                                             QuestionMappingType.HostingOffer_CountShared,
                                                                                             QuestionMappingType.HostingOffer_Comment,
                                                                                             QuestionMappingType.HostingRequest_Partner,
                                                                                             QuestionMappingType.HostingRequest_Comment
                                                                                         };

    private readonly IReadOnlyCollection<MappingType> _hostingOptionMappings = new[]
                                                                               {
                                                                                   MappingType.HostingOffer,
                                                                                   MappingType.HostingRequest,
                                                                                   MappingType.HostingRequest_ShareOkWithPartner,
                                                                                   MappingType.HostingRequest_ShareOkWithRandom,
                                                                                   MappingType.HostingRequest_TravelByCar
                                                                               };

    public HostingMappingReader(IQueryable<Question> questions,
                                IQueryable<QuestionOption> questionOptions)
    {
        _questions = questions;
        _questionOptions = questionOptions;
    }

    public async Task<HostingQuestionMappings> GetHostingMappings(Guid eventId, CancellationToken cancellationToken = default)
    {
        var questionMappings = await _questions.Where(qst => qst.RegistrationForm!.EventId == eventId
                                                          && _hostingQuestionMappings.Contains(qst.Mapping!.Value))
                                               .Select(qst => new QuestionMapping
                                                       (
                                                           qst.Id,
                                                           qst.Mapping!.Value
                                                       ))
                                               .ToListAsync(cancellationToken);
        var questionOptionMappings = await _questionOptions.Where(qop => qop.Question!.RegistrationForm!.EventId == eventId
                                                                      && qop.Mappings!.Any(qom => _hostingOptionMappings.Contains(qom.Type!.Value)))
                                                           .Select(qop => new QuestionOptionMapping
                                                                   (
                                                                       qop.Id,
                                                                       qop.Mappings!.FirstOrDefault(qom => _hostingOptionMappings.Contains(qom.Type!.Value))!.Type!.Value,
                                                                       qop.Mappings!.Where(qom => qom.RegistrableId != null).Select(qom => qom.RegistrableId!.Value)
                                                                   ))
                                                           .ToListAsync(cancellationToken);

        return new HostingQuestionMappings
               {
                   QuestionOptionId_Offer = questionOptionMappings.FirstOrDefault(qst => qst.Mapping == MappingType.HostingOffer)?.QuestionOptionId,
                   RegistrableId_Offer = questionOptionMappings.FirstOrDefault(qst => qst.Mapping == MappingType.HostingOffer)?.RegistrableIds.FirstOrDefault(),
                   QuestionId_Offer_Location = questionMappings.FirstOrDefault(qst => qst.Mapping == QuestionMappingType.HostingOffer_Location)?.QuestionId,
                   QuestionId_Offer_CountTotal = questionMappings.FirstOrDefault(qst => qst.Mapping == QuestionMappingType.HostingOffer_CountTotal)?.QuestionId,
                   QuestionId_Offer_CountShared = questionMappings.FirstOrDefault(qst => qst.Mapping == QuestionMappingType.HostingOffer_CountShared)?.QuestionId,
                   QuestionId_Offer_Comment = questionMappings.FirstOrDefault(qst => qst.Mapping == QuestionMappingType.HostingOffer_Comment)?.QuestionId,

                   QuestionOptionId_Request = questionOptionMappings.FirstOrDefault(qst => qst.Mapping == MappingType.HostingRequest)?.QuestionOptionId,
                   RegistrableId_Request = questionOptionMappings.FirstOrDefault(qst => qst.Mapping == MappingType.HostingRequest)?.RegistrableIds.FirstOrDefault(),
                   QuestionId_Request_Partner = questionMappings.FirstOrDefault(qst => qst.Mapping == QuestionMappingType.HostingRequest_Partner)?.QuestionId,
                   QuestionId_Request_Comment = questionMappings.FirstOrDefault(qst => qst.Mapping == QuestionMappingType.HostingRequest_Comment)?.QuestionId,
                   QuestionOptionId_Request_ShareOkWithPartner = questionOptionMappings.FirstOrDefault(qst => qst.Mapping == MappingType.HostingRequest_ShareOkWithPartner)?.QuestionOptionId,
                   QuestionOptionId_Request_ShareOkWithRandom = questionOptionMappings.FirstOrDefault(qst => qst.Mapping == MappingType.HostingRequest_ShareOkWithRandom)?.QuestionOptionId,
                   QuestionOptionId_Request_TravelByCar = questionOptionMappings.FirstOrDefault(qst => qst.Mapping == MappingType.HostingRequest_TravelByCar)?.QuestionOptionId
               };
    }
}

public record QuestionMapping(Guid QuestionId, QuestionMappingType Mapping);

public record QuestionOptionMapping(Guid QuestionOptionId, MappingType? Mapping, IEnumerable<Guid> RegistrableIds);

public class HostingQuestionMappings
{
    public Guid? QuestionOptionId_Offer { get; set; }
    public Guid? RegistrableId_Offer { get; set; }
    public Guid? QuestionId_Offer_Location { get; set; }
    public Guid? QuestionId_Offer_CountTotal { get; set; }
    public Guid? QuestionId_Offer_CountShared { get; set; }
    public Guid? QuestionId_Offer_Comment { get; set; }

    public Guid? QuestionOptionId_Request { get; set; }
    public Guid? RegistrableId_Request { get; set; }
    public Guid? QuestionId_Request_Partner { get; set; }
    public Guid? QuestionId_Request_Comment { get; set; }

    public Guid? QuestionOptionId_Request_ShareOkWithPartner { get; set; }
    public Guid? QuestionOptionId_Request_ShareOkWithRandom { get; set; }
    public Guid? QuestionOptionId_Request_TravelByCar { get; set; }
}
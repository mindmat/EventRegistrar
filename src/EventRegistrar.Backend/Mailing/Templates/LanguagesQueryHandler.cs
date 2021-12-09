using MediatR;

namespace EventRegistrar.Backend.Mailing.Templates;

public class LanguagesQueryHandler : IRequestHandler<LanguagesQuery, IEnumerable<LanguageItem>>
{
    public Task<IEnumerable<LanguageItem>> Handle(LanguagesQuery request, CancellationToken cancellationToken)
    {
        var languages = new[]
                        {
                            new LanguageItem { Acronym = "de", UserText = "Deutsch" },
                            new LanguageItem { Acronym = "en", UserText = "Englisch" }
                        };
        return Task.FromResult((IEnumerable<LanguageItem>)languages);
    }
}
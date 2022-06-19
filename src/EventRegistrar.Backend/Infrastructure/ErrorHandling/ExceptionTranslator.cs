using System.Net;

namespace EventRegistrar.Backend.Infrastructure.ErrorHandling;

public class ExceptionTranslator
{
    private readonly Lazy<ILookup<Type, IExceptionTranslation>> _exceptionTranslations;

    public ExceptionTranslator(IEnumerable<IExceptionTranslation> exceptionTranslations)
    {
        _exceptionTranslations = new Lazy<ILookup<Type, IExceptionTranslation>>(() => exceptionTranslations.ToLookup(ext => ext.ExceptionType));
    }

    public (object result, HttpStatusCode? httpCode) TranslateExceptionToUserText(Exception ex)
    {
        var match = FindMatchingTranslationRecursive(ex);
        if (match != null)
        {
            return (match.Value.translation.SelectMessage(match.Value.ex),
                       match.Value.translation.HttpCode);
        }

        // Fallback (no translation for this message)
        return (ex.Message, HttpStatusCode.InternalServerError);
    }

    private (IExceptionTranslation translation, Exception ex)? FindMatchingTranslationRecursive(Exception? ex)
    {
        if (ex == null)
        {
            return null;
        }

        var translations = _exceptionTranslations.Value[ex.GetType()];
        var translation = translations.FirstOrDefault(trs => trs.Filter(ex));
        if (translation == null)
        {
            return FindMatchingTranslationRecursive(ex.InnerException);
        }

        return (translation, ex);
    }
}
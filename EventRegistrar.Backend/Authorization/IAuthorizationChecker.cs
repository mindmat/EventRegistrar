using System.Threading.Tasks;

namespace EventRegistrar.Backend.Authorization
{
    public interface IAuthorizationChecker
    {
        Task ThrowIfUserHasNotRight(string eventAcronym, string requestTypeName);
    }
}
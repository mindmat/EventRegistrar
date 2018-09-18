using System;
using System.Linq;
using System.Threading.Tasks;
using EventRegistrar.Backend.Registrations;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Mailing.Compose
{
    public class PriceReader
    {
        private readonly IQueryable<Registration> _registrations;

        public PriceReader(IQueryable<Registration> registrations)
        {
            _registrations = registrations;
        }

        public async Task<decimal> GetPrice(Guid registrationId)
        {
            return await _registrations.Where(reg => reg.Id == registrationId)
                                       .Select(reg => (reg.Price ?? 0m) - reg.IndividualReductions
                                                                             .Select(ird => ird.Amount)
                                                                             .DefaultIfEmpty(0)
                                                                             .Sum())
                                       .FirstAsync();
        }
    }
}
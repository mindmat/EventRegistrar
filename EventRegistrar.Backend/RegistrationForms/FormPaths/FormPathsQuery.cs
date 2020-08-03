using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EventRegistrar.Backend.Registrations.Register;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.RegistrationForms.FormPaths
{
    public class FormPathsQuery : IRequest<IEnumerable<IRegistrationProcessConfiguration>>
    {
        public Guid EventId { get; set; }
    }

    public class FormPathsQueryHandler : IRequestHandler<FormPathsQuery, IEnumerable<IRegistrationProcessConfiguration>>
    {
        private readonly IQueryable<FormPath> _formPaths;

        public FormPathsQueryHandler(IQueryable<FormPath> formPaths)
        {
            _formPaths = formPaths;
        }

        public async Task<IEnumerable<IRegistrationProcessConfiguration>> Handle(FormPathsQuery query, CancellationToken cancellationToken)
        {
            var paths = await _formPaths.Where(fpt => fpt.RegistrationForm!.EventId == query.EventId)
                                        .Select(fpt => new
                                        {
                                            fpt.Id,
                                            fpt.RegistrationFormId,
                                            fpt.Type,
                                            fpt.Description,
                                            fpt.Configuration
                                        })
                                        .Where(fpt => fpt.Configuration != null)
                                        .ToListAsync(cancellationToken);

            // transfer properties from table
            paths.ForEach(fpt =>
            {
                switch (fpt.Configuration)
                {
                    case SingleRegistrationProcessConfiguration singleConfig:
                        singleConfig.Id = fpt.Id;
                        singleConfig.Description = fpt.Description;
                        singleConfig.RegistrationFormId = fpt.RegistrationFormId;
                        break;

                    case PartnerRegistrationProcessConfiguration partnerConfig:
                        partnerConfig.Id = fpt.Id;
                        partnerConfig.Description = fpt.Description;
                        partnerConfig.RegistrationFormId = fpt.RegistrationFormId;
                        break;
                }
            });

            return paths.Select(fpt => fpt.Configuration);
        }
    }
}
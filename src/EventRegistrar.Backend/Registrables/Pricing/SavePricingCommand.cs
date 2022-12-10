using EventRegistrar.Backend.Infrastructure.DataAccess;

namespace EventRegistrar.Backend.Registrables.Pricing;

public class SavePricingCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public IEnumerable<PricePackageDto>? Packages { get; set; }
}

public class SavePricingCommandHandler : IRequestHandler<SavePricingCommand>
{
    private readonly IRepository<PricePackage> _packages;
    private readonly IRepository<PricePackagePart> _parts;
    private readonly IRepository<RegistrableInPricePackagePart> _registrableInParts;

    public SavePricingCommandHandler(IRepository<PricePackage> packages,
                                     IRepository<PricePackagePart> parts,
                                     IRepository<RegistrableInPricePackagePart> registrableInParts)
    {
        _packages = packages;
        _parts = parts;
        _registrableInParts = registrableInParts;
    }

    public async Task<Unit> Handle(SavePricingCommand command, CancellationToken cancellationToken)
    {
        var packagesToSave = command.Packages;
        if (packagesToSave == null)
        {
            return Unit.Value;
        }

        var packages = await _packages.Where(frm => frm.EventId == command.EventId)
                                      .Include(ppg => ppg.Parts!)
                                      .ThenInclude(ppp => ppp.Registrables)
                                      .AsTracking()
                                      .ToListAsync(cancellationToken);

        foreach (var packageToSave in packagesToSave)
        {
            var package = packages.FirstOrDefault(qst => qst.Id == packageToSave.Id)
                       ?? _packages.InsertObjectTree(new PricePackage
                                                     {
                                                         Id = packageToSave.Id,
                                                         EventId = command.EventId
                                                     });

            package.Name = packageToSave.Name ?? string.Empty;
            package.Price = packageToSave.Price;
            package.AllowAsFallback = packageToSave.AllowAsFallback;

            foreach (var partToSave in packageToSave.Parts ?? Enumerable.Empty<PricePackagePartDto>())
            {
                var part = package.Parts?.FirstOrDefault(ppp => ppp.Id == partToSave.Id)
                        ?? _parts.InsertObjectTree(new PricePackagePart
                                                   {
                                                       Id = partToSave.Id,
                                                       PricePackageId = package.Id
                                                   });

                part.SelectionType = partToSave.SelectionType;
                part.PriceAdjustment = partToSave.PriceAdjustment;

                var registrableIdsToSave = (partToSave.RegistrableIds ?? Enumerable.Empty<Guid>()).ToList();
                var existingRegistrableIds = part.Registrables?
                                                 .Select(rip => rip.RegistrableId)
                                                 .ToList()
                                          ?? new List<Guid>();
                var addedIds = registrableIdsToSave.Except(existingRegistrableIds)
                                                   .ToList();
                var removedIds = existingRegistrableIds.Except(registrableIdsToSave)
                                                       .ToList();
                foreach (var addedId in addedIds)
                {
                    _registrableInParts.InsertObjectTree(new RegistrableInPricePackagePart
                                                         {
                                                             Id = Guid.NewGuid(),
                                                             PricePackagePartId = partToSave.Id,
                                                             RegistrableId = addedId
                                                         });
                }

                if (removedIds.Any() && part.Registrables != null)
                {
                    part.Registrables = part.Registrables!
                                            .Where(rip => !removedIds.Contains(rip.RegistrableId))
                                            .ToList();
                }
            }
        }

        return Unit.Value;
    }
}
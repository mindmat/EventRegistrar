using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

namespace EventRegistrar.Backend.Registrables.Pricing;

public class SavePricingCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public IEnumerable<PricePackageDto>? Packages { get; set; }
}

public class SavePricingCommandHandler(
    IRepository<PricePackage> packages,
    IRepository<PricePackagePart> parts,
    IRepository<RegistrableInPricePackagePart> registrableInParts,
    ChangeTrigger changeTrigger)
    : IRequestHandler<SavePricingCommand>
{
    public async Task Handle(SavePricingCommand command, CancellationToken cancellationToken)
    {
        var packagesToSave = command.Packages?.ToList();
        if (packagesToSave == null)
        {
            return;
        }

        var existingPackages = await packages.AsTracking()
                                             .Where(frm => frm.EventId == command.EventId)
                                             .Include(ppg => ppg.Parts!)
                                             .ThenInclude(ppp => ppp.Registrables)
                                             .ToListAsync(cancellationToken);

        // remove deleted multi mappings
        var packagesToDelete = existingPackages.ExceptBy(packagesToSave.Select(ppk => ppk.Id), ppk => ppk.Id)
                                               .ToList();
        foreach (var packageToDelete in packagesToDelete)
        {
            packages.Remove(packageToDelete);
        }

        foreach (var packageToSave in packagesToSave)
        {
            var package = existingPackages.FirstOrDefault(qst => qst.Id == packageToSave.Id)
                       ?? packages.InsertObjectTree(new PricePackage
                                                    {
                                                        Id = packageToSave.Id,
                                                        EventId = command.EventId
                                                    });

            package.Name = packageToSave.Name ?? string.Empty;
            package.Price = packageToSave.Price;
            package.AllowAsAutomaticFallback = packageToSave.AllowAsAutomaticFallback;
            package.AllowAsManualFallback = packageToSave.AllowAsManualFallback;
            package.IsCorePackage = packageToSave.IsCorePackage;
            package.ShowInOverview = packageToSave.ShowInOverview;

            var partIdsNew = packageToSave.Parts!
                                          .Select(prt => prt.Id)
                                          .ToList();
            foreach (var removedPart in package.Parts!.Where(prt => !partIdsNew.Contains(prt.Id)).ToList())
            {
                package.Parts!.Remove(removedPart);
            }

            foreach (var partToSave in packageToSave.Parts ?? Enumerable.Empty<PricePackagePartDto>())
            {
                var part = package.Parts?.FirstOrDefault(ppp => ppp.Id == partToSave.Id)
                        ?? parts.InsertObjectTree(new PricePackagePart
                                                  {
                                                      Id = partToSave.Id,
                                                      PricePackageId = package.Id
                                                  });

                part.SelectionType = partToSave.SelectionType;
                part.PriceAdjustment = partToSave.PriceAdjustment;
                part.ShowInMailSpotList = partToSave.ShowInMailSpotList;

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
                    registrableInParts.InsertObjectTree(new RegistrableInPricePackagePart
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

        changeTrigger.QueryChanged<PricingQuery>(command.EventId);
    }
}
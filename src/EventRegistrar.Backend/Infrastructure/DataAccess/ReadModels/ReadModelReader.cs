using System.Text.Json;

using EventRegistrar.Backend.Infrastructure.Mediator;

namespace EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

public class ReadModelReader
{
    private readonly IQueryable<ReadModel> _readModels;
    private static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    public ReadModelReader(IQueryable<ReadModel> readModels)
    {
        _readModels = readModels;
    }

    public async Task<SerializedJson<T>> Get<T>(string queryName,
                                                Guid eventId,
                                                Guid? rowId,
                                                CancellationToken cancellationToken)
        where T : class
    {
        var readModel = await _readModels.Where(rdm => rdm.QueryName == queryName
                                                    && rdm.EventId == eventId
                                                    && rdm.RowId == rowId)
                                         .Select(rdm => rdm.ContentJson)
                                         .FirstAsync(cancellationToken);
        return new SerializedJson<T>(readModel);
    }

    public async Task<T> GetDeserialized<T>(string queryName,
                                            Guid eventId,
                                            Guid? rowId,
                                            CancellationToken cancellationToken)
    {
        var readModel = await _readModels.Where(rdm => rdm.QueryName == queryName
                                                    && rdm.EventId == eventId
                                                    && rdm.RowId == rowId)
                                         .Select(rdm => rdm.ContentJson)
                                         .FirstAsync(cancellationToken);
        return JsonSerializer.Deserialize<T>(readModel, _serializerOptions)!;
    }

    public async Task<IEnumerable<T>> GetDeserialized<T>(string queryName,
                                                         Guid eventId,
                                                         IEnumerable<Guid> rowIds,
                                                         CancellationToken cancellationToken)
    {
        var readModels = await _readModels.Where(rdm => rdm.QueryName == queryName
                                                     && rdm.EventId == eventId
                                                     && rdm.RowId != null
                                                     && rowIds.Contains(rdm.RowId.Value))
                                          .Select(rdm => rdm.ContentJson)
                                          .ToListAsync(cancellationToken);
        return readModels.Select(rmd => JsonSerializer.Deserialize<T>(rmd, _serializerOptions)!);
    }
}
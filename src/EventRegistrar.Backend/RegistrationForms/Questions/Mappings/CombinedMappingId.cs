namespace EventRegistrar.Backend.RegistrationForms.Questions.Mappings;

public class CombinedMappingId
{
    public CombinedMappingId(string combinedId)
    {
        var splits = combinedId.Split('|').ToList();
        if (splits.Count != 3)
        {
            throw new ArgumentOutOfRangeException(nameof(combinedId));
        }

        if (Enum.TryParse<MappingType>(splits[0], out var type))
        {
            Type = type;
        }

        if (Guid.TryParse(splits[1], out var id))
        {
            Id = id;
        }

        var language = splits[2];
        if (!string.IsNullOrWhiteSpace(language))
        {
            Language = language;
        }
    }

    public CombinedMappingId(MappingType? type, Guid? id, string? language)
    {
        Type = type;
        Id = id;
        Language = language;
    }

    public override string ToString()
    {
        return $"{Type}|{Id}|{Language}";
    }

    public MappingType? Type { get; }
    public Guid? Id { get; set; }
    public string? Language { get; set; }
}
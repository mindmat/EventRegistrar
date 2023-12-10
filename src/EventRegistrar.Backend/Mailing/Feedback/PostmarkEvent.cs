namespace EventRegistrar.Backend.Mailing.Feedback;

public class PostmarkEventDelivery
{
    public string? RecordType { get; set; }
    public int? ServerID { get; set; }
    public string? MessageStream { get; set; }
    public string? MessageID { get; set; }
    public string? Recipient { get; set; }
    public string? Tag { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? Details { get; set; }
    public IDictionary<string, string>? Metadata { get; set; }
}

public class PostmarkEventBounce
{
    public string? RecordType { get; set; }
    public int? ID { get; set; }
    public string? Type { get; set; }
    public int? TypeCode { get; set; }
    public string? Name { get; set; }
    public string? Tag { get; set; }
    public string? MessageID { get; set; }
    public int? ServerID { get; set; }
    public string? MessageStream { get; set; }
    public string? Description { get; set; }
    public string? Details { get; set; }
    public string? Email { get; set; }
    public string? From { get; set; }
    public DateTime? BouncedAt { get; set; }
    public bool? DumpAvailable { get; set; }
    public bool? Inactive { get; set; }
    public bool? CanActivate { get; set; }
    public string? Subject { get; set; }
    public string? Content { get; set; }
    public IDictionary<string, string>? Metadata { get; set; }
}

public class PostmarkEventOpen
{
    public string? RecordType { get; set; }
    public bool FirstOpen { get; set; }
    public Client? Client { get; set; }
    public OS? OS { get; set; }
    public string? Platform { get; set; }
    public string? UserAgent { get; set; }
    public int? ReadSeconds { get; set; }
    public Geo? Geo { get; set; }
    public string? MessageID { get; set; }
    public string? MessageStream { get; set; }
    public DateTimeOffset? ReceivedAt { get; set; }
    public string? Tag { get; set; }
    public string? Recipient { get; set; }
    public IDictionary<string, string>? Metadata { get; set; }
}

public class Client
{
    public string? Name { get; set; }
    public string? Company { get; set; }
    public string? Family { get; set; }
}

public class OS
{
    public string? Name { get; set; }
    public string? Company { get; set; }
    public string? Family { get; set; }
}

public class Geo
{
    public string? CountryISOCode { get; set; }
    public string? Country { get; set; }
    public string? RegionISOCode { get; set; }
    public string? Region { get; set; }
    public string? City { get; set; }
    public string? Zip { get; set; }
    public string? Coords { get; set; }
    public string? IP { get; set; }
}

public class PostmarkSpamReport
{
    public string? RecordType { get; set; }
    public int? ID { get; set; }
    public string? Type { get; set; }
    public int? TypeCode { get; set; }
    public string? Tag { get; set; }
    public string? MessageID { get; set; }
    public string? Details { get; set; }
    public string? Email { get; set; }
    public string? From { get; set; }
    public DateTime? BouncedAt { get; set; }
    public bool? Inactive { get; set; }
    public bool? DumpAvailable { get; set; }
    public bool? CanActivate { get; set; }
    public string? Subject { get; set; }
    public int? ServerID { get; set; }
    public string? MessageStream { get; set; }
    public string? Content { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public IDictionary<string, string>? Metadata { get; set; }
}
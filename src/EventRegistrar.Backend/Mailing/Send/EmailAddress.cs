namespace EventRegistrar.Backend.Mailing.Send;

public class EmailAddress
{
    public required string Email { get; set; }
    public string? Name { get; set; }

    public string ToNameMail()
    {
        return Name == null
                   ? Email
                   : $"{Name} <{Email}>";
    }
}
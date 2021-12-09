namespace EventRegistrar.Backend.Registrations;

public class PhoneNormalizer
{
    public string NormalizePhone(string phone)
    {
        if (string.IsNullOrEmpty(phone)) return phone;
        var phoneNormalized = phone.Replace(" ", "").Replace("-", "");

        // Replace 0041 with +41
        if (phoneNormalized.StartsWith("00")) phoneNormalized = $"+{phoneNormalized.Remove(0, 2)}";

        // add swiss prefix: 079 123 45 67 -> +41 79 123 45 67
        if (phoneNormalized.StartsWith("07") && phoneNormalized.Length == 10)
            phoneNormalized = $"+41{phoneNormalized.Remove(0, 1)}";

        // 41... -> +41...
        if (phoneNormalized.StartsWith("41") && phoneNormalized.Length == 11) phoneNormalized = $"+{phoneNormalized}";
        return phoneNormalized;
    }
}
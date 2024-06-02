using EventRegistrar.Backend.Infrastructure.Configuration;

namespace EventRegistrar.Backend.Payments.Account;

public class BankAccountConfiguration : IConfigurationItem
{
    public string? Iban { get; set; }
    public string? AccountHolderName { get; set; }
    public string? AccountHolderStreet { get; set; }
    public string? AccountHolderHouseNo { get; set; }
    public string? AccountHolderPostalCode { get; set; }
    public string? AccountHolderTown { get; set; }
    public string? AccountHolderCountryCode { get; set; }
}

public class DefaultBankAccountConfiguration : BankAccountConfiguration, IDefaultConfigurationItem
{
    //public DefaultQrBillConfiguration()
    //{
    //    Iban = "CH68 0870 4050 0554 1114 7";
    //    AccountHolderName = "Swingmachine Bern / Leapin Lindy";
    //    AccountHolderStreet = "Reiterstrasse";
    //    AccountHolderHouseNo = "2";
    //    AccountHolderPostalCode = "3013";
    //    AccountHolderTown = "Bern";
    //    AccountHolderCountryCode = "CH";
    //}
}
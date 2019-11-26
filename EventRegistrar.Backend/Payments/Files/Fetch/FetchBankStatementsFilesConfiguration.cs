using EventRegistrar.Backend.Infrastructure.Configuration;

namespace EventRegistrar.Backend.Payments.Files.Fetch
{
    public class FetchBankStatementsFilesConfiguration : IConfigurationItem
    {
        public string Server { get; set; }
        public string ContractIdentifier { get; set; }
        public string KeyName { get; set; }
        public string Passphrase { get; set; }
        public string Directory { get; set; }
    }

    public class TestFetchBankStatementsFilesConfiguration : FetchBankStatementsFilesConfiguration, IDefaultConfigurationItem
    {
        public TestFetchBankStatementsFilesConfiguration()
        {
            Server = "fdsbc.post.ch";
        }
    }
}

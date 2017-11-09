using EventRegistrator.Functions.Payments;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EventRegistrator.Functions.Test.Payment
{
    [TestClass]
    public class EmailExtractorTest
    {
        private const string Email1 = "papa.schlumpf@google.com";
        private const string Email2 = "donald.duck@outlook.com";

        [TestMethod]
        [DataRow(Email1, Email1)]
        [DataRow(Email2, Email2)]
        [DataRow("Payment Leapin' Lindy for " + Email1, Email1)]
        [DataRow("Payment for both " + Email1 + " and " + Email2, Email1 + ";" + Email2)]
        public void ExtractPureEmail()
        {
            var result = EmailExtractor.TryExtractEmailFromInfo(Email1);
            //result.ShouldBe(Email1);
        }
    }
}
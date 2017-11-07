using System.IO;
using EventRegistrator.Functions.Payment;
using EventRegistrator.Functions.Test.Properties;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace EventRegistrator.Functions.Test.Payment
{
    [TestClass]
    public class CamtParserTest
    {
        [TestMethod]
        public void ParseCamt()
        {
            var camt = CamtParser.Parse(new MemoryStream(Resources.camt053Example));
            camt.Owner.ShouldBe("Papa Schlumpf");
            camt.Account.ShouldBe("CH3309000000462980261");

            camt.Entries.Count.ShouldBe(7);
        }
    }
}
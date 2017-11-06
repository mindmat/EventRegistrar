using System.IO;
using EventRegistrator.Functions.Payment;
using EventRegistrator.Functions.Test.Properties;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EventRegistrator.Functions.Test.Payment
{
    [TestClass]
    public class CamtParserTest
    {
        [TestMethod]
        public void ParseCamt()
        {
            var camt = CamtParser.Parse(new MemoryStream(Resources.camt053Example));
        }
    }
}
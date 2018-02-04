using System;
using System.Threading.Tasks;
using EventRegistrator.Functions.Sms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EventRegistrator.Functions.Test.Sms
{
    [TestClass]
    public class TemplateParameterFinderTest
    {
        [TestMethod]
        public async Task FillSmsTemplate()
        {
            const string template = "Hallo {{FirstName}}, hast du die Mails vom Leapin' Lindy erhalten? Bitte melde dich in den nächsten 24h, ob du immer noch dabei bist oder deine Anmeldung stornieren willst. Liebe Grüsse, das Leapin' Lindy-Team";
            var content = await new TemplateParameterFinder().Fill(template, new Guid("3C42DD94-5705-4942-8846-684322CAE3E4"));
            throw new Exception(content);
        }
    }
}
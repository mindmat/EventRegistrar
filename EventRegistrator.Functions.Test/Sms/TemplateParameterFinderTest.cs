using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EventRegistrator.Functions.Test.Sms
{
    [TestClass]
    public class TemplateParameterFinderTest
    {
        [TestMethod]
        public void FillSmsTemplate()
        {
            var template = "Hallo {{FirstName}}, hast du die Mails vom Leapin' Lindy erhalten? Bitte melde dich in den nächsten 24h, ob du immer noch dabei bist oder deine Anmeldung stornieren willst. Liebe Grüsse, das Leapin' Lindy-Team";
            //var finder = new TemplateParameterFinder(template);
        }
    }
}
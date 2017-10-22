using EventRegistrator.Functions.Mailing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace EventRegistrator.Functions.Test.Mailing
{
    [TestClass]
    public class TemplateFillerTest
    {
        [TestMethod]
        public void TestExtractParametersSingleBracket()
        {
            const string template = "Hallo {Firstname} {LastName}";

            var filler = new TemplateFiller(template);

            filler.Parameters.Count.ShouldBe(0);
        }

        [TestMethod]
        public void TestExtractParameters()
        {
            const string template = "Hallo {{Firstname}} {{LastName}}";

            var filler = new TemplateFiller(template);

            filler.Parameters.Count.ShouldBe(2);
            filler["Firstname"].ShouldBeNullOrEmpty();
            filler["LastName"].ShouldBeNullOrEmpty();
        }

        [TestMethod]
        public void TestFill()
        {
            const string template = "Hallo {{Firstname}} {{LastName}}";

            var filler = new TemplateFiller(template)
            {
                ["Firstname"] = "Peter",
                ["LastName"] = "Jackson"
            };

            var result = filler.Fill();

            result.ShouldBe("Hallo Peter Jackson");
        }
    }
}
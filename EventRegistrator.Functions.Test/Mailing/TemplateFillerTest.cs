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
        public void TestExtractPrefixes()
        {
            const string template = "Hallo {{Leader.Firstname}} & {{Follower.Firstname}}";

            var filler = new TemplateFiller(template);

            filler.Prefixes.ShouldBe(new[] { "LEADER", "FOLLOWER" }, true);
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
        public void TestExtractParametersWithDot()
        {
            const string template = "Hallo {{Firstname}}, hallo {{Follower.FirstName}}";

            var filler = new TemplateFiller(template);

            filler.Parameters.Count.ShouldBe(2);
            filler["Firstname"].ShouldBeNullOrEmpty();
            filler["Follower.FirstName"].ShouldBeNullOrEmpty();
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
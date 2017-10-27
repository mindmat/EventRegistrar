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

        [TestMethod]
        public void TestTemplate()
        {
            const string template = @"<table>
  <tbody>
    <tr>
      <td><br /><strong>Hallo {{FirstName}}!</strong><br />
        <p>Herzlichen Dank f&uuml;r deine Anmeldung zum Leapin'' Lindy 2018! <br />Beiliegend findest du deine Anmeldedetails. Bitte erw&auml;hne deine <br />Mailadresse bei der &Uuml;berweisung des Kursgeldes.</p>
        <p>Wir freuen uns, dass du dabei bist!</p>
        <p>Das Leapin'' Lindy Team</p>
        <hr />
        <p>F&uuml;r dich ist ein Platz reserviert bei:<br />{{SeatList}}</p>
        <table>
          <tbody>
            <tr>
              <td><strong>Name</strong></td>
              <td>{{FirstName}} {{LastName}}</td>
            </tr>
            <tr>
              <td><strong>Wohnort</strong></td>
              <td>{{City}}</td>
            </tr>
            <tr>
              <td><strong>Telefon</strong></td>
              <td>{{Phone}}</td>
            </tr>
            <tr>
              <td><strong>Helfereinsatz</strong></td>
              <td>{{Volunteer}}</td>
            </tr>
            <tr>
              <td><strong>Erm&auml;ssigung</strong></td>
              <td>{{Reduction}}</td>
            </tr>
            <tr>
              <td><strong>Bemerkung</strong></td>
              <td>{{Comments}}</td>
            </tr>
          </tbody>
        </table>
        <p>&nbsp;</p>
      </td>
    </tr>
    <tr>
      <td><hr />
        <p>Zahlungsinformationen:</p>
        <p><strong>WICHTIG: Mache eine &Uuml;berweisung pro Anmeldung und vermerke&nbsp;deine&nbsp;Emailadresse!&nbsp;</strong></p>
        <p>Deine Zahlung muss innerhalb von 14 Tagen nach Erhalt dieser Email bei uns eintreffen. Wenn wir deine Zahlung erhalten haben, schicken wir dir eine Buchungsbest&auml;tigung. Bezahlst du nicht innerhalb der Frist, wird deine Registrierung m&ouml;glicherweise storniert. Bitte beachte die untenstehende Regelung bez&uuml;glich Absage und R&uuml;ckerstattung.</p>
        <p>Konto: 60-224741-6<br />Swing Machine Bern / Leapin Lindy<br />3000 Bern&nbsp;</p>
        <p>IBAN: CH93 0900 0000 6022 4741 6<br />BLZ: 09000<br />BIC: POFICHBEXXX</p>
        <p><strong>Bankadresse</strong><br />Swiss Post - PostFinance<br />Nordring 8<br />3030 Bern<br />Switzerland</p>
        <p>Bitte &uuml;berweise immer Schweizer Franken. Spesen zu Lasten des Absenders.&nbsp;</p>
        <p><strong>Absagen und R&uuml;ckerstattung:</strong><br />Wenn du deine Anmeldung zur&uuml;cknehmen musst, gelten folgende Regeln f&uuml;r die R&uuml;ckerstattungen:</p>
        <p>Absage bis zum 10. Januar 2018: volle R&uuml;ckerstattung<br />Januar - 31. Januar 2018: 50% R&uuml;ckerstattung<br />Februar 2018 und sp&auml;ter: keine R&uuml;ckerstattung&nbsp;</p>
        <p>Wenn du deine Anmeldung annulieren willst, kontaktiere uns in jedem Fall damit wir deinen Platz freigeben k&ouml;nnen.</p>
        <p><strong>WICHTIG</strong>: Krankheit und Verletzung sind in der obigen Regelung eingeschlossen.</p>
        <p>Kannst du kurzfristig nicht teilnehmen, werden wir versuchen, einen Ersatz auf der Warteliste zu finden. Gelingt uns dies, ist eine R&uuml;ckerstattung m&ouml;glich, aber nicht garantiert.<br />Im Grundsatz beh&auml;lt die Regelung G&uuml;ltigkeit.&nbsp;</p>
        <p>In jedem Falle steht es dir frei, selbst einen Ersatz f&uuml;r deinen Workshopplatz oder Partypass zu finden. Bitte kontaktiere uns vor dem Check-In, wenn du deine Anmeldung an eine andere&nbsp; Person &uuml;bertragen hast. Du bleibst aber weiterhin f&uuml;r die Zahlung des Kursgeledes verantwortlich.</p>
        <p><strong>Versicherung<br /></strong>Versicherung ist Sache der Teilnehmenden. Der Veranstalter lehnt jede Haftung ab.</p>
      </td>
    </tr>
  </tbody>
</table>";

            var filler = new TemplateFiller(template);
            filler.Parameters.Count.ShouldBe(2);

            //var result = filler.Fill();

            //result.ShouldBe("Hallo Peter Jackson");
        }
    }
}
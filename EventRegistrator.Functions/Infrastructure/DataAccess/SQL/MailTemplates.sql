DELETE FROM dbo.MailTemplates WHERE Id IN ('A3A7144D-F51F-4B51-ACDD-9FE96F197506')

INSERT INTO dbo.MailTemplates(Id, EventId, [Type], ContentType, [Subject], SenderMail, SenderName, [Language], Template)
VALUES ('A3A7144D-F51F-4B51-ACDD-9FE96F197506', '762A93A4-56E0-402C-B700-1CFB3362B39D', 1, 2, 'Anmeldebestätigung', 'noreply@leapinlindy.ch', 'Leapin'' Lindy', 'de', 
'<table cellspacing="0" cellpadding="0" align="center">
  <tbody>
    <tr>
      <td style="text-align: center;"><img src="https://scontent-frx5-1.xx.fbcdn.net/v/t31.0-8/22042178_1480177115410174_419267934679857047_o.jpg?oh=d6833f06299ceb688c53b8439413a696&amp;oe=5A7EF2E1" width="800" height="304" /></td>
    </tr>
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
</table>')
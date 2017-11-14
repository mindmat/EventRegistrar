begin tran

DELETE FROM dbo.MailTemplates WHERE EventId = '762A93A4-56E0-402C-B700-1CFB3362B39D';

INSERT INTO dbo.MailTemplates(Id, EventId, [Type], ContentType, [Subject], SenderMail, SenderName, [Language], Template)
VALUES ('A3A7144D-F51F-4B51-ACDD-9FE96F197506', '762A93A4-56E0-402C-B700-1CFB3362B39D', 1 /*SingleRegistrationAccepted*/, 2, 'Anmeldebestätigung', 'noreply@leapinlindy.ch', 'Leapin'' Lindy', 'de', 
'<table cellspacing="0" cellpadding="0" align="center">
  <tbody>
    <tr>
      <td style="text-align: center;"><img src="https://scontent-frx5-1.xx.fbcdn.net/v/t31.0-8/22042178_1480177115410174_419267934679857047_o.jpg?oh=d6833f06299ceb688c53b8439413a696&amp;oe=5A7EF2E1" width="800" height="304" /></td>
    </tr>
    <tr>
      <td><br /><strong>Hallo {{FirstName}}</strong><br />
        <p>Herzlichen Dank f&uuml;r deine Anmeldung zum Leapin'' Lindy 2018!<br />Wir haben einen Platz f&uuml;r dich reserviert, bitte bezahle die Anmeldegeb&uuml;hr von CHF{{Price}} in den n&auml;chsten 5 Arbeitstagen (Details siehe unten).</p>
        <p>Wir freuen uns, dass du dabei bist!</p>
        <p>Das Leapin'' Lindy Team</p>
        <hr />
        <p>Du hast folgendes gebucht:<br />{{SeatList}}</p>
        <table style="width: 826px;">
          <tbody>
            <tr>
              <td style="width: 130px;"><strong>Name</strong></td>
              <td style="width: 700px;">{{FirstName}} {{LastName}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Wohnort</strong></td>
              <td style="width: 700px;">{{City}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Telefon</strong></td>
              <td style="width: 700px;">{{Phone}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Helfereinsatz</strong></td>
              <td style="width: 700px;">{{Volunteer}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Erm&auml;ssigung</strong></td>
              <td style="width: 700px;">{{Reduction}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Bemerkung</strong></td>
              <td style="width: 700px;">{{Comments}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>AGBs</strong></td>
              <td style="width: 700px;">{{AcceptTerms}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Preis</strong></td>
              <td style="width: 700px;">CHF{{Price}}</td>
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
        <p>Deine Zahlung muss innerhalb von 5 Arbeitstagen nach Erhalt dieser Email bei uns eintreffen. Wenn wir deine Zahlung erhalten haben, schicken wir dir eine Buchungsbest&auml;tigung. Bezahlst du nicht innerhalb der Frist, wird deine Registrierung m&ouml;glicherweise storniert. Bitte beachte die untenstehende Regelung bez&uuml;glich Absage und R&uuml;ckerstattung.</p>
        <p>Konto: 60-224741-6<br />Swing Machine Bern / Leapin Lindy<br />3000 Bern&nbsp;</p>
        <p>IBAN: CH93 0900 0000 6022 4741 6<br />BLZ: 09000<br />BIC: POFICHBEXXX</p>
        <p><strong>Bankadresse</strong><br />Swiss Post - PostFinance<br />Nordring 8<br />3030 Bern<br />Switzerland</p>
        <p>Bitte &uuml;berweise immer Schweizer Franken. Spesen zu Lasten des Absenders.&nbsp;</p>
        <p><strong>Absagen und R&uuml;ckerstattung:</strong><br />Wenn du deine Anmeldung zur&uuml;cknehmen musst, gelten folgende Regeln f&uuml;r die R&uuml;ckerstattungen:</p>
        <p>Absage bis zum 10. Januar 2018: volle R&uuml;ckerstattung<br />11. Januar - 31. Januar 2018: 50% R&uuml;ckerstattung<br />Februar 2018 und sp&auml;ter: keine R&uuml;ckerstattung&nbsp;</p>
        <p>Wenn du deine Anmeldung annulieren willst, kontaktiere uns in jedem Fall damit wir deinen Platz freigeben k&ouml;nnen.</p>
        <p><strong>WICHTIG</strong>: Krankheit und Verletzung sind in der obigen Regelung eingeschlossen.</p>
        <p>Kannst du kurzfristig nicht teilnehmen, werden wir versuchen, einen Ersatz auf der Warteliste zu finden. Gelingt uns dies, ist eine R&uuml;ckerstattung m&ouml;glich, aber nicht garantiert.<br />Im Grundsatz beh&auml;lt die Regelung G&uuml;ltigkeit.&nbsp;</p>
        <p>In jedem Falle steht es dir frei, selbst einen Ersatz f&uuml;r deinen Workshopplatz oder Partypass zu finden. Bitte kontaktiere uns vor dem Check-In, wenn du deine Anmeldung an eine andere&nbsp; Person &uuml;bertragen hast. Du bleibst aber weiterhin f&uuml;r die Zahlung des Kursgeledes verantwortlich.</p>
        <p><strong>Versicherung<br /></strong>Versicherung ist Sache der Teilnehmenden. Der Veranstalter lehnt jede Haftung ab.</p>
      </td>
    </tr>
  </tbody>
</table>')

INSERT INTO dbo.MailTemplates(Id, EventId, [Type], ContentType, [Subject], SenderMail, SenderName, [Language], Template)
VALUES ('77204060-8E30-4055-B797-92A324A00214', '762A93A4-56E0-402C-B700-1CFB3362B39D', 2 /*SingleRegistrationOnWaitingList*/, 2, 'Anmeldebestätigung (Warteliste)', 'noreply@leapinlindy.ch', 'Leapin'' Lindy', 'de', 
'<table cellspacing="0" cellpadding="0" align="center">
  <tbody>
    <tr>
      <td style="text-align: center;"><img src="https://scontent-frx5-1.xx.fbcdn.net/v/t31.0-8/22042178_1480177115410174_419267934679857047_o.jpg?oh=d6833f06299ceb688c53b8439413a696&amp;oe=5A7EF2E1" width="800" height="304" /></td>
    </tr>
    <tr>
      <td><br /><strong>Hallo {{FirstName}}</strong><br />
        <p>Herzlichen Dank f&uuml;r deine Anmeldung zum Leapin'' Lindy 2018!<br /><strong>Du bist auf der Warteliste, bitte bezahle die Anmeldegeb&uuml;hr noch nicht.</strong></p>
        <p>Das Leapin'' Lindy Team</p>
        <hr />
        <p>Du hast folgendes gebucht:<br />{{SeatList}}</p>
        <table style="width: 474px;">
          <tbody>
            <tr>
              <td style="width: 130px;"><strong>Name</strong></td>
              <td style="width: 600px;">{{FirstName}} {{LastName}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Wohnort</strong></td>
              <td style="width: 600px;">{{City}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Telefon</strong></td>
              <td style="width: 600px;">{{Phone}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Helfereinsatz</strong></td>
              <td style="width: 600px;">{{Volunteer}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Erm&auml;ssigung</strong></td>
              <td style="width: 600px;">{{Reduction}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Bemerkung</strong></td>
              <td style="width: 600px;">{{Comments}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>AGBs</strong></td>
              <td style="width: 600px;">{{AcceptTerms}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Preis</strong></td>
              <td style="width: 600px;">CHF{{Price}}</td>
            </tr>
          </tbody>
        </table>
      </td>
    </tr>
  </tbody>
</table>');



INSERT INTO dbo.MailTemplates(Id, EventId, [Type], ContentType, [Subject], SenderMail, SenderName, [Language], Template)
VALUES ('E5AB6F99-8A43-4D99-9CCD-B10B4F711B5C', '762A93A4-56E0-402C-B700-1CFB3362B39D', 11 /*DoubleRegistrationFirstPartnerAccepted*/, 2, 'Anmeldebestätigung', 'noreply@leapinlindy.ch', 'Leapin'' Lindy', 'de', 
'<table cellspacing="0" cellpadding="0" align="center">
  <tbody>
    <tr>
      <td style="text-align: center;"><img src="https://scontent-frx5-1.xx.fbcdn.net/v/t31.0-8/22042178_1480177115410174_419267934679857047_o.jpg?oh=d6833f06299ceb688c53b8439413a696&amp;oe=5A7EF2E1" width="800" height="304" /></td>
    </tr>
    <tr>
      <td><br /><strong>Hallo {{FirstName}}</strong><br />
        <p>Herzlichen Dank f&uuml;r deine Anmeldung zum Leapin'' Lindy 2018!<br />Wir haben einen Platz f&uuml;r dich reserviert und warten nun auf die Anmeldung von {{Partner}}. Sobald diese eingegangen ist, erhaltet ihr die Best&auml;tigung eurer Paaranmeldung.</p>
        <p>Das Leapin'' Lindy Team</p>
        <hr />
        <p>Du hast folgendes gebucht:<br />{{SeatList}}</p>
        <table style="width: 474px;">
          <tbody>
            <tr>
              <td style="width: 130px;"><strong>Name</strong></td>
              <td style="width: 600px;">{{FirstName}} {{LastName}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Wohnort</strong></td>
              <td style="width: 600px;">{{City}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Telefon</strong></td>
              <td style="width: 600px;">{{Phone}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Helfereinsatz</strong></td>
              <td style="width: 600px;">{{Volunteer}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Erm&auml;ssigung</strong></td>
              <td style="width: 600px;">{{Reduction}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Bemerkung</strong></td>
              <td style="width: 600px;">{{Comments}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>AGBs</strong></td>
              <td style="width: 600px;">{{AcceptTerms}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Preis</strong></td>
              <td style="width: 600px;">{{Price}}</td>
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
        <p>Deine Zahlung muss innerhalb von 5 Arbeitstagen nach Erhalt dieser Email bei uns eintreffen. Wenn wir deine Zahlung erhalten haben, schicken wir dir eine Buchungsbest&auml;tigung. Bezahlst du nicht innerhalb der Frist, wird deine Registrierung m&ouml;glicherweise storniert. Bitte beachte die untenstehende Regelung bez&uuml;glich Absage und R&uuml;ckerstattung.</p>
        <p>Konto: 60-224741-6<br />Swing Machine Bern / Leapin Lindy<br />3000 Bern&nbsp;</p>
        <p>IBAN: CH93 0900 0000 6022 4741 6<br />BLZ: 09000<br />BIC: POFICHBEXXX</p>
        <p><strong>Bankadresse</strong><br />Swiss Post - PostFinance<br />Nordring 8<br />3030 Bern<br />Switzerland</p>
        <p>Bitte &uuml;berweise immer Schweizer Franken. Spesen zu Lasten des Absenders.&nbsp;</p>
        <p><strong>Absagen und R&uuml;ckerstattung:</strong><br />Wenn du deine Anmeldung zur&uuml;cknehmen musst, gelten folgende Regeln f&uuml;r die R&uuml;ckerstattungen:</p>
        <p>Absage bis zum 10. Januar 2018: volle R&uuml;ckerstattung<br />11. Januar - 31. Januar 2018: 50% R&uuml;ckerstattung<br />Februar 2018 und sp&auml;ter: keine R&uuml;ckerstattung&nbsp;</p>
        <p>Wenn du deine Anmeldung annulieren willst, kontaktiere uns in jedem Fall damit wir deinen Platz freigeben k&ouml;nnen.</p>
        <p><strong>WICHTIG</strong>: Krankheit und Verletzung sind in der obigen Regelung eingeschlossen.</p>
        <p>Kannst du kurzfristig nicht teilnehmen, werden wir versuchen, einen Ersatz auf der Warteliste zu finden. Gelingt uns dies, ist eine R&uuml;ckerstattung m&ouml;glich, aber nicht garantiert. Im Grundsatz beh&auml;lt die Regelung G&uuml;ltigkeit.&nbsp;</p>
        <p>In jedem Falle steht es dir frei, selbst einen Ersatz f&uuml;r deinen Workshopplatz oder Partypass zu finden. Bitte kontaktiere uns vor dem Check-In, wenn du deine Anmeldung an eine andere&nbsp; Person &uuml;bertragen hast. Du bleibst aber weiterhin f&uuml;r die Zahlung des Kursgeledes verantwortlich.</p>
        <p><strong>Versicherung<br /></strong>Versicherung ist Sache der Teilnehmenden. Der Veranstalter lehnt jede Haftung ab.</p>
      </td>
    </tr>
  </tbody>
</table>')

INSERT INTO dbo.MailTemplates(Id, EventId, [Type], ContentType, [Subject], SenderMail, SenderName, [Language], Template)
VALUES ('C16F52CC-DA0A-4319-8A31-C26582CA6381', '762A93A4-56E0-402C-B700-1CFB3362B39D', 12 /*DoubleRegistrationMatchedAndAccepted*/, 2, 'Anmeldebestätigung Paaranmeldung', 'noreply@leapinlindy.ch', 'Leapin'' Lindy', 'de', 
'<table cellspacing="0" cellpadding="0" align="center">
  <tbody>
    <tr>
      <td style="text-align: center;"><img src="https://scontent-frx5-1.xx.fbcdn.net/v/t31.0-8/22042178_1480177115410174_419267934679857047_o.jpg?oh=d6833f06299ceb688c53b8439413a696&amp;oe=5A7EF2E1" width="800" height="304" /></td>
    </tr>
    <tr>
      <td><br /><strong>Hallo {{Follower.FirstName}} &amp; {{Leader.FirstName}}</strong><br />
        <p>Herzlichen Dank f&uuml;r eure Paaranmeldung zum Leapin'' Lindy 2018!<br />Wir haben einen Platz f&uuml;r euch reserviert, bitte bezahlt eure Anmeldegeb&uuml;hr&nbsp;innerhalb von 5 Arbeitstagen&nbsp;(Details siehe unten).</p>
        <p>Das Leapin'' Lindy Team</p>
        <hr />
        <p>Ihr habt folgendes gebucht:<br /><br />Leader:<br />{{Leader.SeatList}}</p>
        <table style="width: 474px;">
          <tbody>
            <tr>
              <td style="width: 130px;"><strong>Name</strong></td>
              <td style="width: 600px;">{{Leader.FirstName}} {{Leader.LastName}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Wohnort</strong></td>
              <td style="width: 600px;">{{Leader.City}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Telefon</strong></td>
              <td style="width: 600px;">{{Leader.Phone}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Helfereinsatz</strong></td>
              <td style="width: 600px;">{{Leader.Volunteer}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Erm&auml;ssigung</strong></td>
              <td style="width: 600px;">{{Leader.Reduction}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Bemerkung</strong></td>
              <td style="width: 600px;">{{Leader.Comments}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>AGBs</strong></td>
              <td style="width: 600px;">{{Leader.AcceptTerms}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Preis</strong></td>
              <td style="width: 600px;">{{Leader.Price}}</td>
            </tr>
          </tbody>
        </table>
        <p>&nbsp;Follower:<br />{{Follower.SeatList}}</p>
        <table style="width: 474px;">
          <tbody>
            <tr>
              <td style="width: 130px;"><strong>Name</strong></td>
              <td style="width: 600px;">{{Follower.FirstName}} {{Follower.LastName}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Wohnort</strong></td>
              <td style="width: 600px;">{{Follower.City}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Telefon</strong></td>
              <td style="width: 600px;">{{Follower.Phone}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Helfereinsatz</strong></td>
              <td style="width: 600px;">{{Follower.Volunteer}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Erm&auml;ssigung</strong></td>
              <td style="width: 600px;">{{Follower.Reduction}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Bemerkung</strong></td>
              <td style="width: 600px;">{{Follower.Comments}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>AGBs</strong></td>
              <td style="width: 600px;">{{Follower.AcceptTerms}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Preis</strong></td>
              <td style="width: 600px;">{{Follower.Price}}</td>
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
        <p>Deine Zahlung muss innerhalb von 5 Arbeitstagen nach Erhalt dieser Email bei uns eintreffen. Wenn wir deine Zahlung erhalten haben, schicken wir dir eine Buchungsbest&auml;tigung. Bezahlst du nicht innerhalb der Frist, wird deine Registrierung m&ouml;glicherweise storniert. Bitte beachte die untenstehende Regelung bez&uuml;glich Absage und R&uuml;ckerstattung.</p>
        <p>Konto: 60-224741-6<br />Swing Machine Bern / Leapin Lindy<br />3000 Bern&nbsp;</p>
        <p>IBAN: CH93 0900 0000 6022 4741 6<br />BLZ: 09000<br />BIC: POFICHBEXXX</p>
        <p><strong>Bankadresse</strong><br />Swiss Post - PostFinance<br />Nordring 8<br />3030 Bern<br />Switzerland</p>
        <p>Bitte &uuml;berweise immer Schweizer Franken. Spesen zu Lasten des Absenders.&nbsp;</p>
        <p><strong>Absagen und R&uuml;ckerstattung:</strong><br />Wenn du deine Anmeldung zur&uuml;cknehmen musst, gelten folgende Regeln f&uuml;r die R&uuml;ckerstattungen:</p>
        <p>Absage bis zum 10. Januar 2018: volle R&uuml;ckerstattung<br />11. Januar - 31. Januar 2018: 50% R&uuml;ckerstattung<br />Februar 2018 und sp&auml;ter: keine R&uuml;ckerstattung&nbsp;</p>
        <p>Wenn du deine Anmeldung annulieren willst, kontaktiere uns in jedem Fall damit wir deinen Platz freigeben k&ouml;nnen.</p>
        <p><strong>WICHTIG</strong>: Krankheit und Verletzung sind in der obigen Regelung eingeschlossen.</p>
        <p>Kannst du kurzfristig nicht teilnehmen, werden wir versuchen, einen Ersatz auf der Warteliste zu finden. Gelingt uns dies, ist eine R&uuml;ckerstattung m&ouml;glich, aber nicht garantiert. Im Grundsatz beh&auml;lt die Regelung G&uuml;ltigkeit.&nbsp;</p>
        <p>In jedem Falle steht es dir frei, selbst einen Ersatz f&uuml;r deinen Workshopplatz oder Partypass zu finden. Bitte kontaktiere uns vor dem Check-In, wenn du deine Anmeldung an eine andere&nbsp; Person &uuml;bertragen hast. Du bleibst aber weiterhin f&uuml;r die Zahlung des Kursgeledes verantwortlich.</p>
        <p><strong>Versicherung<br /></strong>Versicherung ist Sache der Teilnehmenden. Der Veranstalter lehnt jede Haftung ab.</p>
      </td>
    </tr>
  </tbody>
</table>')

INSERT INTO dbo.MailTemplates(Id, EventId, [Type], ContentType, [Subject], SenderMail, SenderName, [Language], Template)
VALUES ('B312E47B-0AD3-43D3-AB90-580FE321788F', '762A93A4-56E0-402C-B700-1CFB3362B39D', 13 /*DoubleRegistrationFirstPartnerOnWaitingList*/, 2, 'Anmeldebestätigung Paaranmeldung (Warteliste)', 'noreply@leapinlindy.ch', 'Leapin'' Lindy', 'de', 
'<table cellspacing="0" cellpadding="0" align="center">
  <tbody>
    <tr>
      <td style="text-align: center;"><img src="https://scontent-frx5-1.xx.fbcdn.net/v/t31.0-8/22042178_1480177115410174_419267934679857047_o.jpg?oh=d6833f06299ceb688c53b8439413a696&amp;oe=5A7EF2E1" width="800" height="304" /></td>
    </tr>
    <tr>
      <td><br /><strong>Hallo {{FirstName}}</strong><br />
        <p>Herzlichen Dank f&uuml;r deine Anmeldung zum Leapin'' Lindy 2018!<br /><strong>Deine Paaranmeldung mit {{Partner}} ist auf der Warteliste,&nbsp;bitte bezahle die Anmeldegeb&uuml;hr noch nicht</strong>.</p>
        <p>Das Leapin'' Lindy Team</p>
        <hr />
        <p>Du hast folgendes gebucht:<br />{{SeatList}}</p>
        <table style="width: 816px;">
          <tbody>
            <tr>
              <td style="width: 130px;"><strong>Name</strong></td>
              <td style="width: 700px;">{{FirstName}} {{LastName}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Wohnort</strong></td>
              <td style="width: 700px;">{{City}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Telefon</strong></td>
              <td style="width: 700px;">{{Phone}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Helfereinsatz</strong></td>
              <td style="width: 700px;">{{Volunteer}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Erm&auml;ssigung</strong></td>
              <td style="width: 700px;">{{Reduction}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Bemerkung</strong></td>
              <td style="width: 700px;">{{Comments}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>AGBs</strong></td>
              <td style="width: 700px;">{{AcceptTerms}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Preis</strong></td>
              <td style="width: 700px;">CHF{{Price}}</td>
            </tr>
          </tbody>
        </table>
      </td>
    </tr>
  </tbody>
</table>');


INSERT INTO dbo.MailTemplates(Id, EventId, [Type], ContentType, [Subject], SenderMail, SenderName, [Language], Template)
VALUES ('BB09F868-D619-4392-B25C-08C6B83CD0DF', '762A93A4-56E0-402C-B700-1CFB3362B39D', 14 /*DoubleRegistrationMatchedOnWaitingList*/, 2, 'Anmeldebestätigung Paaranmeldung (Warteliste)', 'noreply@leapinlindy.ch', 'Leapin'' Lindy', 'de', 
'<table cellspacing="0" cellpadding="0" align="center">
  <tbody>
    <tr>
      <td style="text-align: center;"><img src="https://scontent-frx5-1.xx.fbcdn.net/v/t31.0-8/22042178_1480177115410174_419267934679857047_o.jpg?oh=d6833f06299ceb688c53b8439413a696&amp;oe=5A7EF2E1" width="800" height="304" /></td>
    </tr>
    <tr>
      <td><br /><strong>Hallo {{Follower.FirstName}} &amp; {{Leader.FirstName}}</strong><br />
        <p>Herzlichen Dank f&uuml;r eure Paaranmeldung zum Leapin'' Lindy 2018!<br /><strong>Eure Paaranmeldung ist auf der Warteliste, bitte bezahlt die Anmeldegeb&uuml;hr noch nicht.</strong></p>
        <p>Das Leapin'' Lindy Team</p>
        <hr />
        <p>Ihr habt folgendes gebucht:<br /><br />Leader:<br />{{Leader.SeatList}}</p>
        <table style="width: 730px;">
          <tbody>
            <tr>
              <td style="width: 130px;"><strong>Name</strong></td>
              <td style="width: 600px;">{{Leader.FirstName}} {{Leader.LastName}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Wohnort</strong></td>
              <td style="width: 600px;">{{Leader.City}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Telefon</strong></td>
              <td style="width: 600px;">{{Leader.Phone}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Helfereinsatz</strong></td>
              <td style="width: 600px;">{{Leader.Volunteer}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Erm&auml;ssigung</strong></td>
              <td style="width: 600px;">{{Leader.Reduction}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Bemerkung</strong></td>
              <td style="width: 600px;">{{Leader.Comments}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>AGBs</strong></td>
              <td style="width: 600px;">{{Leader.AcceptTerms}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Preis</strong></td>
              <td style="width: 600px;">CHF{{Leader.Price}}</td>
            </tr>
          </tbody>
        </table>
        <p>&nbsp;Follower:<br />{{Follower.SeatList}}</p>
        <table style="width: 730px;">
          <tbody>
            <tr>
              <td style="width: 130px;"><strong>Name</strong></td>
              <td style="width: 600px;">{{Follower.FirstName}} {{Follower.LastName}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Wohnort</strong></td>
              <td style="width: 600px;">{{Follower.City}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Telefon</strong></td>
              <td style="width: 600px;">{{Follower.Phone}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Helfereinsatz</strong></td>
              <td style="width: 600px;">{{Follower.Volunteer}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Erm&auml;ssigung</strong></td>
              <td style="width: 600px;">{{Follower.Reduction}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Bemerkung</strong></td>
              <td style="width: 600px;">{{Follower.Comments}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>AGBs</strong></td>
              <td style="width: 600px;">{{Follower.AcceptTerms}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Preis</strong></td>
              <td style="width: 600px;">CHF{{Follower.Price}}</td>
            </tr>
          </tbody>
        </table>
      </td>
    </tr>
  </tbody>
</table>');


INSERT INTO dbo.MailTemplates(Id, EventId, [Type], ContentType, [Subject], SenderMail, SenderName, [Language], Template)
VALUES ('A4198E42-E965-4259-9154-F673A6F38C4E', '762A93A4-56E0-402C-B700-1CFB3362B39D', 21 /*SoldOut*/, 2, 'Ausverkauft', 'noreply@leapinlindy.ch', 'Leapin'' Lindy', 'de', 
'<table cellspacing="0" cellpadding="0" align="center">
  <tbody>
    <tr>
      <td style="text-align: center;"><img src="https://scontent-frx5-1.xx.fbcdn.net/v/t31.0-8/22042178_1480177115410174_419267934679857047_o.jpg?oh=d6833f06299ceb688c53b8439413a696&amp;oe=5A7EF2E1" width="800" height="304" /></td>
    </tr>
    <tr>
      <td><br /><strong>Hallo {{FirstName}}</strong><br />
        <p>Leider sind die von dir gew&uuml;nschten Optionen ausverkauft.<br />Wir&nbsp;hoffen, dich trotzdem beim n&auml;chsten Mal begr&uuml;ssen zu d&uuml;rfen!</p>
        <p>Das Leapin'' Lindy Team</p>
        <hr />
        <p>{{SeatList}}</p>
        <table style="width: 826px;">
          <tbody>
            <tr>
              <td style="width: 130px;"><strong>Name</strong></td>
              <td style="width: 700px;">{{FirstName}} {{LastName}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Wohnort</strong></td>
              <td style="width: 700px;">{{City}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Telefon</strong></td>
              <td style="width: 700px;">{{Phone}}</td>
            </tr>
            <tr>
              <td style="width: 130px;"><strong>Bemerkung</strong></td>
              <td style="width: 700px;">{{Comments}}</td>
            </tr>
          </tbody>
        </table>
      </td>
    </tr>
  </tbody>
</table>');


INSERT INTO dbo.MailTemplates(Id, EventId, [Type], ContentType, [Subject], SenderMail, SenderName, [Language], Template)
VALUES ('186F48DC-9927-464E-B4B7-55262DCB941C', '762A93A4-56E0-402C-B700-1CFB3362B39D', 31 /*RegistrationCancelled*/, 2, 'Booking cancelled', 'noreply@leapinlindy.ch', 'Leapin'' Lindy', 'de', 
'<table cellspacing="0" cellpadding="0" align="center">
  <tbody>
    <tr>
      <td style="text-align: center;"><img src="https://scontent-frx5-1.xx.fbcdn.net/v/t31.0-8/22042178_1480177115410174_419267934679857047_o.jpg?oh=d6833f06299ceb688c53b8439413a696&amp;oe=5A7EF2E1" width="800" height="304" /></td>
    </tr>
    <tr>
      <td><br /><strong>Hallo {{FirstName}}</strong><br />
        <p>Wir haben keine Zahlung von dir erhalten und deshalb deine Buchung storniert, um den Platz f&uuml;r jemand anderen freizugeben.</p>
        <p>Das Leapin'' Lindy Team</p>
      </td>
    </tr>
  </tbody>
</table>');


INSERT INTO dbo.MailTemplates(Id, EventId, [Type], ContentType, [Subject], SenderMail, SenderName, [Language], Template)
VALUES ('22844150-3210-4854-ADCF-5DCFD6A79757', '762A93A4-56E0-402C-B700-1CFB3362B39D', 41 /*FullyPaid*/, 2, 'Buchungsbestätigung', 'noreply@leapinlindy.ch', 'Leapin'' Lindy', 'de', 
'<table cellspacing="0" cellpadding="0" align="center">
  <tbody>
    <tr>
      <td style="text-align: center;"><img src="https://scontent-frx5-1.xx.fbcdn.net/v/t31.0-8/22042178_1480177115410174_419267934679857047_o.jpg?oh=d6833f06299ceb688c53b8439413a696&amp;oe=5A7EF2E1" width="800" height="304" /></td>
    </tr>
    <tr>
      <td><br /><strong>Hallo {{FirstName}}</strong><br />
        <p>Wir haben deine Zahlung erhalten und best&auml;tigen deine Buchung f&uuml;r Leapin'' Lindy 2018.<br />Wir freuen uns darauf, dich im Februar begr&uuml;ssen zu d&uuml;rfen!</p>
        <p>Schau dir doch die Leapin'' Lindy-App an:&nbsp;<a href="https://lineupr.com/ll18/leapin-lindy-2018">https://lineupr.com/ll18/leapin-lindy-2018</a></p>
        <p>Das Leapin'' Lindy Team</p>
      </td>
    </tr>
  </tbody>
</table>');


INSERT INTO dbo.MailTemplates(Id, EventId, [Type], ContentType, [Subject], SenderMail, SenderName, [Language], Template)
VALUES ('2631FC6B-B673-404E-BC25-B5099FBAB003', '762A93A4-56E0-402C-B700-1CFB3362B39D', 42 /*PartnerRegistrationFirstPaid*/, 2, 'Erste Zahlung erhalten', 'noreply@leapinlindy.ch', 'Leapin'' Lindy', 'de', 
'<table cellspacing="0" cellpadding="0" align="center">
  <tbody>
    <tr>
      <td style="text-align: center;"><img src="https://scontent-frx5-1.xx.fbcdn.net/v/t31.0-8/22042178_1480177115410174_419267934679857047_o.jpg?oh=d6833f06299ceb688c53b8439413a696&amp;oe=5A7EF2E1" width="800" height="304" /></td>
    </tr>
    <tr>
      <td><br /><strong>Hallo {{Follower.FirstName}} &amp; {{Leader.FirstName}}</strong><br />
        <p>Wir haben die erste Zahlung erhalten; sobald die zweite eintrifft werden wir eure Anmeldung best&auml;tigen.</p>
        <p>Das Leapin'' Lindy Team</p>
      </td>
    </tr>
  </tbody>
</table>');


INSERT INTO dbo.MailTemplates(Id, EventId, [Type], ContentType, [Subject], SenderMail, SenderName, [Language], Template)
VALUES ('E2EE1519-EA1F-488A-80B4-F0F2ECB453C1', '762A93A4-56E0-402C-B700-1CFB3362B39D', 43 /*PartnerRegistrationFullyPaid*/, 2, 'Buchungsbestätigung', 'noreply@leapinlindy.ch', 'Leapin'' Lindy', 'de', 
'<table cellspacing="0" cellpadding="0" align="center">
  <tbody>
    <tr>
      <td style="text-align: center;"><img src="https://scontent-frx5-1.xx.fbcdn.net/v/t31.0-8/22042178_1480177115410174_419267934679857047_o.jpg?oh=d6833f06299ceb688c53b8439413a696&amp;oe=5A7EF2E1" width="800" height="304" /></td>
    </tr>
    <tr>
      <td><br /><strong>Hallo {{FirstName}}</strong><br />
        <p>Wir haben eure Zahlungen erhalten und best&auml;tigen eure Buchung f&uuml;r Leapin'' Lindy 2018.<br />Wir freuen uns darauf, euch im Februar begr&uuml;ssen zu d&uuml;rfen!</p>
        <p>Schaut euch doch die Leapin'' Lindy-App an:&nbsp;<a href="https://lineupr.com/ll18/leapin-lindy-2018">https://lineupr.com/ll18/leapin-lindy-2018</a></p>
        <p>Das Leapin'' Lindy Team</p>
      </td>
    </tr>
  </tbody>
</table>');


INSERT INTO dbo.MailTemplates(Id, EventId, [Type], ContentType, [Subject], SenderMail, SenderName, [Language], Template)
VALUES ('05A0F1D1-7F43-4F30-A29A-9D5E2E38FA66', '762A93A4-56E0-402C-B700-1CFB3362B39D', 1 /*SingleRegistrationAccepted*/, 2, 'Registration received', 'noreply@leapinlindy.ch', 'Leapin'' Lindy', 'en', 
'<table cellspacing="0" cellpadding="0" align="center">
  <tbody>
    <tr>
      <td style="text-align: center;"><img src="https://scontent-frx5-1.xx.fbcdn.net/v/t31.0-8/22042178_1480177115410174_419267934679857047_o.jpg?oh=d6833f06299ceb688c53b8439413a696&amp;oe=5A7EF2E1" width="800" height="304" /></td>
    </tr>
    <tr>
      <td><br /><strong>Hello {{FirstName}}</strong><br />
        <p>Thank you for registering for Leapin'' Lindy 2018!<br />We have reserved a place for you, please pay the fee of CHF{{Price}} within the next 5 work days (for payment details, see below).</p>
        <p>We are happy that you are taking part!</p>
        <p>The Leapin'' Lindy Team</p>
        <hr />
        <p>You booked the following:<br />{{SeatList}}</p>
        <table style="width: 800px;">
          <tbody>
            <tr>
              <td style="width: 160px;"><strong>Name</strong></td>
              <td style="width: 640px;">{{FirstName}} {{LastName}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Place of Residence</strong></td>
              <td style="width: 640px;">{{City}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Phone</strong></td>
              <td style="width: 640px;">{{Phone}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Volunteer</strong></td>
              <td style="width: 640px;">{{Volunteer}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Reduction</strong></td>
              <td style="width: 640px;">{{Reduction}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Comments</strong></td>
              <td style="width: 640px;">{{Comments}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Terms</strong></td>
              <td style="width: 640px;">{{AcceptTerms}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Price</strong></td>
              <td style="width: 640px;">CHF{{Price}}</td>
            </tr>
          </tbody>
        </table>
      </td>
    </tr>
    <tr>
      <td><hr />
        <p>Payment information:</p>
        <p><strong>Please make one payment per person and quote your email address&nbsp;</strong></p>
        <p>Your payment has to reach us within 5 working days upon the receipt of this email. As soon as we have received your payment, we will send you a booking confirmation. Should we not receive your payment in time, your registration may be cancelled. Please note the regulations concerning cancellations as mentioned below.</p>
        <p>IBAN: CH93 0900 0000 6022 4741 6<br />BIC: POFICHBEXXX<br />BLZ: 09000<br />Account: 60-224741-6<br />Swing Machine Bern / Leapin Lindy<br />3000 Bern&nbsp;</p>
        <p><strong>Address of the Bank</strong><br />Swiss Post - PostFinance<br />Nordring 8<br />3030 Bern<br />Switzerland</p>
        <p>Please pay in Swiss Francs. Additional expenses on sender&rsquo;s account.</p>
        <p><strong>Cancellations and Refunds:</strong><br />If you have to withdraw your booking, the following regulations or refunding apply:</p>
        <p>Cancellations until the 10th January 2018:&nbsp;full refund<br />11th January 2018 - 31th Januar 2018: 50%&nbsp;refund<br />February 2018 and later:&nbsp;no refund</p>
        <p>Should you wish to cancel your registration, contact us in any case so that we can give your place to someone else.</p>
        <p><strong>Important</strong>: Illness and injury are included in the above-mentioned regulations.</p>
        <p>Should you be unable to&nbsp;participate at short notice, we will do our best to find a replacement from the waiting list. If we succeed, a refund might be possible. However, we cannot guarantee it. As a principle, the regulations apply.</p>
        <p>However, you are free to find your own replacement for workshop or party. Please let us know before check in, if you have passed your booking on to someone else. Nonetheless, you remain responsible for the payment of the booking fee.&nbsp;</p>
        <p><strong>Insurance: </strong>To be properly insured is the responsibility of the participant. We herewith decline any liability.</p>
      </td>
    </tr>
  </tbody>
</table>')


INSERT INTO dbo.MailTemplates(Id, EventId, [Type], ContentType, [Subject], SenderMail, SenderName, [Language], Template)
VALUES ('D06E51C5-F329-4317-B5FB-2BCEB72D050F', '762A93A4-56E0-402C-B700-1CFB3362B39D', 2 /*SingleRegistrationOnWaitingList*/, 2, 'Registration received (waiting list)', 'noreply@leapinlindy.ch', 'Leapin'' Lindy', 'en', 
'<table cellspacing="0" cellpadding="0" align="center">
  <tbody>
    <tr>
      <td style="text-align: center;"><img src="https://scontent-frx5-1.xx.fbcdn.net/v/t31.0-8/22042178_1480177115410174_419267934679857047_o.jpg?oh=d6833f06299ceb688c53b8439413a696&amp;oe=5A7EF2E1" width="800" height="304" /></td>
    </tr>
    <tr>
      <td><br /><strong>Hello {{FirstName}}</strong><br />
        <p>Thank you for registering for Leapin'' Lindy 2018!<br /><strong>You are on the waiting list, please don''t pay the fee yet.</strong></p>
        <p>The Leapin'' Lindy Team</p>
        <hr />
        <p>Du hast folgendes gebucht:<br />{{SeatList}}</p>
        <table style="width: 800px;">
          <tbody>
            <tr>
              <td style="width: 160px;"><strong>Name</strong></td>
              <td style="width: 640px;">{{FirstName}} {{LastName}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Place of Residence</strong></td>
              <td style="width: 640px;">{{City}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Phone</strong></td>
              <td style="width: 640px;">{{Phone}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Volunteer</strong></td>
              <td style="width: 640px;">{{Volunteer}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Reduction</strong></td>
              <td style="width: 640px;">{{Reduction}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Comments</strong></td>
              <td style="width: 640px;">{{Comments}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Terms</strong></td>
              <td style="width: 640px;">{{AcceptTerms}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Price</strong></td>
              <td style="width: 640px;">CHF{{Price}}</td>
            </tr>
          </tbody>
        </table>
      </td>
    </tr>
  </tbody>
</table>');



INSERT INTO dbo.MailTemplates(Id, EventId, [Type], ContentType, [Subject], SenderMail, SenderName, [Language], Template)
VALUES ('569FA80F-1BCA-40CC-AA1B-9A68C49FA237', '762A93A4-56E0-402C-B700-1CFB3362B39D', 11 /*DoubleRegistrationFirstPartnerAccepted*/, 2, 'Registration received', 'noreply@leapinlindy.ch', 'Leapin'' Lindy', 'en', 
'<table cellspacing="0" cellpadding="0" align="center">
  <tbody>
    <tr>
      <td style="text-align: center;"><img src="https://scontent-frx5-1.xx.fbcdn.net/v/t31.0-8/22042178_1480177115410174_419267934679857047_o.jpg?oh=d6833f06299ceb688c53b8439413a696&amp;oe=5A7EF2E1" width="800" height="304" /></td>
    </tr>
    <tr>
      <td><br /><strong>Hello {{FirstName}}</strong><br />
        <p>Thank you for registering for Leapin'' Lindy 2018!<br />We have reserved a place for you, now we''re waiting for the registration from {{Partner}}. As soon as this is received you''ll get a confirmation of your partner registration.</p>
        <p>The Leapin'' Lindy Team</p>
        <hr />
        <p>You booked the following:<br />{{SeatList}}</p>
        <table style="width: 800px;">
          <tbody>
            <tr>
              <td style="width: 160px;"><strong>Name</strong></td>
              <td style="width: 640px;">{{FirstName}} {{LastName}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Place of Residence</strong></td>
              <td style="width: 640px;">{{City}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Phone</strong></td>
              <td style="width: 640px;">{{Phone}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Volunteer</strong></td>
              <td style="width: 640px;">{{Volunteer}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Reduction</strong></td>
              <td style="width: 640px;">{{Reduction}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Comments</strong></td>
              <td style="width: 640px;">{{Comments}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Terms</strong></td>
              <td style="width: 640px;">{{AcceptTerms}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Price</strong></td>
              <td style="width: 640px;">CHF{{Price}}</td>
            </tr>
          </tbody>
        </table>
      </td>
    </tr>
    <tr>
      <td><hr />
        <p>Payment information:</p>
        <p><strong>Please make one payment per person and quote your email address&nbsp;</strong></p>
        <p>Your payment has to reach us within 5 working days upon the receipt of this email. As soon as we have received your payment, we will send you a booking confirmation. Should we not receive your payment in time, your registration may be cancelled. Please note the regulations concerning cancellations as mentioned below.</p>
        <p>IBAN: CH93 0900 0000 6022 4741 6<br />BIC: POFICHBEXXX<br />BLZ: 09000<br />Account: 60-224741-6<br />Swing Machine Bern / Leapin Lindy<br />3000 Bern&nbsp;</p>
        <p><strong>Address of the Bank</strong><br />Swiss Post - PostFinance<br />Nordring 8<br />3030 Bern<br />Switzerland</p>
        <p>Please pay in Swiss Francs. Additional expenses on sender&rsquo;s account.</p>
        <p><strong>Cancellations and Refunds:</strong><br />If you have to withdraw your booking, the following regulations or refunding apply:</p>
        <p>Cancellations until the 10th January 2018:&nbsp;full refund<br />11th January 2018 - 31th Januar 2018: 50%&nbsp;refund<br />February 2018 and later:&nbsp;no refund</p>
        <p>Should you wish to cancel your registration, contact us in any case so that we can give your place to someone else.</p>
        <p><strong>Important</strong>: Illness and injury are included in the above-mentioned regulations.</p>
        <p>Should you be unable to&nbsp;participate at short notice, we will do our best to find a replacement from the waiting list. If we succeed, a refund might be possible. However, we cannot guarantee it. As a principle, the regulations apply.</p>
        <p>However, you are free to find your own replacement for workshop or party. Please let us know before check in, if you have passed your booking on to someone else. Nonetheless, you remain responsible for the payment of the booking fee.&nbsp;</p>
        <p><strong>Insurance: </strong>To be properly insured is the responsibility of the participant. We herewith decline any liability.</p>
      </td>
    </tr>
  </tbody>
</table>')

INSERT INTO dbo.MailTemplates(Id, EventId, [Type], ContentType, [Subject], SenderMail, SenderName, [Language], Template)
VALUES ('0A07D8AE-7CD1-4939-A395-14167BD4FAE5', '762A93A4-56E0-402C-B700-1CFB3362B39D', 12 /*DoubleRegistrationMatchedAndAccepted*/, 2, 'Partner registration received', 'noreply@leapinlindy.ch', 'Leapin'' Lindy', 'en', 
'<table cellspacing="0" cellpadding="0" align="center">
  <tbody>
    <tr>
      <td style="text-align: center;"><img src="https://scontent-frx5-1.xx.fbcdn.net/v/t31.0-8/22042178_1480177115410174_419267934679857047_o.jpg?oh=d6833f06299ceb688c53b8439413a696&amp;oe=5A7EF2E1" width="800" height="304" /></td>
    </tr>
    <tr>
      <td><br /><strong>Hello {{Follower.FirstName}} &amp; {{Leader.FirstName}}</strong><br />
        <p>Thank you for your partner registration for Leapin'' Lindy 2018!<br />We have reserved a place for you,&nbsp;please pay the fee&nbsp;within the next 5 work days (for payment details, see below).</p>
        <p>The Leapin'' Lindy Team</p>
        <hr />
        <p>You booked the following:<br /><br />Leader:<br />{{Leader.SeatList}}</p>
        <table style="width: 800px;">
          <tbody>
            <tr>
              <td style="width: 160px;"><strong>Name</strong></td>
              <td style="width: 640px;">{{Leader.FirstName}} {{Leader.LastName}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Place of Residence</strong></td>
              <td style="width: 640px;">{{Leader.City}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Phone</strong></td>
              <td style="width: 640px;">{{Leader.Phone}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Volunteer</strong></td>
              <td style="width: 640px;">{{Leader.Volunteer}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Reduction</strong></td>
              <td style="width: 640px;">{{Leader.Reduction}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Comments</strong></td>
              <td style="width: 640px;">{{Leader.Comments}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Terms</strong></td>
              <td style="width: 640px;">{{Leader.AcceptTerms}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Price</strong></td>
              <td style="width: 640px;">CHF{{Leader.Price}}</td>
            </tr>
          </tbody>
        </table>
        <p>&nbsp;Follower:<br />{{Follower.SeatList}}</p>
        <table style="width: 800px;">
          <tbody>
            <tr>
              <td style="width: 160px;"><strong>Name</strong></td>
              <td style="width: 640px;">{{Follower.FirstName}} {{Follower.LastName}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Place of Residence</strong></td>
              <td style="width: 640px;">{{Follower.City}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Phone</strong></td>
              <td style="width: 640px;">{{Follower.Phone}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Volunteer</strong></td>
              <td style="width: 640px;">{{Follower.Volunteer}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Reduction</strong></td>
              <td style="width: 640px;">{{Follower.Reduction}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Comments</strong></td>
              <td style="width: 640px;">{{Follower.Comments}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Terms</strong></td>
              <td style="width: 640px;">{{Follower.AcceptTerms}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Price</strong></td>
              <td style="width: 640px;">CHF{{Follower.Price}}</td>
            </tr>
          </tbody>
        </table>
      </td>
    </tr>
    <tr>
      <td><hr />
        <p>Payment information:</p>
        <p><strong>Please make one payment per person and quote your email address&nbsp;</strong></p>
        <p>Your payment has to reach us within 5 working days upon the receipt of this email. As soon as we have received your payment, we will send you a booking confirmation. Should we not receive your payment in time, your registration may be cancelled. Please note the regulations concerning cancellations as mentioned below.</p>
        <p>IBAN: CH93 0900 0000 6022 4741 6<br />BIC: POFICHBEXXX<br />BLZ: 09000<br />Account: 60-224741-6<br />Swing Machine Bern / Leapin Lindy<br />3000 Bern&nbsp;</p>
        <p><strong>Address of the Bank</strong><br />Swiss Post - PostFinance<br />Nordring 8<br />3030 Bern<br />Switzerland</p>
        <p>Please pay in Swiss Francs. Additional expenses on sender&rsquo;s account.</p>
        <p><strong>Cancellations and Refunds:</strong><br />If you have to withdraw your booking, the following regulations or refunding apply:</p>
        <p>Cancellations until the 10th January 2018:&nbsp;full refund<br />11th January 2018 - 31th Januar 2018: 50%&nbsp;refund<br />February 2018 and later:&nbsp;no refund</p>
        <p>Should you wish to cancel your registration, contact us in any case so that we can give your place to someone else.</p>
        <p><strong>Important</strong>: Illness and injury are included in the above-mentioned regulations.</p>
        <p>Should you be unable to&nbsp;participate at short notice, we will do our best to find a replacement from the waiting list. If we succeed, a refund might be possible. However, we cannot guarantee it. As a principle, the regulations apply.</p>
        <p>However, you are free to find your own replacement for workshop or party. Please let us know before check in, if you have passed your booking on to someone else. Nonetheless, you remain responsible for the payment of the booking fee.&nbsp;</p>
        <p><strong>Insurance: </strong>To be properly insured is the responsibility of the participant. We herewith decline any liability.</p>
      </td>
    </tr>
  </tbody>
</table>')

INSERT INTO dbo.MailTemplates(Id, EventId, [Type], ContentType, [Subject], SenderMail, SenderName, [Language], Template)
VALUES ('A07609C7-0715-4892-B0D0-E25F3460DDEC', '762A93A4-56E0-402C-B700-1CFB3362B39D', 13 /*DoubleRegistrationFirstPartnerOnWaitingList*/, 2, 'Partner registration received (waiting list)', 'noreply@leapinlindy.ch', 'Leapin'' Lindy', 'en', 
'<table cellspacing="0" cellpadding="0" align="center">
  <tbody>
    <tr>
      <td style="text-align: center;"><img src="https://scontent-frx5-1.xx.fbcdn.net/v/t31.0-8/22042178_1480177115410174_419267934679857047_o.jpg?oh=d6833f06299ceb688c53b8439413a696&amp;oe=5A7EF2E1" width="800" height="304" /></td>
    </tr>
    <tr>
      <td><br /><strong>Hello {{FirstName}}</strong><br />
        <p>Thank you for registering for Leapin'' Lindy 2018!<br /><strong>Your partner registration with {{Partner}} is on the waiting list, please don''t pay the fee yet.</strong></p>
        <p>The Leapin'' Lindy Team</p>
        <hr />
        <p>You booked the following:<br />{{SeatList}}</p>
        <table style="width: 800px;">
          <tbody>
            <tr>
              <td style="width: 160px;"><strong>Name</strong></td>
              <td style="width: 640px;">{{FirstName}} {{LastName}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Place of Residence</strong></td>
              <td style="width: 640px;">{{City}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Phone</strong></td>
              <td style="width: 640px;">{{Phone}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Volunteer</strong></td>
              <td style="width: 640px;">{{Volunteer}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Reduction</strong></td>
              <td style="width: 640px;">{{Reduction}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Comments</strong></td>
              <td style="width: 640px;">{{Comments}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Terms</strong></td>
              <td style="width: 640px;">{{AcceptTerms}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Price</strong></td>
              <td style="width: 640px;">CHF{{Price}}</td>
            </tr>
          </tbody>
        </table>
      </td>
    </tr>
  </tbody>
</table>');


INSERT INTO dbo.MailTemplates(Id, EventId, [Type], ContentType, [Subject], SenderMail, SenderName, [Language], Template)
VALUES ('698ECAC6-8E95-496C-8531-3BD096B1BFE9', '762A93A4-56E0-402C-B700-1CFB3362B39D', 14 /*DoubleRegistrationMatchedOnWaitingList*/, 2, 'Partner registration received (waiting list)', 'noreply@leapinlindy.ch', 'Leapin'' Lindy', 'en', 
'<table cellspacing="0" cellpadding="0" align="center">
  <tbody>
    <tr>
      <td style="text-align: center;"><img src="https://scontent-frx5-1.xx.fbcdn.net/v/t31.0-8/22042178_1480177115410174_419267934679857047_o.jpg?oh=d6833f06299ceb688c53b8439413a696&amp;oe=5A7EF2E1" width="800" height="304" /></td>
    </tr>
    <tr>
      <td><br /><strong>Hello {{Follower.FirstName}} &amp; {{Leader.FirstName}}</strong><br />
        <p>Thank you for your partner registration for Leapin'' Lindy 2018!<br /><strong>Your partner registration is on the waiting list, please don''t pay the fee yet.</strong></p>
        <p>The Leapin'' Lindy Team</p>
        <hr />
        <p>You booked the following:<br /><br />Leader:<br />{{Leader.SeatList}}</p>
        <table style="width: 800px;">
          <tbody>
            <tr>
              <td style="width: 160px;"><strong>Name</strong></td>
              <td style="width: 640px;">{{Leader.FirstName}} {{Leader.LastName}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Place of Residence</strong></td>
              <td style="width: 640px;">{{Leader.City}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Phone</strong></td>
              <td style="width: 640px;">{{Leader.Phone}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Volunteer</strong></td>
              <td style="width: 640px;">{{Leader.Volunteer}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Reduction</strong></td>
              <td style="width: 640px;">{{Leader.Reduction}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Comments</strong></td>
              <td style="width: 640px;">{{Leader.Comments}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Terms</strong></td>
              <td style="width: 640px;">{{Leader.AcceptTerms}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Price</strong></td>
              <td style="width: 640px;">CHF{{Leader.Price}}</td>
            </tr>
          </tbody>
        </table>
        <p>&nbsp;Follower:<br />{{Follower.SeatList}}</p>
        <table style="width: 800px;">
          <tbody>
            <tr>
              <td style="width: 160px;"><strong>Name</strong></td>
              <td style="width: 640px;">{{Follower.FirstName}} {{Follower.LastName}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Place of Residence</strong></td>
              <td style="width: 640px;">{{Follower.City}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Phone</strong></td>
              <td style="width: 640px;">{{Follower.Phone}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Volunteer</strong></td>
              <td style="width: 640px;">{{Follower.Volunteer}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Reduction</strong></td>
              <td style="width: 640px;">{{Follower.Reduction}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Comments</strong></td>
              <td style="width: 640px;">{{Follower.Comments}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Terms</strong></td>
              <td style="width: 640px;">{{Follower.AcceptTerms}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Price</strong></td>
              <td style="width: 640px;">CHF{{Follower.Price}}</td>
            </tr>
          </tbody>
        </table>
      </td>
    </tr>
  </tbody>
</table>');


INSERT INTO dbo.MailTemplates(Id, EventId, [Type], ContentType, [Subject], SenderMail, SenderName, [Language], Template)
VALUES ('C41D025A-84A2-4590-8974-867D8027B490', '762A93A4-56E0-402C-B700-1CFB3362B39D', 21 /*SoldOut*/, 2, 'Sold out', 'noreply@leapinlindy.ch', 'Leapin'' Lindy', 'en', 
'<table cellspacing="0" cellpadding="0" align="center">
  <tbody>
    <tr>
      <td style="text-align: center;"><img src="https://scontent-frx5-1.xx.fbcdn.net/v/t31.0-8/22042178_1480177115410174_419267934679857047_o.jpg?oh=d6833f06299ceb688c53b8439413a696&amp;oe=5A7EF2E1" width="800" height="304" /></td>
    </tr>
    <tr>
      <td><br /><strong>Hello {{FirstName}}</strong><br />
        <p>Unfortunately the options you asked for are sold out.<br />We hope to to see you next time!</p>
        <p>The Leapin'' Lindy Team</p>
        <hr />
        <p>{{SeatList}}</p>
        <table style="width: 800px;">
          <tbody>
            <tr>
              <td style="width: 160px;"><strong>Name</strong></td>
              <td style="width: 640px;">{{FirstName}} {{LastName}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Place of Residence</strong></td>
              <td style="width: 640px;">{{City}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Phone</strong></td>
              <td style="width: 640px;">{{Phone}}</td>
            </tr>
            <tr>
              <td style="width: 160px;"><strong>Comments</strong></td>
              <td style="width: 640px;">{{Comments}}</td>
            </tr>
          </tbody>
        </table>
      </td>
    </tr>
  </tbody>
</table>');


INSERT INTO dbo.MailTemplates(Id, EventId, [Type], ContentType, [Subject], SenderMail, SenderName, [Language], Template)
VALUES ('2ACB5061-6936-4BD9-8AC6-1905A4E86EE9', '762A93A4-56E0-402C-B700-1CFB3362B39D', 31 /*RegistrationCancelled*/, 2, 'Booking cancelled', 'noreply@leapinlindy.ch', 'Leapin'' Lindy', 'en', 
'<table cellspacing="0" cellpadding="0" align="center">
  <tbody>
    <tr>
      <td style="text-align: center;"><img src="https://scontent-frx5-1.xx.fbcdn.net/v/t31.0-8/22042178_1480177115410174_419267934679857047_o.jpg?oh=d6833f06299ceb688c53b8439413a696&amp;oe=5A7EF2E1" width="800" height="304" /></td>
    </tr>
    <tr>
      <td><br /><strong>Hello {{FirstName}}</strong><br />
        <p>We have not received your payment in time and have cancelled your registration for Leapin'' Lindy 2018 to give the place to somebody else.</p>
        <p>The Leapin'' Lindy Team</p>
      </td>
    </tr>
  </tbody>
</table>');


INSERT INTO dbo.MailTemplates(Id, EventId, [Type], ContentType, [Subject], SenderMail, SenderName, [Language], Template)
VALUES ('CDCBA09B-84D2-4B58-A54C-ECF9F7497667', '762A93A4-56E0-402C-B700-1CFB3362B39D', 41 /*FullyPaid*/, 2, 'Booking confirmation', 'noreply@leapinlindy.ch', 'Leapin'' Lindy', 'en', 
'<table cellspacing="0" cellpadding="0" align="center">
  <tbody>
    <tr>
      <td style="text-align: center;"><img src="https://scontent-frx5-1.xx.fbcdn.net/v/t31.0-8/22042178_1480177115410174_419267934679857047_o.jpg?oh=d6833f06299ceb688c53b8439413a696&amp;oe=5A7EF2E1" width="800" height="304" /></td>
    </tr>
    <tr>
      <td><br /><strong>Hello {{FirstName}}</strong><br />
        <p>We have received your payment and confirm your booking for Leapin'' Lindy 2018.<br />We are looking forward to welcoming you in February!</p>
        <p>Have a look at the Leapin'' Lindy-App&nbsp;<a href="https://lineupr.com/ll18/leapin-lindy-2018">https://lineupr.com/ll18/leapin-lindy-2018</a></p>
        <p>The Leapin'' Lindy Team</p>
      </td>
    </tr>
  </tbody>
</table>');


INSERT INTO dbo.MailTemplates(Id, EventId, [Type], ContentType, [Subject], SenderMail, SenderName, [Language], Template)
VALUES ('BA2076AB-39AA-4687-AEFF-E55846213B82', '762A93A4-56E0-402C-B700-1CFB3362B39D', 42 /*PartnerRegistrationFirstPaid*/, 2, 'First payment received', 'noreply@leapinlindy.ch', 'Leapin'' Lindy', 'en', 
'<table cellspacing="0" cellpadding="0" align="center">
  <tbody>
    <tr>
      <td style="text-align: center;"><img src="https://scontent-frx5-1.xx.fbcdn.net/v/t31.0-8/22042178_1480177115410174_419267934679857047_o.jpg?oh=d6833f06299ceb688c53b8439413a696&amp;oe=5A7EF2E1" width="800" height="304" /></td>
    </tr>
    <tr>
      <td><br /><strong>Hello {{Follower.FirstName}} &amp; {{Leader.FirstName}}</strong><br />
        <p>We have received the first payment; as soon as we get the second one we''ll confirm your booking.</p>
        <p>The Leapin'' Lindy Team</p>
      </td>
    </tr>
  </tbody>
</table>');


INSERT INTO dbo.MailTemplates(Id, EventId, [Type], ContentType, [Subject], SenderMail, SenderName, [Language], Template)
VALUES ('3BFC9A79-3FAF-4C41-BDE4-CF22CE97D4AC', '762A93A4-56E0-402C-B700-1CFB3362B39D', 43 /*PartnerRegistrationFullyPaid*/, 2, 'Booking confirmation', 'noreply@leapinlindy.ch', 'Leapin'' Lindy', 'en', 
'<table cellspacing="0" cellpadding="0" align="center">
  <tbody>
    <tr>
      <td style="text-align: center;"><img src="https://scontent-frx5-1.xx.fbcdn.net/v/t31.0-8/22042178_1480177115410174_419267934679857047_o.jpg?oh=d6833f06299ceb688c53b8439413a696&amp;oe=5A7EF2E1" width="800" height="304" /></td>
    </tr>
    <tr>
      <td><br /><strong>Hello {{Follower.FirstName}} &amp; {{Leader.FirstName}}</strong><br />
        <p>We have received your payments and confirm your booking for Leapin'' Lindy 2018.<br />We are looking forward to welcoming you in February!</p>
        <p>Have a look at the Leapin'' Lindy-App:&nbsp;<a href="https://lineupr.com/ll18/leapin-lindy-2018">https://lineupr.com/ll18/leapin-lindy-2018</a></p>
        <p>The Leapin'' Lindy Team</p>
      </td>
    </tr>
  </tbody>
</table>');

--commit
--rollback
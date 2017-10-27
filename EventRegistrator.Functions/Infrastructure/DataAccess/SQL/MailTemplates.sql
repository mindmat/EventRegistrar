DELETE FROM dbo.MailTemplates WHERE Id IN ('A3A7144D-F51F-4B51-ACDD-9FE96F197506', 
                                           '77204060-8E30-4055-B797-92A324A00214',
                                           'E5AB6F99-8A43-4D99-9CCD-B10B4F711B5C',
                                           'C16F52CC-DA0A-4319-8A31-C26582CA6381')

INSERT INTO dbo.MailTemplates(Id, EventId, [Type], ContentType, [Subject], SenderMail, SenderName, [Language], Template)
VALUES ('A3A7144D-F51F-4B51-ACDD-9FE96F197506', '762A93A4-56E0-402C-B700-1CFB3362B39D', 1, 2, 'Anmeldebestätigung', 'noreply@leapinlindy.ch', 'Leapin'' Lindy (noreply)', 'de', 
'<table cellspacing="0" cellpadding="0" align="center">
  <tbody>
    <tr>
      <td style="text-align: center;"><img src="https://scontent-frx5-1.xx.fbcdn.net/v/t31.0-8/22042178_1480177115410174_419267934679857047_o.jpg?oh=d6833f06299ceb688c53b8439413a696&amp;oe=5A7EF2E1" width="800" height="304" /></td>
    </tr>
    <tr>
      <td><br /><strong>Hallo {{FirstName}}!</strong><br />
        <p>Herzlichen Dank f&uuml;r deine Anmeldung zum Leapin'' Lindy 2018!<br />Wir haben einen Platz f&uuml;r dich reserviert, bitte bezahle die Anmeldegeb&uuml;hr in den n&auml;chsten 14 Tagen (Details siehe unten).<br />Beiliegend findest du deine Anmeldedetails.</p>
        <p>Wir freuen uns, dass du dabei bist!</p>
        <p>Das Leapin'' Lindy Team</p>
        <hr />
        <p>Du hast folgendes gebucht:<br />{{SeatList}}</p>
        <table style="width: 474px;">
          <tbody>
            <tr>
              <td style="width: 129px;"><strong>Name</strong></td>
              <td style="width: 335px;">{{FirstName}} {{LastName}}</td>
            </tr>
            <tr>
              <td style="width: 129px;"><strong>Wohnort</strong></td>
              <td style="width: 335px;">{{City}}</td>
            </tr>
            <tr>
              <td style="width: 129px;"><strong>Telefon</strong></td>
              <td style="width: 335px;">{{Phone}}</td>
            </tr>
            <tr>
              <td style="width: 129px;"><strong>Helfereinsatz</strong></td>
              <td style="width: 335px;">{{Volunteer}}</td>
            </tr>
            <tr>
              <td style="width: 129px;"><strong>Erm&auml;ssigung</strong></td>
              <td style="width: 335px;">{{Reduction}}</td>
            </tr>
            <tr>
              <td style="width: 129px;"><strong>Bemerkung</strong></td>
              <td style="width: 335px;">{{Comments}}</td>
            </tr>
            <tr>
              <td style="width: 129px;"><strong>AGBs</strong></td>
              <td style="width: 335px;">{{AcceptTerms}}</td>
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
DELETE FROM dbo.MailTemplates WHERE Id IN ('A3A7144D-F51F-4B51-ACDD-9FE96F197506')

INSERT INTO dbo.MailTemplates(Id, EventId, [Type], ContentType, [Subject], SenderMail, SenderName, [Language], Template)
VALUES ('77204060-8E30-4055-B797-92A324A00214', '762A93A4-56E0-402C-B700-1CFB3362B39D', 2, 2, 'Anmeldebestätigung (Warteliste)', 'noreply@leapinlindy.ch', 'Leapin'' Lindy (noreply)', 'de', 
'<table cellspacing="0" cellpadding="0" align="center">
  <tbody>
    <tr>
      <td style="text-align: center;"><img src="https://scontent-frx5-1.xx.fbcdn.net/v/t31.0-8/22042178_1480177115410174_419267934679857047_o.jpg?oh=d6833f06299ceb688c53b8439413a696&amp;oe=5A7EF2E1" width="800" height="304" /></td>
    </tr>
    <tr>
      <td><br /><strong>Hallo {{FirstName}}!</strong><br />
        <p>Herzlichen Dank f&uuml;r deine Anmeldung zum Leapin'' Lindy 2018!<br /><strong>Du bist auf der Warteliste, bitte bezahle die Anmeldegeb&uuml;hr noch nicht.</strong><br />Beiliegend findest du deine Anmeldedetails.</p>
        <p>Das Leapin'' Lindy Team</p>
        <hr />
        <p>Du hast folgendes gebucht:<br />{{SeatList}}</p>
        <table style="width: 474px;">
          <tbody>
            <tr>
              <td style="width: 129px;"><strong>Name</strong></td>
              <td style="width: 335px;">{{FirstName}} {{LastName}}</td>
            </tr>
            <tr>
              <td style="width: 129px;"><strong>Wohnort</strong></td>
              <td style="width: 335px;">{{City}}</td>
            </tr>
            <tr>
              <td style="width: 129px;"><strong>Telefon</strong></td>
              <td style="width: 335px;">{{Phone}}</td>
            </tr>
            <tr>
              <td style="width: 129px;"><strong>Helfereinsatz</strong></td>
              <td style="width: 335px;">{{Volunteer}}</td>
            </tr>
            <tr>
              <td style="width: 129px;"><strong>Erm&auml;ssigung</strong></td>
              <td style="width: 335px;">{{Reduction}}</td>
            </tr>
            <tr>
              <td style="width: 129px;"><strong>Bemerkung</strong></td>
              <td style="width: 335px;">{{Comments}}</td>
            </tr>
            <tr>
              <td style="width: 129px;"><strong>AGBs</strong></td>
              <td style="width: 335px;">{{AcceptTerms}}</td>
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
        <p>Kannst du kurzfristig nicht teilnehmen, werden wir versuchen, einen Ersatz auf der Warteliste zu finden. Gelingt uns dies, ist eine R&uuml;ckerstattung m&ouml;glich, aber nicht garantiert. Im Grundsatz beh&auml;lt die Regelung G&uuml;ltigkeit.&nbsp;</p>
        <p>In jedem Falle steht es dir frei, selbst einen Ersatz f&uuml;r deinen Workshopplatz oder Partypass zu finden. Bitte kontaktiere uns vor dem Check-In, wenn du deine Anmeldung an eine andere&nbsp; Person &uuml;bertragen hast. Du bleibst aber weiterhin f&uuml;r die Zahlung des Kursgeledes verantwortlich.</p>
        <p><strong>Versicherung<br /></strong>Versicherung ist Sache der Teilnehmenden. Der Veranstalter lehnt jede Haftung ab.</p>
      </td>
    </tr>
  </tbody>
</table>
<p>&nbsp;</p>')


INSERT INTO dbo.MailTemplates(Id, EventId, [Type], ContentType, [Subject], SenderMail, SenderName, [Language], Template)
VALUES ('E5AB6F99-8A43-4D99-9CCD-B10B4F711B5C', '762A93A4-56E0-402C-B700-1CFB3362B39D', 11 /**/, 2, 'Anmeldebestätigung (Warteliste)', 'noreply@leapinlindy.ch', 'Leapin'' Lindy (noreply)', 'de', 
'<table cellspacing="0" cellpadding="0" align="center">
  <tbody>
    <tr>
      <td style="text-align: center;"><img src="https://scontent-frx5-1.xx.fbcdn.net/v/t31.0-8/22042178_1480177115410174_419267934679857047_o.jpg?oh=d6833f06299ceb688c53b8439413a696&amp;oe=5A7EF2E1" width="800" height="304" /></td>
    </tr>
    <tr>
      <td><br /><strong>Hallo {{FirstName}}!</strong><br />
        <p>Herzlichen Dank f&uuml;r deine Anmeldung zum Leapin'' Lindy 2018!<br />Wir haben einen Platz f&uuml;r dich reserviert und warten nun auf die Anmeldung von {{Partner}}. Sobald diese eingegangen ist, erhaltet ihr die Best&auml;tigung eurer Paaranmeldung.<br />Beiliegend findest du deine Anmeldedetails.</p>
        <p>Das Leapin'' Lindy Team</p>
        <hr />
        <p>Du hast folgendes gebucht:<br />{{SeatList}}</p>
        <table style="width: 474px;">
          <tbody>
            <tr>
              <td style="width: 129px;"><strong>Name</strong></td>
              <td style="width: 335px;">{{FirstName}} {{LastName}}</td>
            </tr>
            <tr>
              <td style="width: 129px;"><strong>Wohnort</strong></td>
              <td style="width: 335px;">{{City}}</td>
            </tr>
            <tr>
              <td style="width: 129px;"><strong>Telefon</strong></td>
              <td style="width: 335px;">{{Phone}}</td>
            </tr>
            <tr>
              <td style="width: 129px;"><strong>Helfereinsatz</strong></td>
              <td style="width: 335px;">{{Volunteer}}</td>
            </tr>
            <tr>
              <td style="width: 129px;"><strong>Erm&auml;ssigung</strong></td>
              <td style="width: 335px;">{{Reduction}}</td>
            </tr>
            <tr>
              <td style="width: 129px;"><strong>Bemerkung</strong></td>
              <td style="width: 335px;">{{Comments}}</td>
            </tr>
            <tr>
              <td style="width: 129px;"><strong>AGBs</strong></td>
              <td style="width: 335px;">{{AcceptTerms}}</td>
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
        <p>Kannst du kurzfristig nicht teilnehmen, werden wir versuchen, einen Ersatz auf der Warteliste zu finden. Gelingt uns dies, ist eine R&uuml;ckerstattung m&ouml;glich, aber nicht garantiert. Im Grundsatz beh&auml;lt die Regelung G&uuml;ltigkeit.&nbsp;</p>
        <p>In jedem Falle steht es dir frei, selbst einen Ersatz f&uuml;r deinen Workshopplatz oder Partypass zu finden. Bitte kontaktiere uns vor dem Check-In, wenn du deine Anmeldung an eine andere&nbsp; Person &uuml;bertragen hast. Du bleibst aber weiterhin f&uuml;r die Zahlung des Kursgeledes verantwortlich.</p>
        <p><strong>Versicherung<br /></strong>Versicherung ist Sache der Teilnehmenden. Der Veranstalter lehnt jede Haftung ab.</p>
      </td>
    </tr>
  </tbody>
</table>
<p>&nbsp;</p>')

INSERT INTO dbo.MailTemplates(Id, EventId, [Type], ContentType, [Subject], SenderMail, SenderName, [Language], Template)
VALUES ('C16F52CC-DA0A-4319-8A31-C26582CA6381', '762A93A4-56E0-402C-B700-1CFB3362B39D', 12 /**/, 2, 'Anmeldebestätigung (Warteliste)', 'noreply@leapinlindy.ch', 'Leapin'' Lindy (noreply)', 'de', 
'<table cellspacing="0" cellpadding="0" align="center">
  <tbody>
    <tr>
      <td style="text-align: center;"><img src="https://scontent-frx5-1.xx.fbcdn.net/v/t31.0-8/22042178_1480177115410174_419267934679857047_o.jpg?oh=d6833f06299ceb688c53b8439413a696&amp;oe=5A7EF2E1" width="800" height="304" /></td>
    </tr>
    <tr>
      <td><br /><strong>Hallo {{Follower.FirstName}} &amp; {{FirstName}}</strong><br />
        <p>Herzlichen Dank f&uuml;r eure Paaranmeldung zum Leapin'' Lindy 2018!<br />Wir haben einen Platz f&uuml;r euch reserviert, bitte bezahlt eure Anmeldegeb&uuml;hr&nbsp;innerhalb von 5 Arbeitstagen&nbsp;(Details siehe unten).<br />Beiliegend findet ihr eure Anmeldedetails.</p>
        <p>Das Leapin'' Lindy Team</p>
        <hr />
        <p>Ihr habt folgendes gebucht:<br /><br />Leader:<br />{{SeatList}}</p>
        <table style="width: 474px;">
          <tbody>
            <tr>
              <td style="width: 129px;"><strong>Name</strong></td>
              <td style="width: 335px;">{{FirstName}} {{LastName}}</td>
            </tr>
            <tr>
              <td style="width: 129px;"><strong>Wohnort</strong></td>
              <td style="width: 335px;">{{City}}</td>
            </tr>
            <tr>
              <td style="width: 129px;"><strong>Telefon</strong></td>
              <td style="width: 335px;">{{Phone}}</td>
            </tr>
            <tr>
              <td style="width: 129px;"><strong>Helfereinsatz</strong></td>
              <td style="width: 335px;">{{Volunteer}}</td>
            </tr>
            <tr>
              <td style="width: 129px;"><strong>Erm&auml;ssigung</strong></td>
              <td style="width: 335px;">{{Reduction}}</td>
            </tr>
            <tr>
              <td style="width: 129px;"><strong>Bemerkung</strong></td>
              <td style="width: 335px;">{{Comments}}</td>
            </tr>
            <tr>
              <td style="width: 129px;"><strong>AGBs</strong></td>
              <td style="width: 335px;">{{AcceptTerms}}</td>
            </tr>
          </tbody>
        </table>
        <p>&nbsp;Follower:<br />{{Follower.SeatList}}</p>
        <table style="width: 474px;">
          <tbody>
            <tr>
              <td style="width: 129px;"><strong>Name</strong></td>
              <td style="width: 335px;">{{Follower.FirstName}} {{Follower.LastName}}</td>
            </tr>
            <tr>
              <td style="width: 129px;"><strong>Wohnort</strong></td>
              <td style="width: 335px;">{{Follower.City}}</td>
            </tr>
            <tr>
              <td style="width: 129px;"><strong>Telefon</strong></td>
              <td style="width: 335px;">{{Follower.Phone}}</td>
            </tr>
            <tr>
              <td style="width: 129px;"><strong>Helfereinsatz</strong></td>
              <td style="width: 335px;">{{Follower.Volunteer}}</td>
            </tr>
            <tr>
              <td style="width: 129px;"><strong>Erm&auml;ssigung</strong></td>
              <td style="width: 335px;">{{Follower.Reduction}}</td>
            </tr>
            <tr>
              <td style="width: 129px;"><strong>Bemerkung</strong></td>
              <td style="width: 335px;">{{Follower.Comments}}</td>
            </tr>
            <tr>
              <td style="width: 129px;"><strong>AGBs</strong></td>
              <td style="width: 335px;">{{Follower.AcceptTerms}}</td>
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
        <p>Kannst du kurzfristig nicht teilnehmen, werden wir versuchen, einen Ersatz auf der Warteliste zu finden. Gelingt uns dies, ist eine R&uuml;ckerstattung m&ouml;glich, aber nicht garantiert. Im Grundsatz beh&auml;lt die Regelung G&uuml;ltigkeit.&nbsp;</p>
        <p>In jedem Falle steht es dir frei, selbst einen Ersatz f&uuml;r deinen Workshopplatz oder Partypass zu finden. Bitte kontaktiere uns vor dem Check-In, wenn du deine Anmeldung an eine andere&nbsp; Person &uuml;bertragen hast. Du bleibst aber weiterhin f&uuml;r die Zahlung des Kursgeledes verantwortlich.</p>
        <p><strong>Versicherung<br /></strong>Versicherung ist Sache der Teilnehmenden. Der Veranstalter lehnt jede Haftung ab.</p>
      </td>
    </tr>
  </tbody>
</table>
<p>&nbsp;</p>')
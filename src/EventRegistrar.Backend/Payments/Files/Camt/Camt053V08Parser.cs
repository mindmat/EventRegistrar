using System.Globalization;
using System.Xml;
using System.Xml.Linq;

using EventRegistrar.Backend.Infrastructure;

namespace EventRegistrar.Backend.Payments.Files.Camt;

public class Camt053V08Parser
{
    public const string Namespace = "urn:iso:std:iso:20022:tech:xsd:camt.053.001.08";

    public CamtFile Parse(Stream stream)
    {
        var xml = XDocument.Load(stream);
        return Parse(xml);
    }

    public CamtFile Parse(XDocument xml)
    {
        XNamespace ns = Namespace;
        if (xml.NodeType != XmlNodeType.Document || ((XElement)xml.FirstNode!).GetDefaultNamespace().NamespaceName != ns)
        {
            throw new Exception("invalid xml");
        }

        var statement = xml.Descendants(ns + "Stmt").ToList();
        var entries = statement.Descendants(ns + "Ntry")
                               .Select(ntry =>
                               {
                                   var bookingDate = DateTime.Parse(ntry.Descendants(ns + "BookgDt")
                                                                        .Descendants(ns + "Dt")
                                                                        .First()
                                                                        .Value);

                                   var amountNode = ntry.Element(ns + "Amt")!;
                                   var amount = decimal.Parse(amountNode.Value, CultureInfo.InvariantCulture);
                                   var charges = ntry.Descendants(ns + "Chrgs")
                                                     .Descendants(ns + "TtlChrgsAndTaxAmt")
                                                     .FirstOrDefault()
                                                     ?.Value.TryToDecimal();

                                   var txDetails = ntry.Descendants(ns + "NtryDtls")
                                                       .Descendants(ns + "TxDtls")
                                                       .FirstOrDefault();

                                   var parties = txDetails?.Descendants(ns + "RltdPties")
                                                          .FirstOrDefault();

                                   var message = txDetails?.Descendants(ns + "RmtInf")
                                                          ?.Descendants(ns + "Ustrd")
                                                          ?.Select(nod => nod.Value)
                                                          ?.StringJoin(Environment.NewLine);

                                   return new CamtEntry
                                          {
                                              Amount = amount,
                                              Currency = amountNode.Attribute("Ccy")?.Value,
                                              Info = ntry.Element(ns + "AddtlNtryInf")?.Value,
                                              Message = message,
                                              Type = (CreditDebit)Enum.Parse(typeof(CreditDebit),
                                                                             ntry.Element(ns + "CdtDbtInd")!.Value),
                                              BookingDate = bookingDate,
                                              Reference = ntry.Element(ns + "AcctSvcrRef")?.Value,
                                              Charges = charges,
                                              InstructionIdentification = txDetails?.Descendants(ns + "Refs")
                                                                                   .Descendants(ns + "InstrId")
                                                                                   .FirstOrDefault()
                                                                                   ?.Value,
                                              DebitorName = parties?.Descendants(ns + "Dbtr")
                                                                   .Descendants(ns + "Nm")
                                                                   .FirstOrDefault()
                                                                   ?.Value,
                                              DebitorStreet = parties?.Descendants(ns + "Dbtr")
                                                                     .Descendants(ns + "PstlAdr")
                                                                     .Descendants(ns + "StrtNm")
                                                                     .FirstOrDefault()
                                                                     ?.Value,
                                              DebitorBuildingNr = parties?.Descendants(ns + "Dbtr")
                                                                         .Descendants(ns + "PstlAdr")
                                                                         .Descendants(ns + "BldgNb")
                                                                         .FirstOrDefault()
                                                                         ?.Value,
                                              DebitorZip = parties?.Descendants(ns + "Dbtr")
                                                                  .Descendants(ns + "PstlAdr")
                                                                  .Descendants(ns + "PstCd")
                                                                  .FirstOrDefault()
                                                                  ?.Value,
                                              DebitorTown = parties?.Descendants(ns + "Dbtr")
                                                                   .Descendants(ns + "PstlAdr")
                                                                   .Descendants(ns + "TwnNm")
                                                                   .FirstOrDefault()
                                                                   ?.Value,
                                              DebitorCountry = parties?.Descendants(ns + "Dbtr")
                                                                      .Descendants(ns + "PstlAdr")
                                                                      .Descendants(ns + "Ctry")
                                                                      .FirstOrDefault()
                                                                      ?.Value,
                                              DebitorAdressLine = parties?.Descendants(ns + "Dbtr")
                                                                         .Descendants(ns + "PstlAdr")
                                                                         .Descendants(ns + "AdrLine")
                                                                         .FirstOrDefault()
                                                                         ?.Value,
                                              DebitorIban = parties?.Descendants(ns + "DbtrAcct")
                                                                   .Descendants(ns + "Id")
                                                                   .Descendants(ns + "IBAN")
                                                                   .FirstOrDefault()
                                                                   ?.Value,
                                              CreditorName = parties?.Descendants(ns + "Cdtr")
                                                                    .Descendants(ns + "Nm")
                                                                    .FirstOrDefault()
                                                                    ?.Value,
                                              CreditorIban = parties?.Descendants(ns + "CdtrAcct")
                                                                    .Descendants(ns + "Id")
                                                                    .Descendants(ns + "IBAN")
                                                                    .FirstOrDefault()
                                                                    ?.Value,
                                              Xml = ntry.ToString()
                                          };
                               });

        var camt = new CamtFile
                   {
                       Account = statement.Descendants(ns + "Acct")
                                          .FirstOrDefault()
                                          ?.Descendants(ns + "Id")
                                          .FirstOrDefault()
                                          ?.Descendants(ns + "IBAN")
                                          .FirstOrDefault()
                                          ?.Value,
                       Owner = statement.Descendants(ns + "Acct")
                                        .FirstOrDefault()
                                        ?.Descendants(ns + "Ownr")
                                        .FirstOrDefault()
                                        ?.Descendants(ns + "Nm")
                                        .FirstOrDefault()
                                        ?.Value,
                       FileId = xml.Descendants(ns + "GrpHdr")
                                   .FirstOrDefault()
                                   ?.Descendants(ns + "MsgId")
                                   .FirstOrDefault()
                                   ?.Value,
                       Balance = decimal.Parse(
                           statement.Descendants(ns + "Bal").Last().Descendants(ns + "Amt").First().Value,
                           CultureInfo.InvariantCulture),
                       Currency = statement.Descendants(ns + "Bal")
                                           .Last()
                                           .Descendants(ns + "Amt")
                                           .First()
                                           .Attribute("Ccy")
                                           ?.Value,
                       Entries = entries.ToList()
                   };
        var filePeriod = statement.Descendants(ns + "FrToDt").FirstOrDefault();
        if (filePeriod != null)
        {
            if (DateTime.TryParse(filePeriod.Descendants(ns + "FrDtTm").FirstOrDefault()?.Value, out var from))
            {
                camt.BookingsFrom = @from;
            }

            if (DateTime.TryParse(filePeriod.Descendants(ns + "ToDtTm").FirstOrDefault()?.Value, out var to))
            {
                camt.BookingsTo = to;
            }
        }

        return camt;
    }
}
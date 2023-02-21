using System.Globalization;
using System.Xml;
using System.Xml.Linq;

using EventRegistrar.Backend.Infrastructure;

namespace EventRegistrar.Backend.Payments.Files.Camt;

public class CamtParser
{
    public CamtFile Parse(Stream stream)
    {
        var xml = XDocument.Load(stream);
        return Parse(xml);
    }

    public CamtFile Parse(XDocument xml)
    {
        XNamespace ns = "urn:iso:std:iso:20022:tech:xsd:camt.053.001.04";
        if (xml.NodeType != XmlNodeType.Document || ((XElement)xml.FirstNode).GetDefaultNamespace().NamespaceName != ns)
        {
            throw new Exception("invalid xml");
        }

        var statement = xml.Descendants(ns + "Stmt").ToList();
        var entries = statement.Descendants(ns + "Ntry")
                               .SelectMany(ntry =>
                               {
                                   var bookingDate = DateTime.Parse(ntry.Descendants(ns + "BookgDt")
                                                                        .Descendants(ns + "Dt")
                                                                        .First()
                                                                        .Value);
                                   return ntry.Descendants(ns + "TxDtls")
                                              .Select(tx =>
                                              {
                                                  var amountNode = tx.Descendants(ns + "Amt").First();
                                                  var instructedAmountNode = tx.Descendants(ns + "AmtDtls")
                                                                               ?.Descendants("InstdAmt")
                                                                               ?.Descendants("Amt")
                                                                               .FirstOrDefault();
                                                  var amount = decimal.Parse(instructedAmountNode?.Value
                                                                          ?? amountNode.Value, CultureInfo.InvariantCulture);
                                                  var charges = instructedAmountNode == null
                                                                    ? ntry.Descendants(ns + "Chrgs")
                                                                          .Descendants(ns + "TtlChrgsAndTaxAmt")
                                                                          .FirstOrDefault()
                                                                          ?.Value.TryToDecimal()
                                                                   ?? tx.Descendants(ns + "Chrgs")
                                                                        .Descendants(ns + "Rcrd")
                                                                        .Descendants(ns + "Amt")
                                                                        .FirstOrDefault()
                                                                        ?.Value.TryToDecimal()
                                                                    : amount - (instructedAmountNode.Value.TryToDecimal() ?? 0);
                                                  var parties = tx.Descendants(ns + "RltdPties")
                                                                  .FirstOrDefault();
                                                  return new CamtEntry
                                                         {
                                                             Amount = amount,
                                                             Currency = amountNode.Attribute("Ccy")?.Value,
                                                             Info = tx.Descendants(ns + "AddtlNtryInf").FirstOrDefault()?.Value,
                                                             Message = tx.Descendants(ns + "RmtInf")
                                                                         ?.Descendants(ns + "Ustrd")
                                                                         ?.Select(nod => nod.Value)
                                                                         ?.StringJoin(Environment.NewLine),
                                                             Type = (CreditDebit)Enum.Parse(typeof(CreditDebit),
                                                                                            tx.Descendants(ns + "CdtDbtInd").First().Value),
                                                             BookingDate = bookingDate,
                                                             Reference = tx.Descendants(ns + "AcctSvcrRef")
                                                                           .FirstOrDefault()
                                                                           ?.Value,
                                                             Charges = charges,
                                                             InstructionIdentification = tx.Descendants(ns + "Refs")
                                                                                           .Descendants(ns + "InstrId")
                                                                                           .FirstOrDefault()
                                                                                           ?.Value,
                                                             DebitorName = parties?.Descendants(ns + "Dbtr")
                                                                                  .Descendants(ns + "Nm")
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
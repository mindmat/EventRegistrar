﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace EventRegistrator.Functions.Payments
{
    public class CamtParser
    {
        public static CamtFile Parse(Stream stream)
        {
            var xml = XDocument.Load(stream);
            return Parse(xml);
        }

        public static CamtFile Parse(XDocument xml)
        {
            XNamespace ns = "urn:iso:std:iso:20022:tech:xsd:camt.053.001.04";
            if (xml.NodeType != XmlNodeType.Document ||
                ((XElement)xml.FirstNode).GetDefaultNamespace()?.NamespaceName != ns)
            {
                throw new Exception("invalid xml");
            }

            var statement = xml.Descendants(ns + "Stmt").ToList();
            var entries = statement.Descendants(ns + "Ntry").Select(ntry => new CamtEntry
            {
                Amount = decimal.Parse(ntry.Descendants(ns + "Amt").First().Value, CultureInfo.InvariantCulture),
                Currency = ntry.Descendants(ns + "Amt").First().Attribute("Ccy")?.Value,
                Info = ntry.Descendants(ns + "AddtlNtryInf").FirstOrDefault()?.Value,
                Type = (CreditDebit)Enum.Parse(typeof(CreditDebit), ntry.Descendants(ns + "CdtDbtInd").First().Value),
                BookingDate = DateTime.Parse(ntry.Descendants(ns + "BookgDt").Descendants(ns + "Dt").First().Value),
                Reference = ntry.Descendants(ns + "AcctSvcrRef").FirstOrDefault()?.Value,
            });

            var camt = new CamtFile
            {
                Account = statement.Descendants(ns + "Acct").FirstOrDefault()?.Descendants(ns + "Id").FirstOrDefault()?.Descendants(ns + "IBAN")?.FirstOrDefault()?.Value,
                Owner = statement.Descendants(ns + "Acct").FirstOrDefault()?.Descendants(ns + "Ownr").FirstOrDefault()?.Descendants(ns + "Nm")?.FirstOrDefault()?.Value,
                FileId = xml.Descendants(ns + "GrpHdr").FirstOrDefault()?.Descendants(ns + "MsgId").FirstOrDefault()?.Value,
                Entries = entries.ToList()
            };
            return camt;
        }
    }
}
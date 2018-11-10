using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using ClosedXML.Excel;
using EventRegistrar.Backend.Events;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Registrations.Overview
{
    public class OverviewController : Controller
    {
        private readonly IEventAcronymResolver _eventAcronymResolver;
        private readonly IMediator _mediator;

        public OverviewController(IMediator mediator,
                                  IEventAcronymResolver eventAcronymResolver)
        {
            _mediator = mediator;
            _eventAcronymResolver = eventAcronymResolver;
        }

        [HttpGet("api/events/{eventAcronym}/checkinView")]
        public async Task<CheckinView> GetCheckinView(string eventAcronym)
        {
            return await _mediator.Send(new CheckinQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
            });
        }

        [HttpGet("api/events/{eventAcronym}/checkinView.xlsx")]
        public async Task<IActionResult> GetCheckinViewXlsx(string eventAcronym)
        {
            var data = await _mediator.Send(new CheckinQuery
            {
                EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
            });

            var mappings = new List<(string Title, Func<CheckinViewItem, object> GetValue)>
            {
                ("Vorname", itm => itm.FirstName),
                ("Nachname", itm => itm.LastName)
            };
            foreach (var header in data.DynamicHeaders)
            {
                mappings.Add((header, itm => itm.Columns[header]));
            }
            mappings.Add(("Status", itm => itm.Status));
            mappings.Add(("Ausstehend", itm => itm.UnsettledAmount));

            var dataTable = new DataTable("Checkin");

            foreach (var mapping in mappings)
            {
                dataTable.Columns.Add(mapping.Title);
            }

            foreach (var registration in data.Items)
            {
                var row = dataTable.NewRow();
                foreach (var (title, getValue) in mappings)
                {
                    row[title] = getValue(registration);
                }

                dataTable.Rows.Add(row);
            }

            var workbook = new XLWorkbook();
            var worksheet = workbook.AddWorksheet(dataTable, "Checkin");
            //worksheet.SortColumns.Add(1, XLSortOrder.Ascending);
            //worksheet.SortColumns.Add(2, XLSortOrder.Ascending);
            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream, "application/octet-stream");
        }
    }
}
using ClosedXML.Excel;
using ClosedXML.Graphics;

using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.Mediator;

namespace EventRegistrar.Backend.VolunteerPlanning;

public class ShiftsOverviewExcelQuery : IRequest<DownloadResult>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class ShiftsOverviewExcelQueryHandler(ReadModelReader readModelReader)
    : IRequestHandler<ShiftsOverviewExcelQuery, DownloadResult>
{
    private static readonly XLColor HeaderBackground = XLColor.FromHtml("#333333");
    private static readonly XLColor EmptyHelperBackground = XLColor.FromHtml("#00FF00");

    public async Task<DownloadResult> Handle(ShiftsOverviewExcelQuery query, CancellationToken cancellationToken)
    {
        var shiftGroups = await readModelReader.GetDeserialized<IEnumerable<ShiftGroup>>(nameof(ShiftsOverviewQuery),
                                                                                         query.EventId,
                                                                                         null,
                                                                                         cancellationToken);

        var groups = shiftGroups.ToList();
        var maxHelpers = groups.SelectMany(g => g.Shifts)
                               .Select(s => s.Assignments.Count)
                               .DefaultIfEmpty(0)
                               .Max();

        LoadOptions.DefaultGraphicEngine = new DefaultGraphicEngine("DejaVu Sans");
        using var workbook = new XLWorkbook();
        var ws = workbook.AddWorksheet("Helpers");

        var row = 2;

        // Title
        ws.Cell(row, 4).Value = "Helpers list";
        ws.Cell(row, 4).Style.Font.FontSize = 20;
        ws.Cell(row, 4).Style.Font.Bold = true;
        row += 3;

        foreach (var group in groups)
        {
            // Group header (e.g. "FRIDAY@WORKSHOP (LeCap)")
            var dayName = group.Day.ToString("dddd").ToUpperInvariant();
            var groupTitle = string.IsNullOrEmpty(group.Location)
                                 ? dayName
                                 : $"{dayName}@{group.Location}";
            ws.Cell(row, 1).Value = groupTitle;
            ws.Cell(row, 1).Style.Font.FontSize = 18;
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Range(row, 1, row, 5 + maxHelpers).Merge();
            row += 3;

            // Column headers
            WriteHeaderRow(ws, row, maxHelpers);
            row++;

            // Data rows
            var colorIndex = 0;
            foreach (var shift in group.Shifts)
            {
                var rowColor = colorIndex % 2 == 0 ? XLColor.Cyan : XLColor.Magenta;
                WriteDataRow(ws, row, shift, maxHelpers, rowColor);
                row++;
                colorIndex++;
            }

            row += 2;
        }

        try
        {
            ws.Columns().AdjustToContents();
        }
        catch
        {
            // Font not available in sandbox environment
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return new DownloadResult
               {
                   ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                   Content = stream.ToArray(),
                   Filename = "Helpers.xlsx"
               };
    }

    private static void WriteHeaderRow(IXLWorksheet ws, int row, int maxHelpers)
    {
        var headers = new[] { "WHEN", "", "", "POST", "RESPONSIBLE" };
        for (var c = 0; c < headers.Length; c++)
        {
            var cell = ws.Cell(row, c + 1);
            cell.Value = headers[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = HeaderBackground;
            cell.Style.Font.FontColor = XLColor.White;
        }

        for (var h = 0; h < maxHelpers; h++)
        {
            var cell = ws.Cell(row, 6 + h);
            cell.Value = $"HELPER {h + 1}";
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = HeaderBackground;
            cell.Style.Font.FontColor = XLColor.White;
        }
    }

    private static void WriteDataRow(IXLWorksheet ws, int row, ShiftDisplayItem shift, int maxHelpers, XLColor rowColor)
    {
        // Col A: Start time
        ws.Cell(row, 1).Value = shift.StartTime.LocalDateTime.ToString("HH:mm");
        // Col B: Separator
        ws.Cell(row, 2).Value = "-";
        // Col C: End time
        ws.Cell(row, 3).Value = shift.EndTime.LocalDateTime.ToString("HH:mm");
        // Col D: Post
        var postParts = new[] { shift.Name, shift.Location }.Where(p => !string.IsNullOrEmpty(p));
        ws.Cell(row, 4).Value = string.Join(" / ", postParts);
        // Col E: Responsible
        var responsible = shift.ParticipantResponsible;
        if (shift.IsParticipantResponsibleCancelled && responsible != null)
        {
            responsible += " (CANCELLED)";
        }

        ws.Cell(row, 5).Value = responsible;
        ws.Cell(row, 5).Style.Font.Bold = true;

        // Apply row color to fixed columns
        for (var c = 1; c <= 5; c++)
        {
            ws.Cell(row, c).Style.Fill.BackgroundColor = rowColor;
        }

        // Helper columns
        for (var h = 0; h < maxHelpers; h++)
        {
            var cell = ws.Cell(row, 6 + h);
            if (h < shift.Assignments.Count)
            {
                var assignment = shift.Assignments.ElementAt(h);
                var cancelledSuffix = assignment.IsRegistrationCancelled ? " (CANCELLED)" : "";
                cell.Value = $"{assignment.Participant}{cancelledSuffix}";
                cell.Style.Fill.BackgroundColor = rowColor;
            }
            else
            {
                cell.Value = "";
                cell.Style.Fill.BackgroundColor = EmptyHelperBackground;
            }
        }
    }
}
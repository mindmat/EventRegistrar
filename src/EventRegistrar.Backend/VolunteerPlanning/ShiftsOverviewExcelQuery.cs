using ClosedXML.Excel;
using ClosedXML.Graphics;

using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.Mediator;

namespace EventRegistrar.Backend.VolunteerPlanning;

public class ShiftsOverviewExcelQuery : IRequest<DownloadResult>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public string? TimeZoneId { get; set; }
}

public class ShiftsOverviewExcelQueryHandler(ReadModelReader readModelReader)
    : IRequestHandler<ShiftsOverviewExcelQuery, DownloadResult>
{
    // Colors matching the Angular view (Tailwind light-mode palette)
    private static readonly XLColor HeaderBackground = XLColor.FromHtml("#333333");
    private static readonly XLColor WhenBackground = XLColor.FromHtml("#DBEAFE");              // blue-100
    private static readonly XLColor PostBackground = XLColor.FromHtml("#F3F4F6");              // gray-100
    private static readonly XLColor ResponsibleBackground = XLColor.FromHtml("#F3E8FF");       // purple-100
    private static readonly XLColor CancelledBackground = XLColor.FromHtml("#FEE2E2");         // red-100
    private static readonly XLColor HelperConfirmedBackground = XLColor.FromHtml("#BBF7D0");   // green-200
    private static readonly XLColor HelperUnconfirmedBackground = XLColor.FromHtml("#F0FDF4"); // green-50
    private static readonly XLColor HelperUnconfirmedFontColor = XLColor.FromHtml("#9CA3AF");  // gray-400
    private static readonly XLColor HelperUnassignedBackground = XLColor.FromHtml("#F0FDF4");  // green-50

    private const double DataRowHeight = 30;

    public async Task<DownloadResult> Handle(ShiftsOverviewExcelQuery query, CancellationToken cancellationToken)
    {
        var shiftGroups = await readModelReader.GetDeserialized<IEnumerable<ShiftGroup>>(nameof(ShiftsOverviewQuery),
                                                                                         query.EventId,
                                                                                         null,
                                                                                         cancellationToken);

        var groups = shiftGroups.ToList();
        var timeZone = GetTimeZone(query.TimeZoneId);

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
            var groupMaxHelpers = group.Shifts
                                       .Select(s => Math.Max(s.Assignments.Count, s.HelpersNeeded))
                                       .DefaultIfEmpty(0)
                                       .Max();

            // Group header (e.g. "FRIDAY@WORKSHOP (LeCap)")
            var dayName = group.Day.ToString("dddd").ToUpperInvariant();
            var groupTitle = string.IsNullOrEmpty(group.Location)
                                 ? dayName
                                 : $"{dayName}@{group.Location}";
            ws.Cell(row, 1).Value = groupTitle;
            ws.Cell(row, 1).Style.Font.FontSize = 18;
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Range(row, 1, row, 5 + groupMaxHelpers).Merge();
            row += 3;

            // Column headers
            WriteHeaderRow(ws, row, groupMaxHelpers);
            row++;

            // Data rows
            foreach (var shift in group.Shifts)
            {
                WriteDataRow(ws, row, shift, groupMaxHelpers, timeZone);
                ws.Row(row).Height = DataRowHeight;
                row++;
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
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }

        for (var h = 0; h < maxHelpers; h++)
        {
            var cell = ws.Cell(row, 6 + h);
            cell.Value = $"HELPER {h + 1}";
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = HeaderBackground;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }
    }

    private static TimeZoneInfo GetTimeZone(string? timeZoneId)
    {
        if (!string.IsNullOrEmpty(timeZoneId))
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException) { }
        }

        return TimeZoneInfo.Local;
    }

    private static void WriteDataRow(IXLWorksheet ws,
                                     int row,
                                     ShiftDisplayItem shift,
                                     int maxHelpers,
                                     TimeZoneInfo timeZone)
    {
        // Col A-C: Time (blue-100, matching bg-blue-100 in the view)
        ws.Cell(row, 1).Value = TimeZoneInfo.ConvertTime(shift.StartTime, timeZone).ToString("HH:mm");
        ws.Cell(row, 2).Value = "-";
        ws.Cell(row, 3).Value = TimeZoneInfo.ConvertTime(shift.EndTime, timeZone).ToString("HH:mm");
        for (var c = 1; c <= 3; c++)
        {
            ws.Cell(row, c).Style.Fill.BackgroundColor = WhenBackground;
        }

        // Vertical center alignment for all columns in this row
        for (var c = 1; c <= 5 + maxHelpers; c++)
        {
            ws.Cell(row, c).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }

        // Col D: Post (gray-100, matching bg-gray-100 in the view)
        var postParts = new[] { shift.Name, shift.Location }.Where(p => !string.IsNullOrEmpty(p));
        ws.Cell(row, 4).Value = string.Join(" / ", postParts);
        ws.Cell(row, 4).Style.Fill.BackgroundColor = PostBackground;

        // Col E: Responsible (purple-100 or red-100 if cancelled)
        var responsible = shift.ParticipantResponsible;
        if (shift.IsParticipantResponsibleCancelled && responsible != null)
        {
            responsible += " (CANCELLED)";
        }

        ws.Cell(row, 5).Value = responsible;
        ws.Cell(row, 5).Style.Font.Bold = true;

        if (shift.ResponsibleRegistrationId != null)
        {
            ws.Cell(row, 5).Style.Fill.BackgroundColor = shift.IsParticipantResponsibleCancelled
                                                             ? CancelledBackground
                                                             : ResponsibleBackground;
            if (shift.IsParticipantResponsibleCancelled)
            {
                ws.Cell(row, 5).Style.Font.FontColor = XLColor.DarkRed;
                ws.Cell(row, 5).Style.Font.Strikethrough = true;
            }
        }

        // Helper columns
        for (var h = 0; h < maxHelpers; h++)
        {
            var cell = ws.Cell(row, 6 + h);

            if (h >= shift.HelpersNeeded)
            {
                // Beyond needed slots: no color (unassigned, not needed)
                cell.Value = "";
                continue;
            }

            if (h < shift.Assignments.Count)
            {
                var assignment = shift.Assignments.ElementAt(h);
                if (assignment.RegistrationId != Guid.Empty)
                {
                    var cancelledSuffix = assignment.IsRegistrationCancelled ? " (CANCELLED)" : "";
                    cell.Value = $"{assignment.Participant}{cancelledSuffix}";

                    if (assignment.IsRegistrationCancelled)
                    {
                        cell.Style.Fill.BackgroundColor = CancelledBackground;
                        cell.Style.Font.FontColor = XLColor.DarkRed;
                        cell.Style.Font.Strikethrough = true;
                    }
                    else if (assignment.IsConfirmed)
                    {
                        cell.Style.Fill.BackgroundColor = HelperConfirmedBackground;
                    }
                    else
                    {
                        cell.Value = $"{assignment.Participant} ?";
                        cell.Style.Fill.BackgroundColor = HelperUnconfirmedBackground;
                        cell.Style.Font.FontColor = HelperUnconfirmedFontColor;
                    }
                }
                else
                {
                    // Slot exists but no one assigned: light green-50
                    cell.Value = "";
                    cell.Style.Fill.BackgroundColor = HelperUnassignedBackground;
                }
            }
            else
            {
                // Open slot needing a helper: light green-50
                cell.Value = "";
                cell.Style.Fill.BackgroundColor = HelperUnassignedBackground;
            }
        }
    }
}
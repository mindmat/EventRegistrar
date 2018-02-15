using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.Registrations
{
    public static class GetCheckinCsv
    {
        [FunctionName("GetCheckinCsv")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "events/{eventIdString:guid}/checkin.xlsx")]
            HttpRequestMessage req,
            string eventIdString,
            TraceWriter log)
        {
            var eventId = Guid.Parse(eventIdString);

            var registrations = await CheckinDataView.GetCheckinData(eventId);

            var dataTable = new DataTable("Checkin");
            var properties = typeof(CheckinDataView.CheckinItem).GetProperties().Where(prp => !(prp.PropertyType.IsGenericType && prp.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))).ToList();
            foreach (var property in properties)
            {
                dataTable.Columns.Add(property.Name, property.PropertyType);
            }

            foreach (var registration in registrations)
            {
                var row = dataTable.NewRow();
                foreach (var property in properties)
                {
                    row[property.Name] = property.GetValue(registration);
                }

                dataTable.Rows.Add(row);
            }

            var workbook = new XLWorkbook();
            var worksheet = workbook.AddWorksheet(dataTable, "Checkin");

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Content = new StreamContent(stream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return response;
        }
    }
}

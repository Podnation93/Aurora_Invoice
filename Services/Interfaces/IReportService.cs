using System.Threading.Tasks;
using AuroraInvoice.Models;
using System;

namespace AuroraInvoice.Services.Interfaces;

public interface IReportService
{
    Task<GstSummary> GetGstSummaryAsync(DateTime startDate, DateTime endDate);
}

public class GstSummary
{
    public decimal GstCollected { get; set; }
    public decimal GstPaid { get; set; }
    public decimal NetGst => GstCollected - GstPaid;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

using Core.Application.ViewModels.Report;

namespace Core.Application.Interfaces
{
    public interface IReportService
    {
        ReportViewModel GetReportInfo(string startDate, string endDate);
    }
}

using System;

namespace SimpleTextPreprocessor;

public interface IReport
{
    string CurrentFileId { get; set; }
    int CurrentLine { get; set; }
    int CurrentColumn { get; set; }

    void Error(string message);
    void Exception(Exception e);
}

public static class ReportExtensions
{
    public static void Error(this IReport report, string fileId, int line, int column, string message)
    {
        report.CurrentFileId = fileId;
        report.CurrentLine = line;
        report.CurrentColumn = column;
        report.Error(message);
    }
    
    public static void Exception(this IReport report, string fileId, int line, int column, Exception e)
    {
        report.CurrentFileId = fileId;
        report.CurrentLine = line;
        report.CurrentColumn = column;
        report.Exception(e);
    }
}
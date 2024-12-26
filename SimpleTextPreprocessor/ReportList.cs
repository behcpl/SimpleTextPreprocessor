using System;
using System.Collections.Generic;

namespace SimpleTextPreprocessor;

public class ReportList : IReport
{
    public readonly List<Entry> Entries = [];

    public string CurrentFileId { get; set; } = string.Empty;
    public int CurrentLine { get; set; }
    public int CurrentColumn { get; set; }

    public void Error(string message)
    {
        Entries.Add(new Entry(CurrentFileId, CurrentLine, CurrentColumn, message, null));
    }

    public void Exception(Exception e)
    {
        Entries.Add(new Entry(CurrentFileId, CurrentLine, CurrentColumn, e.Message, e));
    }

    public readonly struct Entry
    {
        public readonly string FileId;
        public readonly int Line;
        public readonly int Column;
        public readonly string Message;
        public readonly Exception? Exception;

        public Entry(string fileId, int line, int column, string message, Exception? exception)
        {
            FileId = fileId;
            Line = line;
            Column = column;
            Message = message;
            Exception = exception;
        }
    }
}
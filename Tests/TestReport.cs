using System;
using System.Collections.Generic;
using SimpleTextPreprocessor;

namespace Tests;

public class TestReport : IReport
{
    public readonly struct Entry
    {
        public readonly string FileId;
        public readonly int Line;
        public readonly int Column;
        public readonly string Message;

        public Entry(string fileId, int line, int column, string message)
        {
            FileId = fileId;
            Line = line;
            Column = column;
            Message = message;
        }
    }

    public readonly List<Entry> Entries = [];

    public TestReport()
    {
        CurrentFileId = string.Empty;
    }

    public void Error(string fileId, int line, int column, string message) { }

    public string CurrentFileId { get; set; }

    public int CurrentLine { get; set; }

    public int CurrentColumn { get; set; }

    public void Error(string message)
    {
        Entries.Add(new Entry(CurrentFileId, CurrentLine, CurrentColumn, message));
    }

    public void Exception(Exception e)
    {
        throw new NotImplementedException();
    }
}
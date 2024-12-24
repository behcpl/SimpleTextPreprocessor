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

    public void Error(string fileId, int line, int column, string message)
    {
        Entries.Add(new Entry(fileId, line, column, message));
    }
}
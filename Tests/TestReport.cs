using System.Collections.Generic;
using SimpleTextPreprocessor;

namespace Tests;

public class TestReport : IReport
{
    public readonly struct Entry
    {
        public readonly int FileId;
        public readonly string FilePath;
        public readonly int Line;
        public readonly int Column;
        public readonly string Message;

        public Entry(int fileId, string filePath, int line, int column, string message)
        {
            FileId = fileId;
            FilePath = filePath;
            Line = line;
            Column = column;
            Message = message;
        }
    }

    public readonly List<Entry> Entries = [];

    public void Error(int fileId, string filePath, int line, int column, string message)
    {
        Entries.Add(new Entry(fileId, filePath, line, column, message));
    }
}
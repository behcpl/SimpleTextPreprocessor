using System.Collections.Generic;

namespace SimpleTextPreprocessor;

/// <summary>
/// Container for storing line number mapping
/// </summary>
public class LineNumberMapper
{
    public int EntriesCount => _entries.Count;

    private readonly List<Entry> _entries = [];

    public bool TryGetSource(int lineNumber, out string sourceFileId, out int sourceLineNumber)
    {
        if (lineNumber < 0 || lineNumber >= _entries.Count)
        {
            sourceFileId = string.Empty;
            sourceLineNumber = -1;
            return false;
        }

        sourceFileId = _entries[lineNumber].FileId;
        sourceLineNumber = _entries[lineNumber].LineNumber;
        return true;
    }

    public (string sourceFileId, int sourceLineNumber) GetSource(int lineNumber)
    {
        return (_entries[lineNumber].FileId, _entries[lineNumber].LineNumber);
    }

    internal void Clear() => _entries.Clear();
    
    internal void AddEntry(string sourceFileId, int sourceLineNumber)
    {
        _entries.Add(new Entry(sourceFileId, sourceLineNumber));
    }

    private readonly struct Entry
    {
        public readonly string FileId;
        public readonly int LineNumber;

        public Entry(string fileId, int lineNumber)
        {
            FileId = fileId;
            LineNumber = lineNumber;
        }
    }
}
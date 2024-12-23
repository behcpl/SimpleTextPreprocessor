using System.Collections.Generic;

namespace SimpleTextPreprocessor;

public class SourceMap
{
    public List<Entry> Entries;
    
    public SourceMap()
    {
        Entries = new List<Entry>();
    }

    public struct Entry
    {
        public int Line;

        public int SourceLine;
        public string SourcePath;
    }
}
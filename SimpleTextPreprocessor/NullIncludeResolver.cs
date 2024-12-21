using System.IO;

namespace SimpleTextPreprocessor;

public class NullIncludeResolver : IIncludeResolver
{
    public TextReader CreateReader(string currentPath, string includePath)
    {
        return new StringReader(string.Empty);
    }
}
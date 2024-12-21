using System.IO;

namespace SimpleTextPreprocessor;

public class FileSystemIncludeResolver : IIncludeResolver
{
    public TextReader CreateReader(string currentPath, string includePath)
    {
        // TODO: check for absolute path?
        return new StreamReader(Path.Combine(currentPath, includePath));
    }
}
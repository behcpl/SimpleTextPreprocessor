using System.IO;

namespace SimpleTextPreprocessor;

public interface IIncludeResolver
{
    TextReader CreateReader(string currentPath, string includePath);
}
using System.IO;

namespace SimpleTextPreprocessor;

// TODO: error line numbers should point to original file
public interface IIncludeResolver
{
    TextReader CreateReader(string currentPath, string includePath);
}
using System.IO;

namespace SimpleTextPreprocessor;

public interface IIncludeResolver
{
    bool TryCreateReader(string sourceFileId, string includeParameter, out string? newFileId, out TextReader? reader, IReport? report);
}
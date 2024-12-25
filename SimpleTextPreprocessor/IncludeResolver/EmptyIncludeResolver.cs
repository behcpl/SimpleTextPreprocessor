using System.IO;

namespace SimpleTextPreprocessor.IncludeResolver;

public class EmptyIncludeResolver : IIncludeResolver
{
    public bool TryCreateReader(string sourceFileId, string includeParameter, out string? newFileId, out TextReader? reader, IReport? report)
    {
        newFileId = includeParameter;
        reader = new StringReader(string.Empty);
        return true;
    }
}
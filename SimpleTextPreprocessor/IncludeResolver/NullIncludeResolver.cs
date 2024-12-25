using System.IO;

namespace SimpleTextPreprocessor.IncludeResolver;

// TODO: should this always succeed but with empty data?
public class NullIncludeResolver : IIncludeResolver
{
    public bool TryCreateReader(string sourceFileId, string includeParameter, out string? newFileId, out TextReader? reader, IReport? report)
    {
        report?.Error(sourceFileId, 0, 0, $"{nameof(NullIncludeResolver)} always fail!");
        newFileId = null;
        reader = null;
        return false;
    }
}
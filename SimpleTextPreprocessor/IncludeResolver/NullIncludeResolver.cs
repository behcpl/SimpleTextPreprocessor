using System.IO;

namespace SimpleTextPreprocessor.IncludeResolver;

/// <summary>
/// This implementation will always fail string when resolving <c>#include</c>.
/// Use this if you don't want any support for <c>#include</c> directive.
/// </summary>
public class NullIncludeResolver : IIncludeResolver
{
    public bool TryCreateReader(string sourceFileId, string includeParameter, out string? newFileId, out TextReader? reader, IReport? report)
    {
        report?.Error(sourceFileId, report.CurrentLine, 0, $"{nameof(NullIncludeResolver)} always fail!");
        newFileId = null;
        reader = null;
        return false;
    }
}
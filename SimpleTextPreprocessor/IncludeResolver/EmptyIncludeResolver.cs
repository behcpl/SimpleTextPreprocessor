using System.IO;

namespace SimpleTextPreprocessor.IncludeResolver;

/// <summary>
/// This implementation will always succeed with empty string when resolving <c>#include</c>.
/// Use this if you don't care about including other content.
/// </summary>
public class EmptyIncludeResolver : IIncludeResolver
{
    public bool TryCreateReader(string sourceFileId, string includeParameter, out string? newFileId, out TextReader? reader, IReport? report)
    {
        newFileId = includeParameter;
        reader = new StringReader(string.Empty);
        return true;
    }
}
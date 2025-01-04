using System.Collections.Generic;
using System.IO;

namespace SimpleTextPreprocessor.IncludeResolver;

/// <summary>
/// This implementation will try to resolve <c>#include "key"</c> using in-memory dictionary.
/// </summary>
/// <remarks>
/// <c>Entries</c> must be populated before use.
/// </remarks>
public class InMemoryIncludeResolver : IIncludeResolver
{
    public readonly Dictionary<string, string> Entries;
    
    public InMemoryIncludeResolver()
    {
        Entries = new Dictionary<string, string>();
    }

    public bool TryCreateReader(string sourceFileId, string includeParameter, out string? newFileId, out TextReader? reader, IReport? report)
    {
        string strippedName = includeParameter.TrimStart('"').TrimEnd('"');

        if (Entries.TryGetValue(strippedName, out string? content))
        {
            newFileId = strippedName;
            reader = new StringReader(content);
            return true;
        }
 
        newFileId = null;
        reader = null;
        return false;
    }
}
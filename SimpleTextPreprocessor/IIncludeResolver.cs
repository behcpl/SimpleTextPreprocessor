using System.IO;

namespace SimpleTextPreprocessor;

/// <summary>
/// Implement for custom <c>#include</c> resolution logic.
/// </summary>
public interface IIncludeResolver
{
    /// <summary>
    /// Method invoked every time <c>#include</c> needs to be resolved
    /// </summary>
    /// <param name="sourceFileId">ID of file containing <c>#include</c> directive. Value is either <c>fileId</c> passed to  <c>Process(...)</c> for root object or value returned as <c>newFileId</c> from previous call</param>
    /// <param name="includeParameter">String that goes after <c>#include</c></param>
    /// <param name="newFileId">Unique and deterministic ID of resolved file</param>
    /// <param name="reader">Valid instace if succeeded, <c>null</c> otherwise</param>
    /// <param name="report">(optional) Report instance for collecting errors and exceptions</param>
    /// <returns><c>true</c> if operation succeeded</returns>
    bool TryCreateReader(string sourceFileId, string includeParameter, out string? newFileId, out TextReader? reader, IReport? report);
}
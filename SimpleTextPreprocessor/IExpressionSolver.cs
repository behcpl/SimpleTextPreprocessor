using System.Collections.Generic;

namespace SimpleTextPreprocessor;

/// <summary>
/// Implement if you want to replace <c>DefaultExpressionSolver</c>
/// </summary>
public interface IExpressionSolver
{
    /// <summary>
    /// Method invoked every time <c>#if</c> of <c>#elif</c> must be evaluated to determine if conditional block must be skipped.
    /// </summary>
    /// <param name="symbols">Map of symbol names into valid string values</param>
    /// <param name="expression">String that goes after <c>#if</c> of <c>#elif</c></param>
    /// <param name="result">What expression evaluated into</param>
    /// <param name="report">(optional) Report instance for collecting errors and exceptions</param>
    /// <returns></returns>
    bool TryEvaluate(IReadOnlyDictionary<string, string?> symbols, string expression, out bool result, IReport? report);

    /// <summary>
    /// Method invoked every time <c>#define</c> is encountered inside processed content.
    /// </summary>
    /// <param name="value">String that goes after <c>#define</c>, can contain white chars (but doesn't start or end with one)</param>
    /// <param name="report">(optional) Report instance for collecting errors and exceptions</param>
    /// <returns><c>true</c> if <c>value</c> represents correct string that can be used by this solver</returns>
    bool IsValidValue(string value, IReport? report);
}
using System.Collections.Generic;

namespace SimpleTextPreprocessor;

public interface IExpressionSolver
{
    // TODO: add error handling support
    bool Evaluate(IReadOnlyDictionary<string, string?> symbols, string expression);

    bool IsValidValue(string value);
}
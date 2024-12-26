using System.Collections.Generic;

namespace SimpleTextPreprocessor;

public interface IExpressionSolver
{
    bool TryEvaluate(IReadOnlyDictionary<string, string?> symbols, string expression, out bool result, IReport? report);

    bool IsValidValue(string value, IReport? report);
}
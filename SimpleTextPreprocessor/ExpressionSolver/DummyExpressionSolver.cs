using System.Collections.Generic;

namespace SimpleTextPreprocessor.ExpressionSolver;

// TODO: this will be replaced by DefaultExpressionSolver
// DummyExpressionSolver only handles true/false literals or single symbol name
public class DummyExpressionSolver : IExpressionSolver
{
    public bool Evaluate(IReadOnlyDictionary<string, string?> symbols, string expression)
    {
        string trimmed = expression.TrimStart().TrimEnd();

        if (symbols.ContainsKey(trimmed))
            return true;

        return trimmed == "true";
    }

    public bool TryEvaluate(IReadOnlyDictionary<string, string?> symbols, string expression, out bool result, IReport? report)
    {
        string trimmed = expression.TrimStart().TrimEnd();

        result = symbols.ContainsKey(trimmed) || trimmed == "true";
        return true;
    }

    public bool IsValidValue(string value, IReport? report)
    {
        report?.Error($"{nameof(DummyExpressionSolver)} doesn't support any values!");
        return false;
    }
}
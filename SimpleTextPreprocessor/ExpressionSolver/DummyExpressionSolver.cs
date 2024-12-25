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
}
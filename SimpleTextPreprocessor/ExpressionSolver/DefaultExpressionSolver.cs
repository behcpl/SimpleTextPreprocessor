using System;
using System.Collections.Generic;
using System.Globalization;

namespace SimpleTextPreprocessor.ExpressionSolver;

public class DefaultExpressionSolver : IExpressionSolver
{
    public bool TryEvaluate(IReadOnlyDictionary<string, string?> symbols, string expression, out bool result, IReport? report)
    {
        Parser parser = new(expression, symbols, report);
        return parser.TryEvaluate(out result);
    }

    public bool IsValidValue(string value, IReport? report)
    {
        // bool literals are ok
        if (value == "true" || value == "false")
            return true;

        // valid hex integers
        if (value.StartsWith("0x") || value.StartsWith("0X"))
        {
            return int.TryParse(value.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int _);
        }

        // valid normal integers
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int _);
    }
}
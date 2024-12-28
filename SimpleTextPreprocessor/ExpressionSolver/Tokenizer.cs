using System;
using System.Collections.Generic;
using System.Globalization;

namespace SimpleTextPreprocessor.ExpressionSolver;

internal enum TokenType
{
    INVALID,
    LOGIC_OR,
    LOGIC_AND,
    LOGIC_NOT,
    EQUAL,
    NOT_EQUAL,
    GREATER,
    GREATER_EQUAL,
    LESSER,
    LESSER_EQUAL,
    NEGATE,
    GROUP_START,
    GROUP_END,
    BOOL_VALUE,
    INT_VALUE,
    UNDEFINED_VALUE,
    END
}

internal struct Token
{
    public TokenType Type;
    public int IntValue;
    public bool BoolValue;
}
 
internal static class Tokenizer
{
    public static void SkipWhiteSpace(string input, ref int position)
    {
        while (position < input.Length && char.IsWhiteSpace(input[position]))
        {
            position++;
        }       
    }
    
    public static Token GetToken(string input, IReadOnlyDictionary<string, string?> symbols, ref int position, out string? errorDetails)
    {
        errorDetails = null;

        while (position < input.Length && char.IsWhiteSpace(input[position]))
        {
            position++;
        }

        if (position == input.Length)
        {
            return new Token { Type = TokenType.END };
        }

        char current = input[position++];
        char next = position < input.Length ? input[position] : (char)0;

        switch (current)
        {
            case '(':
                return new Token { Type = TokenType.GROUP_START };

            case ')':
                return new Token { Type = TokenType.GROUP_END };

            case '-':
                return new Token { Type = TokenType.NEGATE };

            case '!':
                if (next != '=')
                    return new Token { Type = TokenType.LOGIC_NOT };

                position++;
                return new Token { Type = TokenType.NOT_EQUAL };

            case '>':
                if (next != '=')
                    return new Token { Type = TokenType.GREATER };

                position++;
                return new Token { Type = TokenType.GREATER_EQUAL };

            case '<':
                if (next != '=')
                    return new Token { Type = TokenType.LESSER };

                position++;
                return new Token { Type = TokenType.LESSER_EQUAL };

            case '=':
                if (next != '=')
                    return new Token { Type = TokenType.INVALID };

                position++;
                return new Token { Type = TokenType.EQUAL };

            case '&':
                if (next != '&')
                    return new Token { Type = TokenType.INVALID };

                position++;
                return new Token { Type = TokenType.LOGIC_AND };

            case '|':
                if (next != '|')
                    return new Token { Type = TokenType.INVALID };

                position++;
                return new Token { Type = TokenType.LOGIC_OR };

            default:
                if (char.IsNumber(current))
                {
                    int nextIndex = position;
                    while (nextIndex < input.Length && (char.IsAsciiLetterOrDigit(input[nextIndex]) || input[nextIndex] == '_'))
                    {
                        nextIndex++;
                    }

                    if (current == '0' && (next == 'x' || next == 'X'))
                    {
                        bool valid = int.TryParse(input.AsSpan(position + 1, nextIndex - position - 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int value);
                        if (!valid)
                            errorDetails = $"Invalid integer '{input.AsSpan(position - 1, nextIndex - position + 1)}'";

                        position = nextIndex;
                        return valid ? new Token { Type = TokenType.INT_VALUE, IntValue = value } : new Token { Type = TokenType.INVALID };
                    }
                    else
                    {
                        bool valid = int.TryParse(input.AsSpan(position - 1, nextIndex - position + 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value);
                        if (!valid)
                            errorDetails = $"Invalid integer '{input.AsSpan(position - 1, nextIndex - position + 1)}'";

                        position = nextIndex;
                        return valid ? new Token { Type = TokenType.INT_VALUE, IntValue = value } : new Token { Type = TokenType.INVALID };
                    }
                }

                if (char.IsAsciiLetter(current) || current == '_')
                {
                    int nextIndex = position;
                    while (nextIndex < input.Length && (char.IsAsciiLetterOrDigit(input[nextIndex]) || input[nextIndex] == '_'))
                    {
                        nextIndex++;
                    }

                    string name = input.Substring(position - 1, nextIndex - position + 1);
                    position = nextIndex;
                    return name switch
                    {
                        "true" => new Token { Type = TokenType.BOOL_VALUE, BoolValue = true },
                        "false" => new Token { Type = TokenType.BOOL_VALUE, BoolValue = false },
                        _ => ResolveSymbol(name, symbols)
                    };
                }

                return new Token { Type = TokenType.INVALID };
        }
    }

    private static Token ResolveSymbol(string name, IReadOnlyDictionary<string, string?> symbols)
    {
        if (symbols.TryGetValue(name, out string? value))
        {
            switch (value)
            {
                case null:
                    return new Token { Type = TokenType.BOOL_VALUE, BoolValue = true };
                case "true":
                    return new Token { Type = TokenType.BOOL_VALUE, BoolValue = true };
                case "false":
                    return new Token { Type = TokenType.BOOL_VALUE, BoolValue = false };
            }

            if (value.StartsWith("0x") || value.StartsWith("0X"))
            {
                return int.TryParse(value.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int hexValue) ? new Token { Type = TokenType.INT_VALUE, IntValue = hexValue } : new Token { Type = TokenType.INVALID };
            }

            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue) ? new Token { Type = TokenType.INT_VALUE, IntValue = intValue } : new Token { Type = TokenType.INVALID };
        }

        return new Token { Type = TokenType.UNDEFINED_VALUE };
    }
}
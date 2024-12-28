using System.Collections.Generic;

namespace SimpleTextPreprocessor.ExpressionSolver;

/*
 *  supported values: bool | int
 *  supported operators (int:binary): >, >=, <, <=, ==, !=
 *    result: bool
 *  supported operators (int:unary): -
 *    result: int
 *  supported operators (bool:binary): &&, ||, ==, !=
 *    result: bool
 *  supported operators (bool:unary): !
 *    result: bool
 *  implicit cast to bool: symbol defined (any value) : true, symbol undefined: false
 * syntax:
 * expression     → logic_or ;
 * logic_or       → logic_and ( "||" logic_and )* ;
 * logic_and      → equality ( "&&" equality )* ;
 * equality       → comparison ( "!=" | "==" ) comparison | comparison ;
 * comparison     → unary ( ">" | ">=" | "<" | "<=" ) unary | unary ;
 * unary          → ( "!" | "-" ) unary | primary ;
 * primary        → "true" | "false" | SYMBOL | INT | "(" + expression + ")" ;
 */

internal struct Parser
{
    private readonly string _input;
    private readonly IReadOnlyDictionary<string, string?> _symbols;
    private readonly IReport? _report;
    private readonly int _columnOffset;

    private int _position;
    private Token _currentToken;
    private bool _gotErrors;

    public Parser(string input, IReadOnlyDictionary<string, string?> symbols, IReport? report)
    {
        _input = input;
        _symbols = symbols;
        _report = report;
        _columnOffset = _report?.CurrentColumn ?? 0;
    }

    public bool TryEvaluate(out bool result)
    {
        Advance();
        if (_currentToken.Type == TokenType.END)
        {
            _report?.Error("Empty expression!");
            result = false;
            return false;
        }

        NodeResult nodeResult = InterpretLogicOr();
        if (nodeResult.Type == NodeType.ERROR)
        {
            result = false;
            return false;
        }

        if (_currentToken.Type != TokenType.END && !_gotErrors)
        {
            if (_report != null)
            {
                _report.CurrentColumn = _columnOffset + _position;
                _report.Error($"Unexpected token '{_currentToken.Type.ToString()}'!");
            }

            result = false;
            return false;
        }

        result = nodeResult.AsBool();
        return !_gotErrors;
    }

    private void Advance()
    {
        do
        {
            Tokenizer.SkipWhiteSpace(_input, ref _position);
            int tokenStart = _position;
            _currentToken = Tokenizer.GetToken(_input, _symbols, ref _position, out string? errorDetails);

            if (_currentToken.Type == TokenType.INVALID)
            {
                _gotErrors = true;
                if (_report != null)
                {
                    _report.CurrentColumn = tokenStart + _columnOffset;
                    _report.Error(errorDetails ?? $"Invalid token `{_input.Substring(tokenStart, _position - tokenStart)}`!");
                }
            }
        } while (_currentToken.Type == TokenType.INVALID);
    }

    private NodeResult InterpretLogicOr()
    {
        NodeResult result = InterpretLogicAnd();
        if (result.Type == NodeType.ERROR)
            return result;

        while (_currentToken.Type == TokenType.LOGIC_OR)
        {
            Advance();

            NodeResult right = InterpretLogicAnd();
            if (right.Type == NodeType.ERROR)
                return right;

            result = new NodeResult { Type = NodeType.BOOL, BoolValue = result.AsBool() || right.AsBool() };
        }

        return result;
    }

    private NodeResult InterpretLogicAnd()
    {
        NodeResult result = InterpretEquality();
        if (result.Type == NodeType.ERROR)
            return result;

        while (_currentToken.Type == TokenType.LOGIC_AND)
        {
            Advance();

            NodeResult right = InterpretEquality();
            if (right.Type == NodeType.ERROR)
                return right;

            result = new NodeResult { Type = NodeType.BOOL, BoolValue = result.AsBool() && right.AsBool() };
        }

        return result;
    }

    private NodeResult InterpretEquality()
    {
        NodeResult result = InterpretCompare();
        if (result.Type == NodeType.ERROR)
            return result;

        TokenType op = _currentToken.Type;
        if (op is TokenType.EQUAL or TokenType.NOT_EQUAL)
        {
            Advance();

            NodeResult right = InterpretCompare();
            if (right.Type == NodeType.ERROR)
                return right;

            if (result.Type == NodeType.UNDEFINED || right.Type == NodeType.UNDEFINED)
            {
                result = new NodeResult { Type = NodeType.BOOL, BoolValue = false };
            }
            else if (result.Type != right.Type)
            {
                result = new NodeResult { Type = NodeType.ERROR };
            }
            else if (result.Type == NodeType.INT)
            {
                result = op == TokenType.EQUAL
                    ? new NodeResult { Type = NodeType.BOOL, BoolValue = result.IntValue == right.IntValue }
                    : new NodeResult { Type = NodeType.BOOL, BoolValue = result.IntValue != right.IntValue };
            }
            else
            {
                result = op == TokenType.EQUAL
                    ? new NodeResult { Type = NodeType.BOOL, BoolValue = result.BoolValue == right.BoolValue }
                    : new NodeResult { Type = NodeType.BOOL, BoolValue = result.BoolValue != right.BoolValue };
            }
        }

        return result;
    }

    private NodeResult InterpretCompare()
    {
        NodeResult result = InterpretUnary();
        if (result.Type == NodeType.ERROR)
            return result;

        TokenType op = _currentToken.Type;
        if (op is TokenType.GREATER or TokenType.GREATER_EQUAL or TokenType.LESSER or TokenType.LESSER_EQUAL)
        {
            Advance();

            NodeResult right = InterpretUnary();
            if (right.Type == NodeType.ERROR)
                return right;

            if (result.Type == NodeType.UNDEFINED || right.Type == NodeType.UNDEFINED)
            {
                result = new NodeResult { Type = NodeType.BOOL, BoolValue = false };
            }
            else if (result.Type != NodeType.INT || right.Type != NodeType.INT)
            {
                result = new NodeResult { Type = NodeType.ERROR };
            }
            else
            {
                result = op switch
                {
                    TokenType.GREATER => new NodeResult { Type = NodeType.BOOL, BoolValue = result.IntValue > right.IntValue },
                    TokenType.GREATER_EQUAL => new NodeResult { Type = NodeType.BOOL, BoolValue = result.IntValue >= right.IntValue },
                    TokenType.LESSER => new NodeResult { Type = NodeType.BOOL, BoolValue = result.IntValue < right.IntValue },
                    TokenType.LESSER_EQUAL => new NodeResult { Type = NodeType.BOOL, BoolValue = result.IntValue <= right.IntValue },
                    // ReSharper disable once UnreachableSwitchArmDueToIntegerAnalysis
                    _ => result
                };
            }
        }

        return result;
    }

    private NodeResult InterpretUnary()
    {
        switch (_currentToken.Type)
        {
            case TokenType.LOGIC_NOT:
            {
                Advance();
                NodeResult result = InterpretUnary();
                if (result.Type == NodeType.ERROR)
                    return result;

                return new NodeResult { Type = NodeType.BOOL, BoolValue = !result.AsBool() };
            }
            case TokenType.NEGATE:
            {
                Advance();
                NodeResult result = InterpretUnary();
                if (result.Type == NodeType.UNDEFINED)
                    return result;
                if (result.Type != NodeType.INT)
                    return new NodeResult { Type = NodeType.ERROR };

                return new NodeResult { Type = NodeType.INT, IntValue = -result.IntValue };
            }
            default:
                return InterpretPrimary();
        }
    }

    private NodeResult InterpretPrimary()
    {
        Token token = _currentToken;
        Advance();

        switch (token.Type)
        {
            case TokenType.BOOL_VALUE:
                return new NodeResult { Type = NodeType.BOOL, BoolValue = token.BoolValue };
            case TokenType.INT_VALUE:
                return new NodeResult { Type = NodeType.INT, IntValue = token.IntValue };
            case TokenType.UNDEFINED_VALUE:
                return new NodeResult { Type = NodeType.UNDEFINED };
            case TokenType.GROUP_START:
            {
                NodeResult result = InterpretLogicOr();

                if (_currentToken.Type == TokenType.GROUP_END)
                {
                    Advance();
                }
                else
                {
                    if (_report != null)
                    {
                        _report.CurrentColumn = _columnOffset + _position;
                        _report.Error("Missing `)` token!");
                    }

                    _gotErrors = true;
                }

                return result;
            }
            default:
                _gotErrors = true;
                return new NodeResult { Type = NodeType.ERROR };
        }
    }

    private enum NodeType
    {
        ERROR,
        BOOL,
        INT,
        UNDEFINED
    }

    private class NodeResult
    {
        public NodeType Type;
        public int IntValue;
        public bool BoolValue;

        public bool AsBool()
        {
            return Type switch
            {
                NodeType.INT => true,
                NodeType.BOOL => BoolValue,
                NodeType.UNDEFINED => false,
                _ => false
            };
        }
    }
}
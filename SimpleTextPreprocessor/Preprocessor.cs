using System.Collections.Generic;
using System.IO;

namespace SimpleTextPreprocessor;

public class Preprocessor
{
    private const string _DIRECTIVE_INCLUDE = "include";
    private const string _DIRECTIVE_DEFINE = "define";
    private const string _DIRECTIVE_UNDEFINE = "undef";
    private const string _DIRECTIVE_IF = "if";
    private const string _DIRECTIVE_END = "endif";
    private const string _DIRECTIVE_ELSE = "else";
    private const string _DIRECTIVE_ELSE_IF = "elif";

    private readonly IIncludeResolver _includeResolver;
    private readonly IExpressionSolver _expressionSolver;

    private readonly HashSet<string> _ignored;
    private readonly Dictionary<string, string?> _symbols;
    private readonly char _directiveChar;

    public Preprocessor(IIncludeResolver includeResolver, IExpressionSolver expressionSolver, PreprocessorOptions options)
    {
        _includeResolver = includeResolver;
        _expressionSolver = expressionSolver;
        _directiveChar = options.SpecialChar;
        _ignored = new HashSet<string>();
        _symbols = new Dictionary<string, string?>();
    }

    public Preprocessor() : this(new NullIncludeResolver(), new DummyExpressionSolver(), PreprocessorOptions.Default) { }

    public void Ignore(string directive)
    {
        _ignored.Add(directive);
    }

    public void Define(string symbol, string? value)
    {
        _symbols[symbol] = value;
    }

    public void Undefine(string symbol)
    {
        _symbols.Remove(symbol);
    }

    public void Process(TextReader reader, TextWriter writer)
    {
        Dictionary<string, string?> symbols = new(_symbols);

        ProcessSection(symbols, reader, writer);
    }

    private void ProcessSection(Dictionary<string, string?> symbols, TextReader reader, TextWriter writer)
    {
        SectionState state = new();

        string? line = reader.ReadLine();
        while (line != null)
        {
            if (HandleLine(ref state, symbols, line, writer))
            {
                writer.WriteLine(line);
            }

            line = reader.ReadLine();
        }
    }

    private bool HandleLine(ref SectionState state, Dictionary<string, string?> symbols, string line, TextWriter writer)
    {
        if (line.Length <= 1 || line[0] != _directiveChar)
            return state.SkipLevel == 0;

        string directive = GetDirective(line);
        if (_ignored.Contains(directive))
        {
            return true;
        }

        switch (directive)
        {
            case _DIRECTIVE_IF:
            {
                state.ScopeLevel++;

                if (state.SkipLevel == 0)
                {
                    bool include = _expressionSolver.Evaluate(_symbols, line.Substring(_DIRECTIVE_IF.Length + 1));
                    if (!include)
                    {
                        state.SkipLevel = state.ScopeLevel;
                    }
                }

                break;
            }
            case _DIRECTIVE_END:
            {
                if (state.SkipLevel == state.ScopeLevel)
                {
                    state.SkipLevel = 0;
                }

                state.ScopeLevel--;
                break;
            }
            case _DIRECTIVE_INCLUDE:
            {
                HandleInclude(line.Substring(_DIRECTIVE_INCLUDE.Length + 1), symbols, writer);
                break;
            }
            case _DIRECTIVE_DEFINE:
            {
                // TODO: parse parameters: symbol and (optional) value
                // symbols[symbol] = value;
                break;
            }
            case _DIRECTIVE_UNDEFINE:
            {
                // TODO: parse one parameter: symbol
                // symbols.Remove(symbol);
                break;
            }
        }

        // by default strip all non-ignored directives
        return false;
    }

    private void HandleInclude(string parameter, Dictionary<string, string?> symbols, TextWriter writer)
    {
        TextReader reader = _includeResolver.CreateReader("./", parameter.TrimStart().TrimStart('"').TrimEnd().TrimEnd('"'));
        ProcessSection(symbols, reader, writer);
    }

    private string GetDirective(string line)
    {
        int lastIndex = 1;
        for (int i = 1; i < line.Length; i++)
        {
            if (line[i] == ' ' || line[i] == '\t')
                break;

            lastIndex = i;
        }

        return line.Substring(1, lastIndex); // length is lastIndex+1, but we -1 because of stripping #
    }

    private struct SectionState
    {
        public int ScopeLevel;
        public int SkipLevel;
    }
}
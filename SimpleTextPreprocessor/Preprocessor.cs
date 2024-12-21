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

    private readonly HashSet<string> _ignored;
    private readonly Dictionary<string, string?> _symbols;
    private readonly char _directiveChar;

    private int _blockScope;
    private int _blockScopeIgnored;

    public Preprocessor(IIncludeResolver includeResolver, PreprocessorOptions options)
    {
        _includeResolver = includeResolver;
        _directiveChar = options.SpecialChar;
        _ignored = new HashSet<string>();
        _symbols = new Dictionary<string, string?>();
    }

    public Preprocessor() : this(new NullIncludeResolver(), PreprocessorOptions.Default) { }

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
        string? line = reader.ReadLine();

        _blockScopeIgnored = -1;

        while (line != null)
        {
            if (HandleLine(line, writer))
            {
                writer.WriteLine(line);
            }

            line = reader.ReadLine();
        }
    }

    private bool HandleLine(string line, TextWriter writer)
    {
        if (line.Length > 1 && line[0] == _directiveChar)
        {
            string directive = GetDirective(line);
            if (_ignored.Contains(directive))
            {
                return true;
            }

            if (directive == _DIRECTIVE_IF)
            {
                _blockScope++;

                if (_blockScopeIgnored < 0)
                {
                    bool include = EvalExpression(line.Substring(_DIRECTIVE_IF.Length + 1));
                    if (!include)
                    {
                        _blockScopeIgnored = _blockScope;
                    }
                }
            }

            if (directive == _DIRECTIVE_END)
            {
                if (_blockScope == _blockScopeIgnored)
                {
                    _blockScopeIgnored = -1;
                }

                _blockScope--;
            }

            if (directive == _DIRECTIVE_INCLUDE)
            {
                HandleInclude(line.Substring(_DIRECTIVE_INCLUDE.Length + 1), writer);
            }

            // by default strip all non-ignored directives
            return false;
        }

        if (_blockScopeIgnored >= 0)
            return false;

        return true;
    }

    private void HandleInclude(string parameter, TextWriter writer)
    {
        TextReader reader = _includeResolver.CreateReader("./", parameter.TrimStart().TrimStart('"').TrimEnd().TrimEnd('"'));
        
        // recursive inclusion, will break if/else groups
        Process(reader, writer);
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

    private bool EvalExpression(string expression)
    {
        // TODO: placeholder expression
        return expression.TrimStart().TrimEnd() == "true";
    }
}
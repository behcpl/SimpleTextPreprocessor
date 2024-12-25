using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using SimpleTextPreprocessor.ExpressionSolver;
using SimpleTextPreprocessor.IncludeResolver;

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
    private readonly bool _breakOnFirstError;

    public Preprocessor(IIncludeResolver includeResolver, IExpressionSolver expressionSolver, PreprocessorOptions options)
    {
        _includeResolver = includeResolver;
        _expressionSolver = expressionSolver;
        _directiveChar = options.DirectiveChar;
        _breakOnFirstError = options.BreakOnFirstError;
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

    public bool Process(TextReader reader, TextWriter writer, IReport? report = null)
    {
        return Process(".", reader, writer, report);
    }

    public bool Process(string fileId, TextReader reader, TextWriter writer, IReport? report = null)
    {
        Dictionary<string, string?> symbols = new(_symbols);
        List<string> fileIds =
        [
            fileId
        ];

        return ProcessSection(fileIds, symbols, reader, writer, report);
    }

    private bool ProcessSection(List<string> fileIds, Dictionary<string, string?> symbols, TextReader reader, TextWriter writer, IReport? report)
    {
        List<BlockState> sectionState = [];

        bool valid = true;
        int lineNumber = 0;
        string? line = reader.ReadLine();
        while (line != null)
        {
            bool success = ProcessLine(out bool outputLine, fileIds, sectionState, symbols, line, lineNumber, writer, report);
            if (outputLine)
                writer.WriteLine(line);

            if (!success)
            {
                valid = false;
                if (_breakOnFirstError)
                    return false;
            }

            lineNumber++;
            line = reader.ReadLine();
        }

        if (sectionState.Count > 0)
        {
            report?.Error(fileIds[^1], lineNumber, 0, "Unexpected end of file!");
            return false;
        }

        return valid;
    }

    private bool ProcessLine(out bool outputLine, List<string> fileIds, List<BlockState> sectionState, Dictionary<string, string?> symbols, string line, int lineNumber, TextWriter writer, IReport? report)
    {
        // optimization / simplicity: directive char must be first, no white chars allowed before
        if (line.Length <= 1 || line[0] != _directiveChar)
        {
            outputLine = sectionState.Count == 0 || !sectionState[^1].SkipContent;
            return true;
        }

        // TODO: this could fail in some cases
        string directive = GetDirective(line);
        if (_ignored.Contains(directive))
        {
            outputLine = true;
            return true;
        }

        string fileId = fileIds[^1];
        outputLine = false;
        switch (directive)
        {
            case _DIRECTIVE_IF:
            {
                bool parentSkip = sectionState.Count > 0 && sectionState[^1].SkipContent;
                bool skipContent = parentSkip || !_expressionSolver.Evaluate(_symbols, line.Substring(_DIRECTIVE_IF.Length + 1));

                sectionState.Add(new BlockState
                {
                    SkipContent = skipContent,
                    CanHaveElif = true,
                    CanHaveElse = true,
                    ConditionFulfilled = !skipContent
                });

                return true;
            }
            case _DIRECTIVE_ELSE_IF:
            {
                if (sectionState.Count == 0)
                {
                    report?.Error(fileId, lineNumber, 0, $"Unexpected directive `{_directiveChar}{_DIRECTIVE_ELSE_IF}` found!");
                    return false;
                }

                if (!sectionState[^1].CanHaveElif)
                {
                    report?.Error(fileId, lineNumber, 0, $"Can't have `{_directiveChar}{_DIRECTIVE_ELSE_IF}` after `{_directiveChar}{_DIRECTIVE_ELSE}` directives!");
                    return false;
                }

                BlockState state = sectionState[^1];

                bool parentSkip = sectionState.Count > 1 && sectionState[^2].SkipContent;
                bool skipContent = parentSkip || state.ConditionFulfilled || !_expressionSolver.Evaluate(_symbols, line.Substring(_DIRECTIVE_ELSE_IF.Length + 1));

                state.SkipContent = skipContent;
                state.ConditionFulfilled = state.ConditionFulfilled || !skipContent;
                sectionState[^1] = state;

                return true;
            }
            case _DIRECTIVE_ELSE:
            {
                if (sectionState.Count == 0)
                {
                    report?.Error(fileId, lineNumber, 0, $"Unexpected directive `{_directiveChar}{_DIRECTIVE_ELSE}` found!");
                    return false;
                }

                if (!sectionState[^1].CanHaveElse)
                {
                    report?.Error(fileId, lineNumber, 0, $"Can't have multiple `{_directiveChar}{_DIRECTIVE_ELSE}` directives!");
                    return false;
                }

                bool parentSkip = sectionState.Count > 1 && sectionState[^2].SkipContent;

                BlockState state = sectionState[^1];
                state.SkipContent = state.ConditionFulfilled || parentSkip;
                state.CanHaveElse = false;
                state.CanHaveElif = false;
                sectionState[^1] = state;

                return true;
            }
            case _DIRECTIVE_END:
            {
                if (sectionState.Count == 0)
                {
                    report?.Error(fileId, lineNumber, 0, $"Unexpected directive `{_directiveChar}{_DIRECTIVE_END}` found!");
                    return false;
                }

                sectionState.RemoveAt(sectionState.Count - 1);
                return true;
            }
            case _DIRECTIVE_INCLUDE:
            {
                return HandleInclude(fileIds, symbols, line, lineNumber, writer, report);
            }
            case _DIRECTIVE_DEFINE:
            {
                // TODO: parse parameters: symbol and (optional) value
                // symbols[symbol] = value;
                return true;
            }
            case _DIRECTIVE_UNDEFINE:
            {
                // TODO: parse one parameter: symbol
                // symbols.Remove(symbol);
                return true;
            }
            default:
            {
                // TODO: or emit error, depends on options
                return true;
            }
        }
    }

    private bool HandleInclude(List<string> fileIds, Dictionary<string, string?> symbols,  string line, int lineNumber, TextWriter writer, IReport? report)
    {
        string parameter = line.Substring(_DIRECTIVE_INCLUDE.Length + 1);
        
        // TODO: remap report line/column here
        if (!_includeResolver.TryCreateReader(fileIds[^1], parameter, out string? newFileId, out TextReader? reader, report))
            return false;

        Debug.Assert(newFileId != null);
        Debug.Assert(reader != null);
        
        using TextReader sectionReader = reader;
        if (fileIds.IndexOf(newFileId) >= 0)
        {
            report?.Error(fileIds[^1], lineNumber, 0, $"Recursive loop detected when including '{newFileId}'!");
            return false;
        }
        
        fileIds.Add(newFileId);
        return ProcessSection(fileIds, symbols, sectionReader, writer, report);
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

    private struct BlockState
    {
        public bool ConditionFulfilled;
        public bool CanHaveElse;
        public bool CanHaveElif;
        public bool SkipContent;
    }
}
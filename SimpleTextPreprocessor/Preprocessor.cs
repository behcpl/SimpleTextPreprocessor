using System;
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
        // using invalid path characters here to guard against undefined behaviour when using FileSystemIncludeResolver
        return Process("*root*", reader, writer, report);
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

        if (!FindDirective(line, out int dirStart, out int dirEnd))
        {
            report?.Error(fileIds[^1], lineNumber, 1, $"No directive found after `{_directiveChar}` character!");
            outputLine = false;
            return false;
        }

        ReadOnlySpan<char> directive = line.AsSpan(dirStart, dirEnd - dirStart);
        // TODO: convert _ignored to use ReadOnlyMemory and add custom comparer
        if (_ignored.Contains(directive.ToString()))
        {
            outputLine = true;
            return true;
        }

        outputLine = false;
        switch (directive)
        {
            case _DIRECTIVE_IF:
            {
                bool parentSkip = sectionState.Count > 0 && sectionState[^1].SkipContent;
                bool validExpression = FindExpression(line, dirEnd, out int expStart, out int expEnd);
                bool skipContent = parentSkip || !validExpression || !_expressionSolver.Evaluate(_symbols, line.Substring(expStart, expEnd - expStart));

                if (!validExpression)
                {
                    report?.Error(fileIds[^1], lineNumber, line.Length, $"No expression found after `{_directiveChar}{_DIRECTIVE_IF}` directive!");
                }

                sectionState.Add(new BlockState
                {
                    SkipContent = skipContent,
                    CanHaveElif = true,
                    CanHaveElse = true,
                    ConditionFulfilled = !skipContent
                });

                return validExpression;
            }
            case _DIRECTIVE_ELSE_IF:
            {
                if (sectionState.Count == 0)
                {
                    report?.Error(fileIds[^1], lineNumber, 0, $"Unexpected directive `{_directiveChar}{_DIRECTIVE_ELSE_IF}` found!");
                    return false;
                }

                if (!sectionState[^1].CanHaveElif)
                {
                    report?.Error(fileIds[^1], lineNumber, 0, $"Can't have `{_directiveChar}{_DIRECTIVE_ELSE_IF}` after `{_directiveChar}{_DIRECTIVE_ELSE}` directive!");
                    return false;
                }

                BlockState state = sectionState[^1];

                bool parentSkip = sectionState.Count > 1 && sectionState[^2].SkipContent;
                bool validExpression = FindExpression(line, dirEnd, out int expStart, out int expEnd);
                bool skipContent = parentSkip || state.ConditionFulfilled || !validExpression || !_expressionSolver.Evaluate(_symbols, line.Substring(expStart, expEnd - expStart));

                if (!validExpression)
                {
                    report?.Error(fileIds[^1], lineNumber, line.Length, $"No expression found after `{_directiveChar}{_DIRECTIVE_IF}` directive!");
                }

                state.SkipContent = skipContent;
                state.ConditionFulfilled = state.ConditionFulfilled || !skipContent;
                sectionState[^1] = state;

                return validExpression;
            }
            case _DIRECTIVE_ELSE:
            {
                if (sectionState.Count == 0)
                {
                    report?.Error(fileIds[^1], lineNumber, 0, $"Unexpected directive `{_directiveChar}{_DIRECTIVE_ELSE}` found!");
                    return false;
                }

                if (!sectionState[^1].CanHaveElse)
                {
                    report?.Error(fileIds[^1], lineNumber, 0, $"Can't have multiple `{_directiveChar}{_DIRECTIVE_ELSE}` directives!");
                    return false;
                }
                
                bool valid = true;
                if (!CheckForEmpty(line, dirEnd, out int nonWhiteChar))
                {
                    report?.Error(fileIds[^1], lineNumber, nonWhiteChar, $"Unexpected character after `{_directiveChar}{_DIRECTIVE_ELSE}`!");
                    valid = false;
                }

                bool parentSkip = sectionState.Count > 1 && sectionState[^2].SkipContent;

                BlockState state = sectionState[^1];
                state.SkipContent = state.ConditionFulfilled || parentSkip;
                state.CanHaveElse = false;
                state.CanHaveElif = false;
                sectionState[^1] = state;

                return valid;
            }
            case _DIRECTIVE_END:
            {
                if (sectionState.Count == 0)
                {
                    report?.Error(fileIds[^1], lineNumber, 0, $"Unexpected directive `{_directiveChar}{_DIRECTIVE_END}` found!");
                    return false;
                }

                bool valid = true;
                if (!CheckForEmpty(line, dirEnd, out int nonWhiteChar))
                {
                    report?.Error(fileIds[^1], lineNumber, nonWhiteChar, $"Unexpected character after `{_directiveChar}{_DIRECTIVE_END}`!");
                    valid = false;
                }
  
                sectionState.RemoveAt(sectionState.Count - 1);
                return valid;
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

    private bool HandleInclude(List<string> fileIds, Dictionary<string, string?> symbols, string line, int lineNumber, TextWriter writer, IReport? report)
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
        bool valid = ProcessSection(fileIds, symbols, sectionReader, writer, report);
        fileIds.RemoveAt(fileIds.Count - 1);
        return valid;
    }

    private static bool FindDirective(string line, out int start, out int end)
    {
        start = 1;
        end = 1;
        for (int i = 1; i < line.Length; i++)
        {
            if (char.IsWhiteSpace(line[i]))
                break;

            end = i + 1;
        }

        return end > start;
    }

    private static bool FindNonWhiteSeparatedSymbol(string line, int lineOffset, out int start, out int end)
    {
        start = int.MaxValue;

        // skip white chars
        for (int i = lineOffset; i < line.Length; i++)
        {
            if (char.IsWhiteSpace(line[i]))
                continue;

            start = i;
            break;
        }

        end = start;
        for (int i = start; i < line.Length; i++)
        {
            if (char.IsWhiteSpace(line[i]))
                break;

            end = i + 1;
        }

        return end > start;
    }

    private static bool FindExpression(string line, int lineOffset, out int start, out int end)
    {
        start = int.MaxValue;

        // trim white chars from lineOffset
        for (int i = lineOffset; i < line.Length; i++)
        {
            if (char.IsWhiteSpace(line[i]))
                continue;

            start = i;
            break;
        }

        end = 0;

        // trim white chars from line.Length
        for (int i = line.Length - 1; i >= lineOffset; i--)
        {
            if (char.IsWhiteSpace(line[i]))
                continue;

            end = i + 1;
            break;
        }

        return end > start;
    }

    private static bool CheckForEmpty(string line, int lineOffset, out int nonWhiteChar)
    {
        for (int i = lineOffset; i < line.Length; i++)
        {
            if (char.IsWhiteSpace(line[i]))
                continue;
            
            nonWhiteChar = i;
            return false;
        }

        nonWhiteChar = -1;
        return true;
    }

    private struct BlockState
    {
        public bool ConditionFulfilled;
        public bool CanHaveElse;
        public bool CanHaveElif;
        public bool SkipContent;
    }
}
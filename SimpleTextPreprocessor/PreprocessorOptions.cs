namespace SimpleTextPreprocessor;

public struct PreprocessorOptions
{
    public char DirectiveChar;
    public bool BreakOnFirstError;
    public bool ErrorOnUnknownDirective;

    public static PreprocessorOptions Default =>
        new()
        {
            DirectiveChar = '#'
        };
}
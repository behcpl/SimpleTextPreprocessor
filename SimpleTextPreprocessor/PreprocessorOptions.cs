namespace SimpleTextPreprocessor;

public struct PreprocessorOptions
{
    /// <summary>
    /// Character that is used to distinguish directive from rest of the content.
    /// </summary>
    public char DirectiveChar;
    /// <summary>
    /// If <c>true</c>, processing will stop when the first problem is encountered.
    /// </summary>
    public bool BreakOnFirstError;
    /// <summary>
    /// If <c>true</c>, unknown directives will be treated as errors and fail processing.
    /// </summary>
    public bool ErrorOnUnknownDirective;
    /// <summary>
    /// If <c>true</c>, unknown directives will be treated as errors and fail processing.
    /// </summary>
    public bool ErrorOnMissingDirective;

    public static PreprocessorOptions Default =>
        new()
        {
            DirectiveChar = '#'
        };
}
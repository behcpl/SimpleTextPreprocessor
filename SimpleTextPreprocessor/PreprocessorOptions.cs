namespace SimpleTextPreprocessor;

public struct PreprocessorOptions
{
    public char SpecialChar; // TODO: rename to DirectiveChar?
    
    // TODO: other potential options:
    // directives: if, else, elif, endif, ...
    // bool BreakOnFirstError - don't try to recover and process whole file, just return on first failure
    // bool ErrorOnUnknownDirective - by default unknown directives are stripped
    

    public static PreprocessorOptions Default =>
        new()
        {
            SpecialChar = '#'
        };
}
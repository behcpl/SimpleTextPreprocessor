namespace SimpleTextPreprocessor;

public struct PreprocessorOptions
{
    public char SpecialChar;

    public static PreprocessorOptions Default =>
        new()
        {
            SpecialChar = '#'
        };
}
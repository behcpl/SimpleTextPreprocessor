using System;
using System.IO;
using System.Text;
using SimpleTextPreprocessor;

namespace Sandbox;

class Program
{
    static void Main(string[] args)
    {
        Preprocessor preprocessor = new Preprocessor(new IncludeResolver(), new DummyExpressionSolver(), PreprocessorOptions.Default);

        using TextReader source = new StreamReader("./ExampleData/example.txt");

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);
        result.NewLine = "\r\n";

        preprocessor.Process(source, result);

        Console.WriteLine(sb.ToString());
    }
}

public class IncludeResolver : IIncludeResolver
{
    public TextReader CreateReader(string currentPath, string includePath)
    {
        Console.WriteLine($"REQUEST: {includePath} FROM: {currentPath}");
        return new StreamReader(Path.Combine(currentPath, includePath));
    }
}
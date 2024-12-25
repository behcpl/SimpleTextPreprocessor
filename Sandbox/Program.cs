using System;
using System.IO;
using System.Text;
using SimpleTextPreprocessor;
using SimpleTextPreprocessor.ExpressionSolver;
using SimpleTextPreprocessor.IncludeResolver;

namespace Sandbox;

class Program
{
    static void Main(string[] args)
    {
        FileSystemIncludeResolver includeResolver = new FileSystemIncludeResolver();
        Preprocessor preprocessor = new Preprocessor(includeResolver, new DummyExpressionSolver(), PreprocessorOptions.Default);

        const string sourceName = "./ExampleData/example.txt";
        using TextReader source = new StreamReader(sourceName);

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);
        result.NewLine = "\r\n";

        preprocessor.Process(includeResolver.GetFileId(sourceName), source, result);

        Console.WriteLine(sb.ToString());
    }
}
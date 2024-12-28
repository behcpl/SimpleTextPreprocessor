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
        Preprocessor preprocessor = new Preprocessor(includeResolver, new DefaultExpressionSolver(), PreprocessorOptions.Default);

        const string sourceName = "./ExampleData/example.txt";
        using TextReader source = new StreamReader(sourceName);

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);
        result.NewLine = "\r\n";

        preprocessor.Process(includeResolver.GetFileId(sourceName), source, result);

        Console.WriteLine(sb.ToString());
    }

    private static void ExampleBasic(string sourceText)
    {
        Preprocessor preprocessor = new();

        using TextReader source = new StringReader(sourceText);

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);

        bool success = preprocessor.Process(source, result);
    } 
    
    private static void ExampleString(string sourceText)
    {
        InMemoryIncludeResolver includeResolver = new();
        includeResolver.Entries.Add("file1", "some content...");
        includeResolver.Entries.Add("file2", "some content...");

        Preprocessor preprocessor = new (includeResolver, new DefaultExpressionSolver(), PreprocessorOptions.Default);
        preprocessor.AddToIgnored("version");
        preprocessor.AddSymbol("DEF1", "123");
        preprocessor.AddSymbol("DEF2", "false");
        preprocessor.AddSymbol("DEF3"); // no value, defaults to true
        
        using TextReader source = new StringReader(sourceText);

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);

        bool success = preprocessor.Process(source, result);
    }

    private static void ExampleFile(string sourceFilePath, string outputFilePath)
    {
        FileSystemIncludeResolver includeResolver = new();
         
        Preprocessor preprocessor = new (includeResolver, new DefaultExpressionSolver(), PreprocessorOptions.Default);

        ReportList report = new();
        LineNumberMapper lineNumberMapper = new();
        
        using TextReader source = new StreamReader(sourceFilePath);
        using TextWriter output = new StreamWriter(outputFilePath);
     
        string rootFileId = includeResolver.GetFileId(sourceFilePath);
        bool success = preprocessor.Process(rootFileId, source, output, report, lineNumberMapper);

        if (!success)
        {
            foreach (ReportList.Entry entry in report.Entries)
                Console.WriteLine($"{entry.FileId}({entry.Line},{entry.Column}): {entry.Message}");
        }
        
        // after using output file by external tools and get error at line 123 it is possible to point to correct file/line number
        (string sourceFileId, int sourceLineNumber) = lineNumberMapper.GetSource(123);
    }
}
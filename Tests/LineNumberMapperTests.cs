using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using SimpleTextPreprocessor;
using SimpleTextPreprocessor.ExpressionSolver;
using SimpleTextPreprocessor.IncludeResolver;

namespace Tests;

public class LineNumberMapperTests
{
    [Test]
    public void Mapping_should_consider_conditional_blocks()
    {
        Preprocessor preprocessor = new Preprocessor(new NullIncludeResolver(), new DefaultExpressionSolver(), PreprocessorOptions.Default);
        preprocessor.AddToIgnored("version");

        const string sourceString =
            """
            #version 1.2.3
            R 1
            #if false
            R 3
            #else
            R 5
            #endif
            R 7
            #define SOMETHING
            R 9
            """;

        using TextReader source = new StringReader(sourceString);

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);
        result.NewLine = "\r\n";

        LineNumberMapper mapper = new LineNumberMapper();
        bool ret = preprocessor.Process("R", source, result, null, mapper);

        Assert.That(ret, Is.True);
        AssertResultSource(sb.ToString(), mapper);
    }

    [Test]
    public void Mapping_should_consider_included_section()
    {
        InMemoryIncludeResolver includeResolver = new InMemoryIncludeResolver();
        includeResolver.Entries["a"] =
            """
            a 0
            a 1
            #include b
            a 3
            a 4
            """;
        includeResolver.Entries["b"] =
            """
            b 0
            b 1
            """;

        Preprocessor preprocessor = new Preprocessor(includeResolver, new DefaultExpressionSolver(), PreprocessorOptions.Default);

        const string sourceString =
            """
            R 0
            R 1
            #include a
            R 3
            R 4
            #include b
            R 6
            """;

        using TextReader source = new StringReader(sourceString);

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);
        result.NewLine = "\r\n";

        LineNumberMapper mapper = new LineNumberMapper();
        bool ret = preprocessor.Process("R", source, result, null, mapper);

        Assert.That(ret, Is.True);
        AssertResultSource(sb.ToString(), mapper);
    }

    private static void AssertResultSource(string result, LineNumberMapper mapper)
    {
        using TextReader resultRead = new StringReader(result);
        List<string> resultList = [];
        while (true)
        {
            string? line = resultRead.ReadLine();
            if (line == null)
                break;

            resultList.Add(line);
        }

        Assert.That(mapper.EntriesCount, Is.EqualTo(resultList.Count));

        (string id0, int num0) = mapper.GetSource(0);

        Assert.That(id0, Is.EqualTo("R"));
        Assert.That(num0, Is.EqualTo(0));

        // skip 0, it might not follow the pattern
        for (int i = 1; i < resultList.Count; i++)
        {
            (string id1, int num1) = mapper.GetSource(i);

            string[] entries = resultList[i].Split(' ');

            Assert.That(id1, Is.EqualTo(entries[0]));
            Assert.That(num1, Is.EqualTo(int.Parse(entries[1], NumberStyles.Integer, NumberFormatInfo.InvariantInfo)));
        }
    }
}
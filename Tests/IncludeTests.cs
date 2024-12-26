using System.IO;
using System.Text;
using SimpleTextPreprocessor;
using SimpleTextPreprocessor.ExpressionSolver;
using SimpleTextPreprocessor.IncludeResolver;

namespace Tests;

public class IncludeTests
{
    private const string _SOURCE =
        """
        first line
        second line
        #include "input"
        third line
        fourth line
        """;

    private const string _INCLUDE =
        """
        include 1
        include 2
        """;

    private const string _EXPECTED =
        """
        first line
        second line
        include 1
        include 2
        third line
        fourth line

        """;

    private const string _EXPECTED_SKIPPED =
        """
        first line
        second line
        third line
        fourth line

        """;

    [Test]
    public void Include_replaced_with_text()
    {
        InMemoryIncludeResolver resolver = new InMemoryIncludeResolver();
        resolver.Entries.Add("input", _INCLUDE);
        Preprocessor preprocessor = new Preprocessor(resolver, new DummyExpressionSolver(), PreprocessorOptions.Default);

        using TextReader source = new StringReader(_SOURCE);

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);
        result.NewLine = "\r\n";

        bool ret = preprocessor.Process(source, result);

        Assert.That(ret, Is.True);
        Assert.That(sb.ToString(), Is.EqualTo(_EXPECTED));
    }

    [Test]
    public void Include_fails_with_null_resolver()
    {
        Preprocessor preprocessor = new Preprocessor(new NullIncludeResolver(), new DummyExpressionSolver(), PreprocessorOptions.Default);

        using TextReader source = new StringReader(_SOURCE);

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);
        result.NewLine = "\r\n";


        TestReport report = new TestReport();
        bool ret = preprocessor.Process(source, result, report);

        Assert.That(ret, Is.False);
        Assert.That(report.Entries, Has.Count.EqualTo(1));
    }

    [Test]
    public void Include_skips_with_empty_resolver()
    {
        Preprocessor preprocessor = new Preprocessor(new EmptyIncludeResolver(), new DummyExpressionSolver(), PreprocessorOptions.Default);

        using TextReader source = new StringReader(_SOURCE);

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);
        result.NewLine = "\r\n";

        TestReport report = new TestReport();
        bool ret = preprocessor.Process(source, result, report);

        Assert.That(ret, Is.True);
        Assert.That(sb.ToString(), Is.EqualTo(_EXPECTED_SKIPPED));
    }

    [Test]
    public void Include_fails_when_loop_is_detected()
    {
        InMemoryIncludeResolver resolver = new InMemoryIncludeResolver();
        resolver.Entries.Add("a", """
                                  #include "b"
                                  """);
        resolver.Entries.Add("b", """
                                  #include "c"
                                  """);
        resolver.Entries.Add("c", """
                                  #include "a"
                                  """);

        Preprocessor preprocessor = new Preprocessor(resolver, new DummyExpressionSolver(), PreprocessorOptions.Default);

        using TextReader source = new StringReader("""
                                                   #include "a"
                                                   """);

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);
        result.NewLine = "\r\n";

        TestReport report = new TestReport();
        bool ret = preprocessor.Process(source, result, report);

        Assert.That(ret, Is.False);
        Assert.That(report.Entries, Has.Count.EqualTo(1));
        Assert.That(report.Entries[0].FileId, Is.EqualTo("c"));
    }

    [Test]
    public void Include_fails_without_parameter()
    {
        Preprocessor preprocessor = new Preprocessor(new EmptyIncludeResolver(), new DummyExpressionSolver(), PreprocessorOptions.Default);

        // disable formatter to keep 2 spaces after include
        // @formatter:off
        const string sourceString = 
            """
            first line
            second line
            #include  
            third line
            fourth line
            """;
        // @formatter:on

        using TextReader source = new StringReader(sourceString);

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);
        result.NewLine = "\r\n";

        TestReport report = new TestReport();
        bool ret = preprocessor.Process(source, result, report);

        Assert.That(ret, Is.False);
        Assert.That(report.Entries, Has.Count.EqualTo(1));
        Assert.That(report.Entries[0].Line, Is.EqualTo(2));
        Assert.That(report.Entries[0].Column, Is.EqualTo(10));
    }
}
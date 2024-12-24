using System.IO;
using System.Text;
using SimpleTextPreprocessor;

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


    [Test]
    public void Include_replaced_with_text()
    {
        Preprocessor preprocessor = new Preprocessor(new StringIncludeResolver(), new DummyExpressionSolver(), PreprocessorOptions.Default);

        using TextReader source = new StringReader(_SOURCE);

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);
        result.NewLine = "\r\n";

        bool ret = preprocessor.Process(source, result);

        Assert.That(ret, Is.True);
        Assert.That(sb.ToString(), Is.EqualTo(_EXPECTED));
    }

    [Test]
    public void Include_fail_with_null_resolver()
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

    private class StringIncludeResolver : IIncludeResolver
    {
        public bool TryCreateReader(string sourceFileId, string includeParameter, out string? newFileId, out TextReader? reader, IReport? report)
        {
            newFileId = includeParameter;
            reader = new StringReader(_INCLUDE);
            return true;
        }
    }
}
using System.IO;
using System.Text;
using SimpleTextPreprocessor;
using SimpleTextPreprocessor.ExpressionSolver;
using SimpleTextPreprocessor.IncludeResolver;

namespace Tests;

public class TestOptions
{
    [Test]
    public void Alternative_directive_char()
    {
        const string sourceText =
            """
            line 1
            %if FILE_DEFINED
            file defined line 1
            %endif
            %if SYSTEM_DEFINED
            system defined line 1
            %endif
            %define FILE_DEFINED
            %undef SYSTEM_DEFINED
            line 2
            %if FILE_DEFINED
            file defined line 2
            %endif
            %if SYSTEM_DEFINED
            system defined line 2
            %endif
            """;

        const string expectedText =
            """
            line 1
            system defined line 1
            line 2
            file defined line 2

            """;

        PreprocessorOptions options = new()
        {
            DirectiveChar = '%'
        };

        Preprocessor preprocessor = new Preprocessor(new NullIncludeResolver(), new DefaultExpressionSolver(), options);
        preprocessor.AddSymbol("SYSTEM_DEFINED");
        using TextReader source = new StringReader(sourceText);

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);
        result.NewLine = "\r\n";

        bool ret = preprocessor.Process(source, result);

        Assert.That(ret, Is.True);
        Assert.That(sb.ToString(), Is.EqualTo(expectedText));
    }

    [Test]
    public void Unknown_directive_is_skipped()
    {
        const string sourceText =
            """
            line 1
            #unknown directive
            line 2
            """;

        const string expectedText =
            """
            line 1
            line 2

            """;

        Preprocessor preprocessor = new Preprocessor();
        using TextReader source = new StringReader(sourceText);

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);
        result.NewLine = "\r\n";

        bool ret = preprocessor.Process(source, result);

        Assert.That(ret, Is.True);
        Assert.That(sb.ToString(), Is.EqualTo(expectedText));
    }

    [Test]
    public void Empty_directive_is_skipped()
    {
        // disable formatter to keep 2 spaces after #
        // @formatter:off
        const string sourceText =
            """
            line 1
            #
            #  
            line 2
            """;
        // @formatter:on

        const string expectedText =
            """
            line 1
            line 2

            """;

        Preprocessor preprocessor = new Preprocessor();
        using TextReader source = new StringReader(sourceText);

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);
        result.NewLine = "\r\n";

        bool ret = preprocessor.Process(source, result);

        Assert.That(ret, Is.True);
        Assert.That(sb.ToString(), Is.EqualTo(expectedText));
    }

    [Test]
    public void Fail_on_unknown_directive()
    {
        const string sourceText =
            """
            line 1
            #unknown directive
            line 2
            """;

        PreprocessorOptions options = PreprocessorOptions.Default;
        options.ErrorOnUnknownDirective = true;

        Preprocessor preprocessor = new Preprocessor(new NullIncludeResolver(), new DefaultExpressionSolver(), options);
        using TextReader source = new StringReader(sourceText);

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);
        result.NewLine = "\r\n";

        ReportList report = new ReportList();
        bool ret = preprocessor.Process(source, result, report);

        Assert.That(ret, Is.False);
        Assert.That(report.Entries, Has.Count.EqualTo(1));
    }

    [Test]
    public void Fail_on_missing_directive()
    {
        const string sourceText =
            """
            line 1
            #
            line 2
            """;

        PreprocessorOptions options = PreprocessorOptions.Default;
        options.ErrorOnMissingDirective = true;

        Preprocessor preprocessor = new Preprocessor(new NullIncludeResolver(), new DefaultExpressionSolver(), options);
        using TextReader source = new StringReader(sourceText);

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);
        result.NewLine = "\r\n";

        ReportList report = new ReportList();
        bool ret = preprocessor.Process(source, result, report);

        Assert.That(ret, Is.False);
        Assert.That(report.Entries, Has.Count.EqualTo(1));
    }
}
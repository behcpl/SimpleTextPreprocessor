using System.Collections.Generic;
using System.IO;
using System.Text;
using SimpleTextPreprocessor;

namespace Tests;

public class ConditionalBlockTests
{
    private static IEnumerable<TestCaseData> SimpleConditionCases()
    {
        yield return new TestCaseData(
            """
            first line
            second line
            #if true
            block line 1
            block line 2
            #endif
            third line
            fourth line
            """,
            """
            first line
            second line
            block line 1
            block line 2
            third line
            fourth line

            """).SetName("Include always true conditional block");

        yield return new TestCaseData(
            """
            first line
            second line
            #if false
            block line 1
            block line 2
            #endif
            third line
            fourth line
            """,
            """
            first line
            second line
            third line
            fourth line

            """).SetName("Skip always false conditional block");
        yield return new TestCaseData(
            """
            first line
            second line
            #if true
            block line 1
            #if false
            nested line
            #endif
            block line 2
            #endif
            third line
            fourth line
            """,
            """
            first line
            second line
            block line 1
            block line 2
            third line
            fourth line

            """).SetName("Skip nested block only");
        yield return new TestCaseData(
            """
            first line
            second line
            #if false
            block line 1
            #if true
            nested line
            #endif
            block line 2
            #endif
            third line
            fourth line
            """,
            """
            first line
            second line
            third line
            fourth line

            """).SetName("Skip all nested blocks");
    }

    private static IEnumerable<TestCaseData> ElseConditionCases()
    {
        yield return new TestCaseData(
            """
            first line
            second line
            #if true
            block line 1
            block line 2
            #else
            other line 1
            other line 2
            #endif
            third line
            fourth line
            """,
            """
            first line
            second line
            block line 1
            block line 2
            third line
            fourth line

            """).SetName("Skip else block when condition met");
        yield return new TestCaseData(
            """
            first line
            second line
            #if false
            block line 1
            block line 2
            #else
            other line 1
            other line 2
            #endif
            third line
            fourth line
            """,
            """
            first line
            second line
            other line 1
            other line 2
            third line
            fourth line

            """).SetName("Use else block when condition wasn't met");
    }

    private static IEnumerable<TestCaseData> ElifConditionCases()
    {
        yield return new TestCaseData(
            """
            start line
            #if true
            if line
            #elif true
            elif1 line
            #elif false
            elif2 line
            #else
            else line
            #endif
            end line
            """,
            """
            start line
            if line
            end line

            """).SetName("Skip elif and else block when condition met");
        yield return new TestCaseData(
            """
            start line
            #if false
            if line
            #elif true
            elif1 line
            #elif false
            elif2 line
            #else
            else line
            #endif
            end line
            """,
            """
            start line
            elif1 line
            end line

            """).SetName("Take 1st elif block when condition met");
        yield return new TestCaseData(
            """
            start line
            #if false
            if line
            #elif false
            elif1 line
            #elif true
            elif2 line
            #else
            else line
            #endif
            end line
            """,
            """
            start line
            elif2 line
            end line

            """).SetName("Take 2nd elif block when condition met");
        yield return new TestCaseData(
            """
            start line
            #if false
            if line
            #elif false
            elif1 line
            #elif false
            elif2 line
            #else
            else line
            #endif
            end line
            """,
            """
            start line
            else line
            end line

            """).SetName("Use else block when no conditions were met");
    }

    private static IEnumerable<TestCaseData> HandleOtherDirectivesCases()
    {
        yield return new TestCaseData(
            """
            start line
            #if true
            #define DEF1
            #else
            #define DEF2
            #endif
            #if DEF1
            def1 line
            #endif
            #if DEF2
            def2 line
            #endif
            end line
            """,
            """
            start line
            def1 line
            end line

            """).SetName("Ignore #define when skipping block");
        yield return new TestCaseData(
            """
            start line
            #define DEF1
            #define DEF2
            #if true
            #undef DEF1
            #else
            #undef DEF2
            #endif
            #if DEF1
            def1 line
            #endif
            #if DEF2
            def2 line
            #endif
            end line
            """,
            """
            start line
            def2 line
            end line

            """).SetName("Ignore #undef when skipping block");
        yield return new TestCaseData(
            """
            start line
            #if true
            #version 1
            #else
            #version 2
            #endif
            end line
            """,
            """
            start line
            #version 1
            end line

            """).SetName("Skip ignored directives when skipping block");
    }

    [TestCaseSource(nameof(SimpleConditionCases))]
    [TestCaseSource(nameof(ElseConditionCases))]
    [TestCaseSource(nameof(ElifConditionCases))]
    [TestCaseSource(nameof(HandleOtherDirectivesCases))]
    public void Handle_conditional_blocks(string sourceText, string expectedText)
    {
        Preprocessor preprocessor = new Preprocessor();
        preprocessor.AddToIgnored("version");
        using TextReader source = new StringReader(sourceText);

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);
        result.NewLine = "\r\n";

        bool ret = preprocessor.Process(source, result);

        Assert.That(ret, Is.True);
        Assert.That(sb.ToString(), Is.EqualTo(expectedText));
    }

    // TODO: test #if false
    // TODO: test nested ifs
    // TODO: test nested ifs, only one unpaired
    [Test]
    public void Fail_on_unpaired_block()
    {
        const string sourceText =
            """
            first line
            second line
            #if true
            third line
            fourth line
            """;

        Preprocessor preprocessor = new Preprocessor();
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
    public void Fail_on_endif_before_if()
    {
        const string sourceText =
            """
            first line
            second line
            #endif
            third line
            fourth line
            """;

        Preprocessor preprocessor = new Preprocessor();
        using TextReader source = new StringReader(sourceText);

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);
        result.NewLine = "\r\n";

        ReportList report = new ReportList();
        bool ret = preprocessor.Process(source, result, report);

        Assert.That(ret, Is.False);
        Assert.That(report.Entries, Has.Count.EqualTo(1));
        Assert.That(report.Entries[0].Line, Is.EqualTo(2));
    }


    [Test]
    public void Fail_on_multiple_else()
    {
        const string sourceText =
            """
            first line
            #if true
            second line
            #else
            third line
            #else
            fourth line
            #endif
            """;

        Preprocessor preprocessor = new Preprocessor();
        using TextReader source = new StringReader(sourceText);

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);
        result.NewLine = "\r\n";

        ReportList report = new ReportList();
        bool ret = preprocessor.Process(source, result, report);

        Assert.That(ret, Is.False);
        Assert.That(report.Entries, Has.Count.EqualTo(1));
        Assert.That(report.Entries[0].Line, Is.EqualTo(5));
    }

    [Test]
    public void Fail_on_elif_before_if()
    {
        const string sourceText =
            """
            first line
            second line
            #elif
            third line
            #endif
            fourth line
            """;

        Preprocessor preprocessor = new Preprocessor();
        using TextReader source = new StringReader(sourceText);

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);
        result.NewLine = "\r\n";

        ReportList report = new ReportList();
        bool ret = preprocessor.Process(source, result, report);

        Assert.That(ret, Is.False);
        Assert.That(report.Entries, Has.Count.EqualTo(2));
        Assert.That(report.Entries[0].Line, Is.EqualTo(2));
        Assert.That(report.Entries[1].Line, Is.EqualTo(4));
    }

    [Test]
    public void Fail_on_elif_after_else()
    {
        const string sourceText =
            """
            first line
            #if true
            second line
            #else
            third line
            #elif
            fourth line
            #endif
            """;

        Preprocessor preprocessor = new Preprocessor();
        using TextReader source = new StringReader(sourceText);

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);
        result.NewLine = "\r\n";

        ReportList report = new ReportList();
        bool ret = preprocessor.Process(source, result, report);

        Assert.That(ret, Is.False);
        Assert.That(report.Entries, Has.Count.EqualTo(1));
        Assert.That(report.Entries[0].Line, Is.EqualTo(5));
    }

    [Test]
    public void Fail_on_if_without_expression()
    {
        // disable formatter to keep 2 spaces after elif
        // @formatter:off
        const string sourceText =
            """
            first line
            #if  
            second line
            #endif
            third line
            """;
        // @formatter:on

        Preprocessor preprocessor = new Preprocessor();
        using TextReader source = new StringReader(sourceText);

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);
        result.NewLine = "\r\n";

        ReportList report = new ReportList();
        bool ret = preprocessor.Process(source, result, report);

        Assert.That(ret, Is.False);
        Assert.That(report.Entries, Has.Count.EqualTo(1));
        Assert.That(report.Entries[0].Line, Is.EqualTo(1));
        Assert.That(report.Entries[0].Column, Is.EqualTo(5)); // NOTE: extra 2 spaces after #if
    }

    [Test]
    public void Fail_on_elif_without_expression()
    {
        // disable formatter to keep 2 spaces after elif
        // @formatter:off
        const string sourceText =
            """
            first line
            #if true
            second line
            #elif  
            third line
            #endif
            fourth line
            """;
        // @formatter:on

        Preprocessor preprocessor = new Preprocessor();
        using TextReader source = new StringReader(sourceText);

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);
        result.NewLine = "\r\n";

        ReportList report = new ReportList();
        bool ret = preprocessor.Process(source, result, report);

        Assert.That(ret, Is.False);
        Assert.That(report.Entries, Has.Count.EqualTo(1));
        Assert.That(report.Entries[0].Line, Is.EqualTo(3));
        Assert.That(report.Entries[0].Column, Is.EqualTo(7)); // NOTE: extra 2 spaces after #elif
    }


    [Test]
    public void Fail_on_text_after_else()
    {
        const string sourceText =
            """
            first line
            #if true
            second line
            #else something
            third line
            #endif
            fourth line
            """;

        Preprocessor preprocessor = new Preprocessor();
        using TextReader source = new StringReader(sourceText);

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);
        result.NewLine = "\r\n";

        ReportList report = new ReportList();
        bool ret = preprocessor.Process(source, result, report);

        Assert.That(ret, Is.False);
        Assert.That(report.Entries, Has.Count.EqualTo(1));
        Assert.That(report.Entries[0].Line, Is.EqualTo(3));
        Assert.That(report.Entries[0].Column, Is.EqualTo(6));
    }

    [Test]
    public void Fail_on_text_after_endif()
    {
        const string sourceText =
            """
            first line
            #if true
            second line
            #endif something
            third line
            """;

        Preprocessor preprocessor = new Preprocessor();
        using TextReader source = new StringReader(sourceText);

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);
        result.NewLine = "\r\n";

        ReportList report = new ReportList();
        bool ret = preprocessor.Process(source, result, report);

        Assert.That(ret, Is.False);
        Assert.That(report.Entries, Has.Count.EqualTo(1));
        Assert.That(report.Entries[0].Line, Is.EqualTo(3));
        Assert.That(report.Entries[0].Column, Is.EqualTo(7));
    }
}
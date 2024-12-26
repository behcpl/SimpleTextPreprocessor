using System.IO;
using System.Text;
using SimpleTextPreprocessor;
using SimpleTextPreprocessor.ExpressionSolver;
using SimpleTextPreprocessor.IncludeResolver;

namespace Tests;

public class DefineTests
{
    [Test]
    public void Modify_symbols_from_text()
    {
        const string sourceText =
            """
            line 1
            #if FILE_DEFINED
            file defined line 1
            #endif
            #if SYSTEM_DEFINED
            system defined line 1
            #endif
            #define FILE_DEFINED
            #undef SYSTEM_DEFINED
            line 2
            #if FILE_DEFINED
            file defined line 2
            #endif
            #if SYSTEM_DEFINED
            system defined line 2
            #endif
            """;

        const string expectedText =
            """
            line 1
            system defined line 1
            line 2
            file defined line 2

            """;

        Preprocessor preprocessor = new Preprocessor();
        preprocessor.Define("SYSTEM_DEFINED");
        using TextReader source = new StringReader(sourceText);

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);
        result.NewLine = "\r\n";

        bool ret = preprocessor.Process(source, result);

        Assert.That(ret, Is.True);
        Assert.That(sb.ToString(), Is.EqualTo(expectedText));
    }

    [Test]
    public void Modified_symbols_affects_included_sections()
    {
        const string sourceText =
            """
            line 1
            #include check
            #define FILE_DEFINED
            #undef SYSTEM_DEFINED
            line 2
            #include check
            """;

        const string expectedText =
            """
            line 1
            include system defined line
            line 2
            include file defined line

            """;

        InMemoryIncludeResolver includeResolver = new InMemoryIncludeResolver();
        includeResolver.Entries["check"] =
            """
            #if FILE_DEFINED
            include file defined line
            #endif
            #if SYSTEM_DEFINED
            include system defined line
            #endif
            """;

        Preprocessor preprocessor = new Preprocessor(includeResolver, new DummyExpressionSolver(), PreprocessorOptions.Default);
        preprocessor.Define("SYSTEM_DEFINED");
        using TextReader source = new StringReader(sourceText);

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);
        result.NewLine = "\r\n";

        bool ret = preprocessor.Process(source, result);

        Assert.That(ret, Is.True);
        Assert.That(sb.ToString(), Is.EqualTo(expectedText));
    }

    [Test]
    public void Symbols_modified_inside_include_section_affects_parent_section()
    {
        const string sourceText =
            """
            line 1
            #if FILE_DEFINED
            file defined line 1
            #endif
            #if SYSTEM_DEFINED
            system defined line 1
            #endif
            #include set
            line 2
            #if FILE_DEFINED
            file defined line 2
            #endif
            #if SYSTEM_DEFINED
            system defined line 2
            #endif
            """;

        const string expectedText =
            """
            line 1
            system defined line 1
            line 2
            file defined line 2

            """;

        InMemoryIncludeResolver includeResolver = new InMemoryIncludeResolver();
        includeResolver.Entries["set"] =
            """
            #define FILE_DEFINED
            #undef SYSTEM_DEFINED
            """;

        Preprocessor preprocessor = new Preprocessor(includeResolver, new DummyExpressionSolver(), PreprocessorOptions.Default);
        preprocessor.Define("SYSTEM_DEFINED");
        using TextReader source = new StringReader(sourceText);

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);
        result.NewLine = "\r\n";

        bool ret = preprocessor.Process(source, result);

        Assert.That(ret, Is.True);
        Assert.That(sb.ToString(), Is.EqualTo(expectedText));
    }
    
    [Test]
    public void Define_fails_without_symbol_name()
    {
        Preprocessor preprocessor = new Preprocessor(new EmptyIncludeResolver(), new DummyExpressionSolver(), PreprocessorOptions.Default);

        // disable formatter to keep 2 spaces after define
        // @formatter:off
        const string sourceString = 
            """
            first line
            second line
            #define  
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
        Assert.That(report.Entries[0].Column, Is.EqualTo(9));
    }
    
    [Test]
    public void Undef_fails_without_symbol_name()
    {
        Preprocessor preprocessor = new Preprocessor(new EmptyIncludeResolver(), new DummyExpressionSolver(), PreprocessorOptions.Default);

        // disable formatter to keep 2 spaces after undef
        // @formatter:off
        const string sourceString = 
            """
            first line
            second line
            #undef  
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
        Assert.That(report.Entries[0].Column, Is.EqualTo(8));
    }  
    
    [Test]
    public void Undef_fails_with_characters_after_symbol_name()
    {
        Preprocessor preprocessor = new Preprocessor(new EmptyIncludeResolver(), new DummyExpressionSolver(), PreprocessorOptions.Default);

        // disable formatter to keep 2 spaces after undef
        // @formatter:off
        const string sourceString = 
            """
            first line
            second line
            #undef SOMETHING  text  
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
        Assert.That(report.Entries[0].Column, Is.EqualTo(18));
    }
}
using System.Collections.Generic;
using System.IO;
using System.Text;
using SimpleTextPreprocessor;

namespace Tests;

public class BasicTests
{
    private const string _PURE_TEXT =
        """
        first line
        second line
        third line
        fourth line
        """;

    private const string _PURE_TEXT_WITH_ENDL = _PURE_TEXT + "\r\n";

    private const string _UNKNOWN_DIRECTIVES =
        """
        #version 1.2.3
        first line
        second line
        #something else
        third line
        fourth line

        """;

    private const string _KEEP_VERSION =
        """
        #version 1.2.3
        first line
        second line
        third line
        fourth line

        """;

    private static IEnumerable<TestCaseData> TestCases()
    {
        yield return new TestCaseData(_PURE_TEXT_WITH_ENDL, _PURE_TEXT_WITH_ENDL).SetName("Copy simple text");
        yield return new TestCaseData(_UNKNOWN_DIRECTIVES, _KEEP_VERSION).SetName("Strip unknown directives, keep ignored");
    }

    [TestCaseSource(nameof(TestCases))]
    public void Handle_basic_cases(string sourceText, string expectedText)
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
}
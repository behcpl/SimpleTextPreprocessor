using System.Collections.Generic;
using System.IO;
using System.Text;
using SimpleTextPreprocessor;

namespace Tests;

public class ConditionalBlockTests
{
    public static IEnumerable<TestCaseData> TestCases()
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

    [TestCaseSource(nameof(TestCases))]
    public void DoTests(string sourceText, string expectedText)
    {
        Preprocessor preprocessor = new Preprocessor();
        using TextReader source = new StringReader(sourceText);

        StringBuilder sb = new StringBuilder();
        using TextWriter result = new StringWriter(sb);
        result.NewLine = "\r\n";

        preprocessor.Process(source, result);

        Assert.That(sb.ToString(), Is.EqualTo(expectedText));
    }
}
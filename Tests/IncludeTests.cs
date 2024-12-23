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

    private class StringIncludeResolver : IIncludeResolver
    {
        public TextReader CreateReader(string currentPath, string includePath)
        {
            return new StringReader(_INCLUDE);
        }
    }
}
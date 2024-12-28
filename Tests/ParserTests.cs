using System.Collections.Generic;
using SimpleTextPreprocessor;
using SimpleTextPreprocessor.ExpressionSolver;

namespace Tests;

public class ParserTests
{
    private static IEnumerable<TestCaseData> ValidExpressions()
    {
        yield return new TestCaseData("true", true).SetName("Eval true");
        yield return new TestCaseData("false", false).SetName("Eval false");
        yield return new TestCaseData("42", true).SetName("Implicit int conversion is true");
        yield return new TestCaseData("DEF_UNKNOWN", false).SetName("Undefined symbol is false");
        yield return new TestCaseData("DEF_NULL", true).SetName("Null value is true");
        yield return new TestCaseData("false || false || true", true).SetName("Eval multiple or");
        yield return new TestCaseData("true && true && false", false).SetName("Eval multiple and");
        yield return new TestCaseData("!false", true).SetName("Eval negate");
        yield return new TestCaseData("!!true", true).SetName("Eval negate of negate");
        yield return new TestCaseData("true || true && false", true).SetName("Precedence of and_or");
        yield return new TestCaseData("(true || true) && false", false).SetName("Grouping of and_or");
        yield return new TestCaseData("false == !true", true).SetName("Equals bool 1");
        yield return new TestCaseData("false == true", false).SetName("Equals bool 2");
        yield return new TestCaseData("false != !true", false).SetName("Not equals bool 1");
        yield return new TestCaseData("false != true", true).SetName("Not equals bool 2");
        yield return new TestCaseData("DEF_42 == 42", true).SetName("Equals int 1");
        yield return new TestCaseData("DEF_42 == 40", false).SetName("Equals int 2");
        yield return new TestCaseData("DEF_42 != 42", false).SetName("Not equals int 1");
        yield return new TestCaseData("DEF_42 != 40", true).SetName("Not equals int 2");
        yield return new TestCaseData("DEF_42 > 44", false).SetName("Greater 1");
        yield return new TestCaseData("DEF_42 > 42", false).SetName("Greater 2");
        yield return new TestCaseData("DEF_42 > 40", true).SetName("Greater 3");
        yield return new TestCaseData("DEF_42 >= 44", false).SetName("Greater equal 1");
        yield return new TestCaseData("DEF_42 >= 42", true).SetName("Greater equal 2");
        yield return new TestCaseData("DEF_42 >= 40", true).SetName("Greater equal 3");
        yield return new TestCaseData("DEF_42 < 44", true).SetName("Lesser 1");
        yield return new TestCaseData("DEF_42 < 42", false).SetName("Lesser 2");
        yield return new TestCaseData("DEF_42 < 40", false).SetName("Lesser 3");
        yield return new TestCaseData("DEF_42 <= 44", true).SetName("Lesser equal 1");
        yield return new TestCaseData("DEF_42 <= 42", true).SetName("Lesser equal 2");
        yield return new TestCaseData("DEF_42 <= 40", false).SetName("Lesser equal 3");
        yield return new TestCaseData("DEF_UNKNOWN && (DEF_42 > 13 || DEF_UNKNOWN < 0) || -DEF_666 < 0", true).SetName("Complex scenario");
    }
    
    [TestCaseSource(nameof(ValidExpressions))]
    public void Valid_expressions(string expression, bool expectedResult)
    {
        Parser parser = new Parser(expression, Symbols(), null);
        bool valid = parser.TryEvaluate(out bool result);

        Assert.That(valid, Is.True);
        Assert.That(result, Is.EqualTo(expectedResult));
    }
    
    [Test]
    public void Fail_on_empty()
    {
        ReportList report = new ReportList();
        Parser parser = new Parser("", Symbols(), report);
        bool valid = parser.TryEvaluate(out bool result);

        Assert.That(valid, Is.False);
        Assert.That(report.Entries, Has.Count.EqualTo(1));
    }

    [Test]
    public void Fail_on_invalid_character()
    {
        ReportList report = new ReportList();
        Parser parser = new Parser("DEF_42, DEF_666", Symbols(), report);
        bool valid = parser.TryEvaluate(out bool result);

        Assert.That(valid, Is.False);
        Assert.That(report.Entries, Has.Count.EqualTo(1));
        Assert.That(report.Entries[0].Column, Is.EqualTo(6));
    }

    [Test]
    public void Fail_on_invalid_int_1()
    {
        ReportList report = new ReportList();
        Parser parser = new Parser("123f > 0", Symbols(), report);
        bool valid = parser.TryEvaluate(out bool result);

        Assert.That(valid, Is.False);
        Assert.That(report.Entries, Has.Count.EqualTo(1));
        Assert.That(report.Entries[0].Column, Is.EqualTo(0));
    }

    [Test]
    public void Fail_on_invalid_int_2()
    {
        ReportList report = new ReportList();
        Parser parser = new Parser("0 < 123f", Symbols(), report);
        bool valid = parser.TryEvaluate(out bool result);

        Assert.That(valid, Is.False);
        Assert.That(report.Entries, Has.Count.EqualTo(1));
        Assert.That(report.Entries[0].Column, Is.EqualTo(4));
    }

    [Test]
    public void Fail_on_invalid_int_3()
    {
        ReportList report = new ReportList();
        Parser parser = new Parser("0x1j > 0", Symbols(), report);
        bool valid = parser.TryEvaluate(out bool result);

        Assert.That(valid, Is.False);
        Assert.That(report.Entries, Has.Count.EqualTo(1));
        Assert.That(report.Entries[0].Column, Is.EqualTo(0));
    }

    [Test]
    public void Fail_on_invalid_int_4()
    {
        ReportList report = new ReportList();
        Parser parser = new Parser("0x > 0", Symbols(), report);
        bool valid = parser.TryEvaluate(out bool result);

        Assert.That(valid, Is.False);
        Assert.That(report.Entries, Has.Count.EqualTo(1));
        Assert.That(report.Entries[0].Column, Is.EqualTo(0));
    }

    [Test]
    public void Fail_on_invalid_int_5()
    {
        ReportList report = new ReportList();
        Parser parser = new Parser("0 > 0x", Symbols(), report);
        bool valid = parser.TryEvaluate(out bool result);

        Assert.That(valid, Is.False);
        Assert.That(report.Entries, Has.Count.EqualTo(1));
        Assert.That(report.Entries[0].Column, Is.EqualTo(4));
    }

    [Test]
    public void Fail_on_unpaired_grouping_1()
    {
        ReportList report = new ReportList();
        Parser parser = new Parser("(true || false", Symbols(), report);
        bool valid = parser.TryEvaluate(out bool result);

        Assert.That(valid, Is.False);
        Assert.That(report.Entries, Has.Count.EqualTo(1));
        Assert.That(report.Entries[0].Column, Is.EqualTo(14));
    }
  
    [Test]
    public void Fail_on_unpaired_grouping_2()
    {
        ReportList report = new ReportList();
        Parser parser = new Parser("true) || false", Symbols(), report);
        bool valid = parser.TryEvaluate(out bool result);

        Assert.That(valid, Is.False);
        Assert.That(report.Entries, Has.Count.EqualTo(1));
        // Assert.That(report.Entries[0].Column, Is.EqualTo(4));
    }
    
    private static IReadOnlyDictionary<string, string?> Symbols()
    {
        return new Dictionary<string, string?>
        {
            { "DEF_TRUE", "true" },
            { "DEF_FALSE", "false" },
            { "DEF_NULL", null },
            { "DEF_42", "42" },
            { "DEF_666", "666" }
        };
    }
}
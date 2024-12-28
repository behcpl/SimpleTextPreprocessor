using System;
using System.Collections.Generic;
using SimpleTextPreprocessor.ExpressionSolver;

namespace Tests;

public class TokenizerTests
{
    [Test]
    public void Recognize_all_tokens()
    {
        const string input = " ( ) && || ! != == > >= < <= - 123 0x123 true false UNDEFINED ";
        Dictionary<string, string?> symbols = new();
        int position = 0;

        Token token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.GROUP_START));

        token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.GROUP_END));

        token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.LOGIC_AND));

        token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.LOGIC_OR));

        token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.LOGIC_NOT));

        token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.NOT_EQUAL));

        token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.EQUAL));

        token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.GREATER));

        token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.GREATER_EQUAL));

        token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.LESSER));

        token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.LESSER_EQUAL));

        token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.NEGATE));

        token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.INT_VALUE));
        Assert.That(token.IntValue, Is.EqualTo(123));

        token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.INT_VALUE));
        Assert.That(token.IntValue, Is.EqualTo(0x123));

        token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.BOOL_VALUE));
        Assert.That(token.BoolValue, Is.True);

        token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.BOOL_VALUE));
        Assert.That(token.BoolValue, Is.False);

        token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.UNDEFINED_VALUE));

        token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.END));
    }

    [Test]
    public void Recognize_single_char_tokens()
    {
        const string input = "3)0x5)A)";
        Dictionary<string, string?> symbols = new()
        {
            { "A", "7" }
        };
        int position = 0;

        Token token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.INT_VALUE));
        Assert.That(token.IntValue, Is.EqualTo(3));

        token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.GROUP_END));

        token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.INT_VALUE));
        Assert.That(token.IntValue, Is.EqualTo(5));

        token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.GROUP_END));

        token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.INT_VALUE));
        Assert.That(token.IntValue, Is.EqualTo(7));
    }

    [Test]
    public void Recognize_single_char_tokens_at_end()
    {
        Dictionary<string, string?> symbols = new()
        {
            { "A", "7" }
        };

        int position = 0;
        Token token = Tokenizer.GetToken("3", symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.INT_VALUE));
        Assert.That(token.IntValue, Is.EqualTo(3));

        position = 0;
        token = Tokenizer.GetToken("A", symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.INT_VALUE));
        Assert.That(token.IntValue, Is.EqualTo(7));
    }

    [Test]
    public void Resolve_symbols_as_values()
    {
        Dictionary<string, string?> symbols = new()
        {
            { "INT_12", "12" },
            { "HEX_66", "0x66" },
            { "BOOL_TRUE", "true" },
            { "BOOL_FALSE", "false" },
            { "NULL_VALUE", null }
        };

        const string input = "INT_12 HEX_66 BOOL_TRUE BOOL_FALSE NULL_VALUE";
        int position = 0;

        Token token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.INT_VALUE));
        Assert.That(token.IntValue, Is.EqualTo(12));

        token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.INT_VALUE));
        Assert.That(token.IntValue, Is.EqualTo(0x66));

        token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.BOOL_VALUE));
        Assert.That(token.BoolValue, Is.True);

        token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.BOOL_VALUE));
        Assert.That(token.BoolValue, Is.False);

        token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.BOOL_VALUE));
        Assert.That(token.BoolValue, Is.True);
    }

    [Test]
    public void Fail_on_invalid_characters()
    {
        const string input = "SYMBOL, OTHER";
        Dictionary<string, string?> symbols = new();
        int position = 0;

        Token token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.UNDEFINED_VALUE));

        token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.INVALID));

        token = Tokenizer.GetToken(input, symbols, ref position, out _);
        Assert.That(token.Type, Is.EqualTo(TokenType.UNDEFINED_VALUE));
    }

    [Test]
    public void Fail_on_invalid_integer()
    {
        const string input = "123a 0xABCO";
        Dictionary<string, string?> symbols = new();
        int position = 0;

        Token token = Tokenizer.GetToken(input, symbols, ref position, out string? msg1);
        Assert.That(token.Type, Is.EqualTo(TokenType.INVALID));

        Console.WriteLine(msg1);

        token = Tokenizer.GetToken(input, symbols, ref position, out string? msg2);
        Assert.That(token.Type, Is.EqualTo(TokenType.INVALID));

        Console.WriteLine(msg2);
    }
}
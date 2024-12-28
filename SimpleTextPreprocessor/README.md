# SimpleTextPreprocessor

Simple text preprocessor that supports basic directives like `#include` and conditional blocks (`#if`/`#elif`/`#else`/`#endif`).
Not as powerful as C/C++ preprocessor, but slightly more robust than C# preprocessor directives.

Expressions for conditional blocks (`#if`/`#elif`) can include boolean logic and integer comparisons:
```c++
#if DEF1 && (DEF2 || DEF3)
// or
#if DEF1 > 0x10 && (DEF2 || DEF3 != 0)
```

Check [repository](https://github.com/behcpl/SimpleTextPreprocessor) for more detailed description.

## How to use

### Examples

#### Simple string processing, without `#include` support:
```csharp
Preprocessor preprocessor = new();

using TextReader source = new StringReader(sourceText);

StringBuilder sb = new StringBuilder();
using TextWriter result = new StringWriter(sb);

bool success = preprocessor.Process(source, result);
```

#### String processing, with `#include` support, ignored directive, and defined symbols:
```csharp
InMemoryIncludeResolver includeResolver = new();
includeResolver.Entries.Add("file1", "some content...");
includeResolver.Entries.Add("file2", "some content...");

Preprocessor preprocessor = new (includeResolver, new DefaultExpressionSolver(), PreprocessorOptions.Default);
preprocessor.AddToIgnored("version");
preprocessor.AddSymbol("DEF1", "123");
preprocessor.AddSymbol("DEF2", "false");
preprocessor.AddSymbol("DEF3"); // no value, defaults to true

using TextReader source = new StringReader(sourceText);

StringBuilder sb = new StringBuilder();
using TextWriter result = new StringWriter(sb);

bool success = preprocessor.Process(source, result);
```

#### File processing, with `#include`, error reporting, and source line number mapping:
```csharp
FileSystemIncludeResolver includeResolver = new();
 
Preprocessor preprocessor = new (includeResolver, new DefaultExpressionSolver(), PreprocessorOptions.Default);

ReportList report = new();
LineNumberMapper lineNumberMapper = new();

using TextReader source = new StreamReader(sourceFilePath);
using TextWriter output = new StreamWriter(outputFilePath);

string rootFileId = includeResolver.GetFileId(sourceFilePath);
bool success = preprocessor.Process(rootFileId, source, output, report, lineNumberMapper);

if (!success)
{
    foreach (ReportList.Entry entry in report.Entries)
        Console.WriteLine($"{entry.FileId}({entry.Line},{entry.Column}): {entry.Message}");
}

// after using output file by external tools and get error at line 123 it is possible to point to correct file/line number
(string sourceFileId, int sourceLineNumber) = lineNumberMapper.GetSource(123);
```

# SimpleTextPreprocessor

Simple text preprocessor that supports basic directives like `#include` or conditional blocks (`#if` / `#endif`).
Not as powerful as C/C++ preprocessor, but slightly more robust than C# preprocessor directives.

Check [repository](https://github.com/behcpl/SimpleTextPreprocessor) for more detailed description.

## How to use

### Example

```csharp
Preprocessor preprocessor = new();

using TextReader source = new StringReader(sourceText);

StringBuilder sb = new StringBuilder();
using TextWriter result = new StringWriter(sb);

bool success = preprocessor.Process(source, result);
```

## TODO
* Expression solver
* Line number remapping
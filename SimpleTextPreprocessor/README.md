# SimpleTextPreprocessor

Temporary description, for package testing.

## How to use

### Example

```csharp
Preprocessor preprocessor = new();

using TextReader source = new StringReader(sourceText);

StringBuilder sb = new StringBuilder();
using TextWriter result = new StringWriter(sb);

preprocessor.Process(source, result);
```

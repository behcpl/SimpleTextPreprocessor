# SimpleTextPreprocessor

Simple text preprocessor that supports basic directives like `#include` or conditional blocks (`#if` / `#endif`).
Not as powerful as C/C++ preprocessor, but slightly more robust than C# preprocessor directives.

> [!WARNING]  
> Expression solver is not implemented, so only basic, single token expressions are working.

## Features

* Support for conditional blocks
  * `#if EXPRESSION`
  * `#elif EXPRESSION`
  * `#else`
  * `#endif`
  * Conditional blocks can be nested
* Support for `#include` directive
  * Resolve dependencies using file system
  * Use in memory dictionary
  * With an option to implement custom dependency resolver
  * Protection against infinite loops
* Ability to ignore some directives (like `#version 3.3`)
* Error reporting
  * Break on first problem or parse until end of a file

## TODO

* Expression solver
* Line number remapping

[More detailed TODO list](TODO.md)

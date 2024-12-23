# SimpleTextPreprocessor

Simple text preprocessor that supports basic directives like `#include` or conditional blocks (`#if` / `#endif`)

> [!WARNING]  
> This project is in very early stage of development. While it works (without expression solver), there is no error handling/reporting API.
> It is available as a nuget package `BEHC.SimpleTextPreprocessor` mostly as a test for me how to set up GitHub Actions. 

## Features

[TODO List](TODO.md)

* Basic support for conditional blocks
  * `#if EXP` / `#endif` pair
  * no real expression solving yet, only `true`/`false` or symbol exists
* Ability to ignore some directives (like `#version 3.3`)
* Basic support for `#include` directive, with an option to implement custom dependency resolver
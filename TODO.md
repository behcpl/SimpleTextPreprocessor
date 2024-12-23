# TODO

1. ~~Parse string / file line by line~~
2. ~~Tokenize everything that starts with #~~ 
   * ~~allow exceptions, i.e. keep #pragma / #version etc~~
3. #include resolver
   * ~~abstracted by interface~~
   * support for error line number remapping 
4. Conditional blocks
   * ~~#if, #endif~~
   * ~~nested blocks~~
   * #else, #elif support
5. Use ReadOnlySpan<char>
6. Handle errors
   * misconfiguration
   * content errors


## Expected features

### Conditional blocks

- `#if expr` - starts new block
- `#endif` - closes block
- `#else` - alternative block
- `#elif expr` - combined else/if for chaining

#### Expression

- `true`
- `false`
- number literal (int: `0`, `1`, `-42`, `0x01`)
- symbol name
  - default value: `null`
  - no symbol means undefined
  - using undefined symbol in compare node always returns false

Nodes:
- compare: `>`, `<`, `>=`, `<=`, `==`, `!=`
- logic: `&&`, `||`, `!`

### Include directive

- Inserts lines of text in place of directive
- Everything is parsed with same rules
- Defined symbols work across include directive
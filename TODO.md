# TODO

1. Parse string / file line by line
2. Tokenize everything that starts with # 
   * allow exceptions, i.e. keep #pragma / #version etc
3. #include resolver
4. Conditional blocks
   * #if, #elif #endif
   * nested blocks

## Features

### Conditional blocks

- `#if expr` - starts new block
- `#endif` - closes block
- `#else` - alternative block
- `#elif expr` - combined else/if for chaining

#### Expression

- `true`
- `false`
- number literal (int: 0, 1, -42, 0x01)
- symbol name
  - default value: 0
  - no symbol means undefined
  - using undefined symbol always returns false

Nodes:
- compare: >, <, >=, <=, ==, !=
- logic: &&, ||, !

### Include directive

- Inserts lines of text in place of directive
- Everything is parsed with same rules
- Defined symbols work across include directive
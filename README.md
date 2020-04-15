# .NET LL(1) grammar parser


Let's say we have a data structure in C#, we have LL(1)-grammar that represents the format this data structure is represented by some text. This framework let you parse an input-string that represents your data structure. 

To prepare the framework to work with your data structure you have to define:

1. Define data structure for tokens (deriving from *IToken* and *ITokenWithValue\<T>*)
2. Define lexer (Deriving from *ILexer\<UToken>*)
3. Give name to all terminals
4. Define grammar using syntax: *non-terminal: symbol symbol ... symbol { expressions }*
5. Create object of LL1GrammarParser and use the Parse (or the TryParse method) to parse an input



## 1. Data structure for tokens

Data structure for tokens must implement interfaces IToken and ITokenWithValue.  IsCompatiable method is for checking compatibility between two tokens during parsing. Must be true iff token from an input sequence can be used as a leaf in the parsing tree.

```c#
interface IToken
{    
    bool IsCompatible(IToken other);
}
interface ITokenWithValue<out T> : IToken where T : class
{
    T Value { get; }
}
```
## 2. Lexer

Lexer must implement interface ILexer\<UToken>, where UToken is a tokens data structure from 1.

```C#
interface ILexer<T> where T : IToken
{
	IList<T> Tokenize(string str);
}
```

## 3. Names of terminals

User has to give name to each terminal and pass IDictionary<string, IToken> to LL1GrammarParser Constructor.
Terminal names can be one of two types:
single symbol (in single quotes like)
named terminal (like variables in C#)

## 4. Grammar-file syntax

User can define the grammar in the following format of its rule

```
nonterminal_name : symbol1 symbol2 ... symboln { expression; expression; ... expression; }
```

where *symboli* is the name of terminal or non-terminal, *expression* is an expression that describes what to do with result of parsing using this rule.

### Expressions in '{' and '}'

Each rule must have non-empty sequence of expressions that describe what to do with parsing result. Each expression support literals int, double, string, calling costructor, static- or instance-method.

Method calling has the following syntax
```C#
// Instance-method invokation
FullMethodName(instance, arg1, arg2, ..., argn);
// static-method invokation 
static FullMethodName(arg1, ... argn);
// constructor invokation 
new FullTypeName(arg1, ..., argn);
```

FullMethodName (FullTypeMethod) is the full name of the method (type) like if we would use it in C#.



## 5. LL1GrammarParser class

The *LL1GrammarParser\<T, UToken>* has costructor with the following parameters:
*string grammar* - content of grammar-file
*lexer*- instance of user defined class derived from *ILexer\<UToken>*
*NameTerminalTable* that is *IDictionary\<string, IToken>* with names of terminals (described in 3.)
*assemblies* that is array of assemblies with the T - data structure definition. Here the framework will find types and methods using reflection

using System;
using System.Collections.Generic;
using System.Text;

namespace MFFParser
{
    using Bootstrap;
    public class LL1GrammarParser<T, UToken> where UToken: IToken
    {
        private ILexer<UToken> Lexer;
        private AbstractParser<UToken> Parser;

        private LL1GrammarParser() { }

        public LL1GrammarParser(string grammar, ILexer<UToken> lexer, IDictionary<string, UToken> TerminalNames, System.Reflection.Assembly[] assemblies)
        {
            // grammar parsing using GrammarDefinitionParser
            Bootstrap.Lexer grammar_lexer = new Lexer();
            IList<AbstractToken> tokens = grammar_lexer.Tokenize(grammar);
            GrammarCreator creator = new GrammarCreator(new REEvaluator(), new AssembliesAccessWrapper(assemblies));
            foreach (var name_term in TerminalNames)
                creator.RegisterTerminal(name_term.Key, name_term.Value);
            GrammarDefinitionParser.ParseGrammarBase(tokens, ref creator);
            Parser = new RDParser<UToken>(creator.Create());
            Lexer = lexer ?? throw new ArgumentNullException("Lexer is null");
        }

        public T Parse(string str)
        {
            IList<UToken> tokens = Lexer.Tokenize(str);
            var result = Parser.Parse(tokens);
            if (result is T TRes)
                return TRes;
            throw new ParserTypeErrorException($"Result of parsing is not of type {typeof(T).FullName}");
        }

        public bool TryParse(string str, out T result)
        {
            try { result = Parse(str); return true; }
            catch { result = default(T); return false; }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace LL1_Parser
{
    using Bootstrap;
    public class LL1GrammarParser<T, UToken> where UToken: IToken
    {
        private ILexer<UToken> Lexer;
        private AbstractParser<UToken> Parser;

        private LL1GrammarParser() { }

        public LL1GrammarParser(string grammar, ILexer<UToken> lexer, IDictionary<string, UToken> TerminalNames, string StartSymbol, System.Reflection.Assembly[] assemblies)
        {
            Lexer = lexer;
            GrammarCreator creator = ParseGrammar(grammar);
            creator.Assemblies = new AssembliesAccessWrapper(assemblies);
            creator.ExpressionEvaluator = new REEvaluator();
            foreach (var kvp in TerminalNames)
                creator.RegisterTerminal(kvp.Key, kvp.Value);
            creator.SetStartSymbol(StartSymbol);
            var Grammar = creator.Create();
            Parser = new RDParser<UToken>(Grammar);
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

        private static GrammarCreator ParseGrammar(string grammar)
        {
            LL1GrammarParser<GrammarCreator, AbstractToken> ParserForUsersGrammar = new LL1GrammarParser<GrammarCreator, AbstractToken>();
            ParserForUsersGrammar.Lexer = new Lexer();
            ParserForUsersGrammar.Parser = new RDParser<AbstractToken>(Grammar.SpecialGrammar);
            return ParserForUsersGrammar.Parse(grammar);
        }
    }
}

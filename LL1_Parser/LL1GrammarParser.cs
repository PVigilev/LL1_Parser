using System;
using System.Collections.Generic;

namespace LL1_Parser
{
    using Initialization;

    /// <summary>
    /// Recursive Descent Parser for LL(1) grammar
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="UToken"></typeparam>
    public class LL1GrammarParser<T, UToken> where UToken: IToken
    {
        private ILexer<UToken> Lexer;
        private AbstractParser<UToken> Parser;
        
        private LL1GrammarParser() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="grammar">Content of grammar-file</param>
        /// <param name="lexer"></param>
        /// <param name="TerminalNames">Names of terminals</param>
        /// <param name="assemblies">Array of assemblies where framewrok will search for types and methods used in "grammar"</param>
        /// <exception cref="LeftRecursionGrammarException"></exception>
        public LL1GrammarParser(string grammar, ILexer<UToken> lexer, IDictionary<string, UToken> TerminalNames, System.Reflection.Assembly[] assemblies)
        {
            // grammar parsing using GrammarDefinitionParser
            Initialization.Lexer grammar_lexer = new Lexer();
            IList<AbstractToken> tokens = grammar_lexer.Tokenize(grammar);
            GrammarCreator creator = new GrammarCreator(new REEvaluator(), new AssembliesAccessWrapper(assemblies));
            foreach (var name_term in TerminalNames)
                creator.RegisterTerminal(name_term.Key, name_term.Value);
            GrammarDefinitionParser.ParseGrammarBase(tokens, ref creator);
            Parser = new RecursiveDescentParser<UToken>(creator.Create());
            Lexer = lexer ?? throw new ArgumentNullException("Lexer is null");
        }


        /// <summary>
        /// Parse str
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        /// <exception cref="ParserTypeErrorException"></exception>
        /// <exception cref="AmbiguousGrammarException"></exception>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="ParserArgumentErrorException"></exception>
        /// <exception cref="ParserUnknownTokenException"></exception>
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

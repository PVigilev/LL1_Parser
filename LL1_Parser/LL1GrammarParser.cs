using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace LL1_Parser
{
    public class LL1GrammarParser<T, UToken> where UToken: IToken 
    {
        /// <summary>
        /// Parser engine that will be used for parsing input data
        /// </summary>
        AbstractParser<UToken> parser;
        /// <summary>
        /// User-defined lexer
        /// </summary>
        ILexer<UToken> Lexer;
        private LL1GrammarParser() { }
        public LL1GrammarParser(string grammar, ILexer<UToken> ulexer, Assembly[] uassemblies)
        {
            if ((uassemblies ?? throw new ArgumentNullException("Array of assemblies is null")).Length == 0)
                throw new ArgumentException("Empty array of assemblies");
            Lexer = ulexer ?? throw new ArgumentNullException("User-defined lexer is null");
            parser = new Parsers.RDParser<UToken>(GetGrammar(grammar ?? throw new ArgumentNullException("Users grammar is null"), 
                uassemblies));
        }

        private static Grammar GetGrammar(string grammar, Assembly[] assemblies)
        {
            Bootstrap.Lexer lexer = new Bootstrap.Lexer();
            Grammar SpecialGrammar = GrammarCreator.Initializer.CreateDefaultGrammarCreator(new AssembliesAccessWrapper(assemblies), new REEvaluator()).Create();
            var LL1parser = new LL1GrammarParser<GrammarCreator, Bootstrap.AbstractToken>();
            LL1parser.parser = new Parsers.RDParser<Bootstrap.AbstractToken>(SpecialGrammar);
            return LL1parser.Parse(grammar).Create();
        }

        public T Parse(string str)
        {
            var tokens = Lexer.Tokenize(str);
            var result = parser.Parse(tokens);
            if (!(result is T r))
                throw new ParserTypeErrorException($"Result of parsing is not {typeof(T).FullName}");
            return r;
        }
        public bool TryParse(string str, out T result)
        {
            try { result = Parse(str); return true; }
            catch (Exception) { result = default(T); return false; }
        }
    }
}

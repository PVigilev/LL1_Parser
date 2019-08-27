using System.Collections.Generic;

namespace LL1_Parser
{

    /// <summary>
    /// Interface for user-defined lexer that is used for tokenization an input
    /// </summary>
    /// <typeparam name="Token">Type that represents a token data-structure</typeparam>
    public interface ILexer<Token> where Token : IToken
    {
        /// <summary>
        /// Tokenization of an input string
        /// </summary>
        /// <param name="str">Input string that has to be tokenized</param>
        /// <returns>List of tokens</returns>
        IList<Token> Tokenize(string str);
    }
}

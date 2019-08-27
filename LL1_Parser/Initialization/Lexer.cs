using System.Collections.Generic;
using System.Text;

namespace LL1_Parser.Initialization
{

    class Lexer : ILexer<AbstractToken>
    {
        HashSet<char> separators = new HashSet<char> { ' ', '.', ',', ':', ';', '(', ')', '{', '}', '\n', '\r', '"', '\'', '\t' };
        Dictionary<string, TokenKeyWord> keywords = new Dictionary<string, TokenKeyWord> { { "new", TokenKeyWord.Instances[TokenKeyWord.KeyWordType.newKW] }, { "static", TokenKeyWord.Instances[TokenKeyWord.KeyWordType.staticKW] } };
        delegate AbstractToken Tokenizer(string str, int from, out int to);
        Tokenizer[] Tokenizers;
        public Lexer()
        {
            Tokenizers = new Tokenizer[]
            {
                SimpleTokenize, NumberTokenize, WordTokenize, StringTokenize, VarIdTokenize
            };
        }

        bool IsSeparator(char ch) => separators.Contains(ch);

        /// <summary>
        /// Tokenize an input string that contains a grammar definition
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public IList<AbstractToken> Tokenize(string str)
        {
            List<AbstractToken> result = new List<AbstractToken>();
            int i = 0;
            while (i < str.Length)
            {
                if (char.IsWhiteSpace(str[i]))
                {
                    i++;
                    continue;
                }


                int next = -1;
                AbstractToken tkn = null;
                foreach (var ft in Tokenizers)
                {
                    if (tkn == null)
                        tkn = ft(str, i, out next);
                    else break;
                }

                if (tkn == null)
                    throw new ParserUnknownTokenException($"Unknown token on position {i}: {str.Substring(i)}");
                result.Add(tkn);
                i = next;
            }
            return result;
        }

        /// <summary>
        /// Tokenize simple token (braces, comma, column, semicolumn, etc.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        SimpleToken SimpleTokenize(string str, int from, out int to)
        {
            SimpleToken result = null;
            switch (str[from])
            {
                case '.': result = SimpleToken.Instances[SimpleToken.TokenType.dot]; break;
                case ',': result = SimpleToken.Instances[SimpleToken.TokenType.comma]; break;
                case ':': result = SimpleToken.Instances[SimpleToken.TokenType.column]; break;
                case ';': result = SimpleToken.Instances[SimpleToken.TokenType.semicolumn]; break;
                case '(': result = SimpleToken.Instances[SimpleToken.TokenType.OB]; break;
                case ')': result = SimpleToken.Instances[SimpleToken.TokenType.CB]; break;
                case '{': result = SimpleToken.Instances[SimpleToken.TokenType.OCB]; break;
                case '}': result = SimpleToken.Instances[SimpleToken.TokenType.CCB]; break;
                default: break;
            }
            if (result == null)
            {
                to = from;
            }
            else
            {
                to = from + 1;
            }
            return result;
        }

        /// <summary>
        /// Tokenize a number (int or double)
        /// </summary>
        /// <param name="str"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        AbstractToken NumberTokenize(string str, int from, out int to)
        {
            if (str[from] == '-' || char.IsDigit(str[from]))
            {
                int i = str[from] == '-' ? from + 1 : from;
                bool ok = true;
                for (; i < str.Length; i++)
                {
                    if (!char.IsDigit(str[i]) && str[i] != '.')
                    {
                        if (!separators.Contains(str[i]))
                        {
                            ok = false;
                            break;
                        }
                        break;
                    }

                }
                if (ok)
                {
                    string substr = str.Substring(from, i - from);
                    AbstractToken token = IntTokenize(substr);
                    if (token == null)
                        token = DoubleTokenize(substr);
                    if (token == null)
                        to = from;
                    else to = i;
                    return token;
                }
            }
            to = from;
            return null;
        }

        /// <summary>
        /// tokenize double
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        TokenDouble DoubleTokenize(string str)
        {
            double res;
            if (!double.TryParse(str, out res))
                return null;
            return new TokenDouble(res);
        }

        /// <summary>
        /// Tokenize int
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        TokenInt IntTokenize(string str)
        {
            int res;
            if (!int.TryParse(str, out res))
                return null;
            return new TokenInt(res);
        }



        //TokenKeyWord
        ///////////////> AbstractToken
        //TokenName
        /// <summary>
        /// Tokenize single word and resolve if it is keyword or name
        /// </summary>
        /// <param name="str"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        AbstractToken WordTokenize(string str, int from, out int to)
        {
            to = from;
            if (str[from] == '_' || char.IsLetter(str[from]))
            {
                int i = from;
                bool ok = true;
                while (i < str.Length)
                {
                    if (separators.Contains(str[i]))
                        break;
                    if (!(str[i] == '_' || char.IsLetterOrDigit(str[i])))
                    {
                        ok = false;
                        break;
                    }
                    i++;
                }
                if (ok)
                {
                    to = i;
                    string res = str.Substring(from, to - from);
                    if (keywords.ContainsKey(res))
                    {
                        return keywords[res];
                    }
                    else
                        return new TokenSymbol(res);
                }

            }
            else
            {
                int i = from;
                if (i + 2 < str.Length && str[i] == str[i + 2] && str[i] == '\'')
                {
                    AbstractToken result = new TokenSymbol($"{str[i + 1]}");
                    to = i + 3;
                    return result;
                }
            }
            return null;
        }

        /// <summary>
        /// Tokenize string in double quotes
        /// </summary>
        /// <param name="str"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        TokenString StringTokenize(string str, int from, out int to)
        {
            to = from;
            if (str[from] == '"')
            {
                StringBuilder result = new StringBuilder();
                bool ok = false;
                int i = from + 1;
                for (; i < str.Length; i++)
                {
                    if (i < str.Length - 1 && str[i] == '\\' && str[i + 1] == '"')
                    {
                        result.Append('"');
                        i++;
                        continue;
                    }
                    else if (str[i] == '"')
                    {
                        ok = true;
                        break;
                    }
                    else result.Append(str[i]);
                }
                if (ok)
                {
                    to = i + 1;
                    return new TokenString(result.ToString());
                }
            }
            return null;
        }

        /// <summary>
        /// Tokenizes id in parsed result ($uint)
        /// </summary>
        /// <param name="str"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        TokenVarId VarIdTokenize(string str, int from, out int to)
        {
            if (str[from] == '$')
            {
                uint res = 0;
                int i = from + 1;
                bool ok = true;
                for (; i < str.Length; i++)
                {
                    if (char.IsDigit(str[i]))
                        res = (uint)(res * 10 + (str[i] - '0'));
                    else if (separators.Contains(str[i]))
                    {
                        break;
                    }
                    else
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok)
                {
                    to = i;
                    return new TokenVarId(res);
                }
            }
            to = from;
            return null;
        }
    }
}

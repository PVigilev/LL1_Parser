using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LL1_Parser.Bootstrap
{

#if DEBUG
    public
#endif

    class Lexer : ILexer<AbstractToken>
    {
        HashSet<char> separators = new HashSet<char> { ' ', '.', ',', ':', '(', ')', '{', '}', '\n', '\r', '"' };
        Dictionary<string, TokenKeyWord> keywords = new Dictionary<string, TokenKeyWord> { { "new", TokenKeyWord.Instances[TokenKeyWord.KeyWordType.newKW] }, {"static", TokenKeyWord.Instances[TokenKeyWord.KeyWordType.staticKW] } };
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
                foreach(var ft in Tokenizers)
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
                default:  break;
            }
            if(result == null)
            {
                to = from;
            }
            else
            {
                to = from + 1;
            }
            return result;
        }
        
        AbstractToken NumberTokenize(string str, int from, out int to)
        {
            if (str[from] == '-'||char.IsDigit(str[from])) {
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

        //TokenDouble
        TokenDouble DoubleTokenize(string str)
        {
            double res;
            if (!double.TryParse(str, out res))
                return null;
            return new TokenDouble(res);
        }
        //TokenInt
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
        AbstractToken WordTokenize(string str, int from, out int to)
        {
            to = from;
            if(str[from] == '_' || char.IsLetter(str[from]))
            {
                int i = from;
                bool ok = true;
                while (i < str.Length)
                {
                    if (separators.Contains(str[i]))
                        break;
                    if(!(str[i] == '_' || char.IsLetterOrDigit(str[i])))
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
                        return new TokenName(res);                        
                }

            }
            return null;
        }

        //TokenString
        TokenString StringTokenize(string str, int from, out int to)
        {
            to = from;
            if(str[from] == '"')
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
        
        //TokenVarId
        TokenVarId VarIdTokenize(string str, int from, out int to)
        {
            if(str[from] == '$')
            {
                uint res = 0;
                int i = from + 1;
                bool ok = true;
                for(; i < str.Length; i++)
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

    /*/
    /// <summary>
    /// Convert string of symbols into the list of tokens.
    /// Now it needs whitespaces between some tokens
    /// </summary>
    class Lexer : ILexer<AbstractToken>
    {
        public IList<AbstractToken> Tokenize(string str)
        {
            int i = 0;
            List<AbstractToken> tokens = new List<AbstractToken>();
            while(i < str.Length)
            {
                if (char.IsWhiteSpace(str[i]))
                {
                    i++;
                    continue;
                }
                int next;
                AbstractToken tkn = SimpleTokenize(str, i, out next);
                if (tkn == null)
                    tkn = ValueTokenize(str, i, out next);
                if (tkn == null)
                    throw new ParserUnknownTokenException($"Unknown token on position {i}: {str.Substring(i)}");
                i = next;
                tokens.Add(tkn);
            }
            return tokens;
        }
        private SimpleToken SimpleTokenize(string str, int from, out int to)
        {
            SimpleToken res = null;
            if (str[from] == ':')
                res = SimpleToken.Instances[SimpleToken.TokenType.column];
            else if (str[from] == ';')
                res = SimpleToken.Instances[SimpleToken.TokenType.semicolumn];
            else if (str[from] == '(')
                res = SimpleToken.Instances[SimpleToken.TokenType.OB];
            else if (str[from] == ')')
                res = SimpleToken.Instances[SimpleToken.TokenType.CB];
            else if (str[from] == '{')
                res = SimpleToken.Instances[SimpleToken.TokenType.OCB];
            else if (str[from] == '}')
                res = SimpleToken.Instances[SimpleToken.TokenType.CCB];
            else if (str[from] == '.')
                res = SimpleToken.Instances[SimpleToken.TokenType.dot];
            else if (str[from] == '*')
                res = SimpleToken.Instances[SimpleToken.TokenType.star];
            else if (str[from] == ',')
                res = SimpleToken.Instances[SimpleToken.TokenType.comma];
            if (res != null)
                to = from + 1;
            else
            {
                if (KeyWordTokenize("static", str, from, out to, true))
                    res = SimpleToken.Instances[SimpleToken.TokenType.staticKeyWord];
                else if (KeyWordTokenize("new", str, from, out to, true))
                    res = SimpleToken.Instances[SimpleToken.TokenType.newKeyWord];
            }
            return res;
        }

        private AbstractToken ValueTokenize(string str, int from, out int to)
        {
            AbstractToken result = null;
            if(from < str.Length)
            {
                if (VarIdTokenize(out result, str, from, out to)) { }
                else if (StringTokenize(out result, str, from, out to)) { }
                else if (IntTokenize(out result, str, from, out to)) { }
                else if (DoubleTokenize(out result, str, from, out to)) { }
                else if (NameTokenize(out result, str, from, out to)) { }
            }
            to = from;
            return result;
        }

        /// <summary>
        /// Find if "str" starts with keyword on position "from" and finds where this keyword ends. 
        /// Simple Finite-State automat
        /// </summary>
        /// <param name="KeyWord">KeyWord to find</param>
        /// <param name="str">The string where we find an occurence</param>
        /// <param name="from">Position where the algorithm starts</param>
        /// <param name="to">Position of the symbol after the last symbol in the KeyWord. to == from if the KW didn't accept</param>
        /// <param name="endWithWhitespace"> if true, then automat accept the string iff the symbol after that is whitespace. (good for keywords such as "new" or "static"
        /// If false, then the automat accept the keyword on any case</param>
        /// <returns></returns>
        private bool KeyWordTokenize(string KeyWord, string str, int from, out int to, bool endWithWhitespace)
        {
            to = from;
            if (KeyWord.Length < str.Length - from)
            {
                return false;
            }
            for(int i = 0; i < KeyWord.Length; i++)
            {
                if(KeyWord[i] != str[from + i])
                {
                    to = from + i;
                    return false;
                }
            }
            if (endWithWhitespace)
            {
                if (char.IsWhiteSpace(str[from + KeyWord.Length]))
                {
                    to = from + KeyWord.Length;
                    return true;
                }
                else
                    return false;
            }
            else
            {
                to = from + KeyWord.Length;
                return true;
            }
            
        }

        /// <summary>
        /// Decide if the "str" starts with some string-token
        /// if doesn't have to end with a whitespace
        /// </summary>
        /// <param name="Result"></param>
        /// <param name="str"></param>
        /// <param name="from"></param>
        /// <param name="to"> The symbol after the closing-'"'</param>
        /// <returns></returns>
        private bool StringTokenize(out AbstractToken Result, string str, int from, out int to)
        {
            to = from;
            Result = null;
            if(str[from] == '"')
            {
                int i = from + 1;
                bool ended = false;
                for (; i < str.Length - 1; i++) 
                {
                    if (str[i] == '"')
                    {
                        ended = true;
                        break;
                    }
                    if (str[i] == '\\' && str[i + 1] == '"')
                        i++;
                }
                if (!ended)
                    return false;
                to = i + 1;
                if (i-from - 1 == 0)
                    Result = new TokenString("");
                else
                    Result = new TokenString(str.Substring(from + 1, i - (from + 1)));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Chech if the string starts with some symbol-name, i.e. starts with a letter or '_' end contains only letters, '_' and numbers
        /// It doesn't have to ens with a whitespace
        /// </summary>
        /// <param name="result"></param>
        /// <param name="str"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private bool NameTokenize(out AbstractToken result, string str, int from, out int to)
        {
            to = from;
            result = null;
            if(char.IsLetter(str[from]) || str[from] == '_')
            {
                int i = from;
                bool accepted = false;
                for(; i< str.Length; i++)
                {
                    if (!(char.IsLetterOrDigit(str[i]) || str[i] == '_'))
                        accepted = true;
                }
                if (accepted)
                {
                    to = i;
                    result = new TokenName(str.Substring(from, to - from + 1));
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if str starts with some integer and has whitespace after it
        /// </summary>
        /// <param name="result"></param>
        /// <param name="str"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private bool IntTokenize(out AbstractToken result, string str, int from, out int to)
        {
            if (str[from] == '-' || char.IsDigit(str[from]))
            {
                int i;
                for (i = from; i < str.Length; i++)
                {
                    if (char.IsWhiteSpace(str[i]))
                        break;
                }
                int val;
                if (int.TryParse(str.Substring(from, i - from), out val))
                {
                    to = i;
                    result = new TokenInt(val);
                    return true;
                }
            }
            to = from;
            result = default(TokenInt);
            return false;
        }

        private bool DoubleTokenize(out AbstractToken result, string str, int from, out int to)
        {
            if (str[from] == '-' || str[from] == '.' || char.IsDigit(str[from]))
            {
                int i;
                for (i = from; i < str.Length; i++)
                {
                    if (char.IsWhiteSpace(str[i]))
                        break;
                }
                double val;
                if (double.TryParse(str.Substring(from, i - from), out val))
                {
                    to = i;
                    result = new TokenDouble(val);
                    return true;
                }
            }
            to = from;
            result = default(TokenDouble);
            return false;
        }

        private bool VarIdTokenize(out AbstractToken result, string str, int from, out int to)
        {
            if(str[from] == '$')
            {
                int i = from + 1;
                for (i = from; i < str.Length; i++)
                {
                    if (char.IsWhiteSpace(str[i]))
                        break;
                }
                uint val;
                if (uint.TryParse(str.Substring(from+1, i - from-1), out val))
                {
                    to = i;
                    result = new TokenVarId(val);
                    return true;
                }
            }
            result = default(TokenVarId);
            to = from;
            return false;
        }
    }

    /**/
}

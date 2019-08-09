﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MFFParser;
using MFFParser.Bootstrap;
using System.Collections.Generic;

namespace BasicTests
{
    [TestClass]
    public class BootstrapingTests
    {
        /*/
        [TestMethod]
        public void CreatingSpecialGrammarTest()
        {
            Grammar grm = Grammar.SpecialGrammar;
            try
            {
                if (grm == null)
                    Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
        /**/
        private bool CompatiableTokenSequence(IList<AbstractToken> tokens, IList<AbstractToken> expected)
        {
            if (tokens.Count != expected.Count)
                return false;
            for (int i = 0; i < tokens.Count; i++)
            {
                if (!expected[i].IsCompatible(tokens[i]))
                    return false;
            }
            return true;
        }


        [TestMethod]
        public void BootstrapLexerTest1()
        {
            string str = "\"string name1\" name2 : .)({}'}'{123.32 12343 $123 _name3 new static newstatic " + Environment.NewLine + "static_new -123";
            var expected = new List<AbstractToken>(new AbstractToken[] {
            new TokenString("string name1"), new TokenSymbol("name2"),
            SimpleToken.Instances[SimpleToken.TokenType.column],
            SimpleToken.Instances[SimpleToken.TokenType.dot],
            SimpleToken.Instances[SimpleToken.TokenType.CB], SimpleToken.Instances[SimpleToken.TokenType.OB],
            SimpleToken.Instances[SimpleToken.TokenType.OCB], SimpleToken.Instances[SimpleToken.TokenType.CCB],
            new TokenSymbol("}"),
            SimpleToken.Instances[SimpleToken.TokenType.OCB],
            new TokenDouble(123.32), new TokenInt(12343), new TokenVarId(123),
            new TokenSymbol("_name3"), TokenKeyWord.Instances[TokenKeyWord.KeyWordType.newKW],
            TokenKeyWord.Instances[TokenKeyWord.KeyWordType.staticKW],
            new TokenSymbol("newstatic"), new TokenSymbol("static_new"), new TokenInt(-123)}) ;

            Lexer lexer = new Lexer();
            var tokens = lexer.Tokenize(str);
            if (!CompatiableTokenSequence(tokens, expected))
                Assert.Fail();
        }

        
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace MFFParser
{
    abstract class GrammarChecker
    {
        public abstract bool Check(Grammar grammar, bool throw_on_false);
    }
    
    
}

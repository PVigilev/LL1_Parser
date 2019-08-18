namespace LL1_Parser
{
    abstract class GrammarChecker
    {
        public abstract bool Check(Grammar grammar, bool throw_on_false);
    }
}

namespace LL1_Parser
{
    /// <summary>
    /// Abstract class that represents some grammar checker 
    /// that can check a grammar for some condition
    /// </summary>
    abstract class GrammarChecker
    {
        /// <summary>
        /// Checks a grammar for some condition
        /// </summary>
        /// <param name="grammar"></param>
        /// <param name="throw_on_false">throws an exception if true</param>
        /// <returns>true if the grammar satisfy some condition</returns>
        public abstract bool Check(Grammar grammar, bool throw_on_false);
    }
}

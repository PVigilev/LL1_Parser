namespace LL1_Parser
{
    /// <summary>
    /// Interface for user-defined data structure that represents a token
    /// </summary>
    public interface IToken
    {
        /// <summary>
        /// Checks if this instance of token is compatible with other. 
        /// Uses during parsing to check if a token can be used as a terminal symbol that keeps some other token inside itself
        /// </summary>
        /// <param name="other"></param>
        /// <returns>true if "other" is the same type with a current instance </returns>
        bool IsCompatible(IToken other);
    }

    /// <summary>
    /// Interface for user-defined data structure that represents a token with a value inside
    /// </summary>
    /// <typeparam name="T">type of value that the token represents. This type is covariant</typeparam>
    public interface ITokenWithValue<out T> : IToken where T : class
    {
        /// <summary>
        /// Value of type T that this instance represents
        /// </summary>
        T Value { get; }
    }
}

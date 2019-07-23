using System;
using System.Collections.Generic;
using System.Text;

namespace LL1_Parser
{
    /// <summary>
    /// Base exception class for parser errors
    /// </summary>
    public class ParserErrorException : Exception
    {
        public ParserErrorException() : base() { }
        public ParserErrorException(string message) : base(message) { }
        public ParserErrorException(string message, Exception InnerException) : base(message, InnerException) { }
        public ParserErrorException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    public class ParserUnknownTokenException : ParserErrorException
    {
        public ParserUnknownTokenException() : base() { }
        public ParserUnknownTokenException(string message) : base(message) { }
        public ParserUnknownTokenException(string message, Exception InnerException) : base(message, InnerException) { }
        public ParserUnknownTokenException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// Exception that is thrown when the type error occurs during using reflection and types are not compatiable
    /// </summary>
    public class ParserTypeErrorException : ParserErrorException
    {
        public ParserTypeErrorException() : base() { }
        public ParserTypeErrorException(string message) : base(message) { }
        public ParserTypeErrorException(string message, Exception InnerException) : base(message, InnerException) { }
        public ParserTypeErrorException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    /// <summary>
    /// Exception that is thrown when there expression of successed parsing returns bad value
    /// </summary>
    public class ParserReturnValueException : ParserTypeErrorException
    {
        public ParserReturnValueException() : base() { }
        public ParserReturnValueException(string message) : base(message) { }
        public ParserReturnValueException(string message, Exception InnerException) : base(message, InnerException) { }
        public ParserReturnValueException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// Exception that throws when there is an error with arguments of some method and method can not be invoked using reflection
    /// </summary>
    public class ParserArgumentErrorException : ParserErrorException
    {
        public ParserArgumentErrorException() : base() { }
        public ParserArgumentErrorException(string message) : base(message) { }
        public ParserArgumentErrorException(string message, Exception InnerException) : base(message, InnerException) { }
        public ParserArgumentErrorException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// All types of type errors in the parser logic
    /// </summary>
    public class TypeNotFoundException : ParserErrorException
    {
        public TypeNotFoundException() : base() { }
        public TypeNotFoundException(string message) : base(message) { }
        public TypeNotFoundException(string message, Exception InnerException) : base(message, InnerException) { }
        public TypeNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    /// <summary>
    /// Throws when parser is not able to find a method using reflection
    /// </summary>
    public class MethodNotFoundException : ParserErrorException
    {
        public MethodNotFoundException() : base() { }
        public MethodNotFoundException(string message) : base(message) { }
        public MethodNotFoundException(string message, Exception InnerException) : base(message, InnerException) { }
        public MethodNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    /// <summary>
    /// Base exception class for grammar errors
    /// </summary>
    public class GrammarException : ParserErrorException
    {
        public GrammarException() : base() { }
        public GrammarException(string message) : base(message) { }
        public GrammarException(string message, Exception InnerException) : base(message, InnerException) { }
        public GrammarException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public class AmbiguousGrammarException : GrammarException
    {
        public AmbiguousGrammarException() : base() { }
        public AmbiguousGrammarException(string message) : base(message) { }
        public AmbiguousGrammarException(string message, Exception InnerException) : base(message, InnerException) { }
        public AmbiguousGrammarException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public class LeftRecursionGrammarException : GrammarException
    {
        public LeftRecursionGrammarException() : base() { }
        public LeftRecursionGrammarException(string message) : base(message) { }
        public LeftRecursionGrammarException(string message, Exception InnerException) : base(message, InnerException) { }
        public LeftRecursionGrammarException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}

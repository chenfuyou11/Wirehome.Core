using System;

namespace Wirehome.Contracts.Scripting
{
    public class ScriptingException : Exception
    {
        public ScriptingException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}

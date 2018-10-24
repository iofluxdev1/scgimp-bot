using System;

namespace StarCitizen.Gimp.Core
{
    public class RsiServiceUnavailableException : Exception
    {
        public RsiServiceUnavailableException()
        {
        }

        public RsiServiceUnavailableException(string message)
            : base(message)
        {
        }

        public RsiServiceUnavailableException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

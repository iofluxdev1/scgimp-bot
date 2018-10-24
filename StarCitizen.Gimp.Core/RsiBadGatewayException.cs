using System;
using System.Collections.Generic;
using System.Text;

namespace StarCitizen.Gimp.Core
{
    public class RsiBadGatewayException : Exception
    {
        public RsiBadGatewayException()
        {
        }

        public RsiBadGatewayException(string message)
            : base(message)
        {
        }

        public RsiBadGatewayException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

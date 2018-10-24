using System;

namespace StarCitizen.Gimp.Core
{
    public class OutOfStockStoreItemException : Exception
    {
        public OutOfStockStoreItemException()
        {
        }

        public OutOfStockStoreItemException(string message)
            : base(message)
        {
        }

        public OutOfStockStoreItemException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

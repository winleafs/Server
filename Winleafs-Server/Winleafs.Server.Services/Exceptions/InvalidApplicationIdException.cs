using System;

namespace Winleafs.Server.Services.Exceptions
{
    public class InvalidApplicationIdException : Exception
    {
        public InvalidApplicationIdException(string message) : base(message)
        {

        }
    }
}

using System;

namespace Winleafs.Server.Services.Exceptions
{
    /// <summary>
    /// Thrown when a user supplies an unknown/invalid application id.
    /// </summary>
    public class InvalidApplicationIdException : Exception
    {
        public InvalidApplicationIdException(string message) : base(message)
        {

        }
    }
}

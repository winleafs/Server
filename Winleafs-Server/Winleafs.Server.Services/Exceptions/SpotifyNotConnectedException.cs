using System;

namespace Winleafs.Server.Services.Exceptions
{
    public class SpotifyNotConnectedException : Exception
    {
        public SpotifyNotConnectedException(string message) : base(message)
        {

        }
    }
}

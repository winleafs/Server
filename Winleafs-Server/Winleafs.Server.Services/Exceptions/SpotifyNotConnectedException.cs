using System;

namespace Winleafs.Server.Services.Exceptions
{
    /// <summary>
    /// Thrown when the user is not connected to Spotify
    /// </summary>
    public class SpotifyNotConnectedException : Exception
    {
        public SpotifyNotConnectedException(string message) : base(message)
        {

        }
    }
}

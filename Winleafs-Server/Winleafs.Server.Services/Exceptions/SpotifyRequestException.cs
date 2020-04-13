using System;

namespace Winleafs.Server.Services.Exceptions
{
    /// <summary>
    /// Thrown when Spotify API does not return status code 200 OK
    /// </summary>
    public class SpotifyRequestException : Exception
    {
        public SpotifyRequestException(string message) : base(message)
        {

        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Winleafs.Server.Models.Models;

namespace Winleafs.Server.Services.Interfaces
{
    public interface ISpotifyService
    {
        /// <summary>
        /// Swaps an authorization code for an access token and
        /// saves the token information at the user represented by
        /// the given <paramref name="applicationId"/>.
        /// </summary>
        /// <param name="code">
        /// The Spotify authorization code.
        /// </param>
        /// <param name="applicationId">
        /// The application Id belonging to the authorization code.
        /// </param>
        Task SwapToken(string code, string applicationId);

        /// <summary>
        /// Refreshes the Spotify access token for the
        /// given <paramref name="user"/>.
        /// </summary>
        /// <param name="user">
        /// The user to refresh the Spotify access token for.
        /// </param>
        Task RefreshToken(User user);

        /// <summary>
        /// Retrieves the current playing playlist id of the
        /// Spotify account connected to the given
        /// <paramref name="applicationId"/>.
        /// </summary>
        /// <returns>
        /// The current playing playlist id or null if the user
        /// is not playing anything.
        /// </returns>
        Task<string> GetCurrentPlayingPlaylistId(string applicationId);

        /// <summary>
        /// Retrieves the ids and names of the playlists of the
        /// Spotify account connected to the given
        /// <paramref name="applicationId"/>.
        /// </summary>
        Task<Dictionary<string, string>> GetPlaylists(string applicationId);

        /// <summary>
        /// Deletes all Spotify related information for the given
        /// <paramref name="applicationId"/>. Does not throw an error
        /// if the user does not exist.
        /// </summary>
        Task Disconnect(string applicationId);

        /// <summary>
        /// Checks if the user associated to the given
        /// <paramref name="applicationId"/> is connected to Spotify.
        /// </summary>
        /// <returns>
        /// False if no user exists, or Spotify is not connected, true otherwise.
        /// </returns>
        Task<bool> IsConnected(string applicationId);
    }
}

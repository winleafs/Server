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
        /// Retrieves the names of the palylists of the
        /// Spotify account connected to the given
        /// <paramref name="applicationId"/>.
        /// </summary>
        Task<IEnumerable<string>> GetPlaylistNames(string applicationId);
    }
}

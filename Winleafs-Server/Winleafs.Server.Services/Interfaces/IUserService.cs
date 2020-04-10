using System.Threading.Tasks;
using Winleafs.Server.Models.Models;

namespace Winleafs.Server.Services.Interfaces
{
    public interface IUserService : IBaseService<User>
    {
        /// <summary>
        /// Adds an user with the given <paramref name="applicationId"/>
        /// if it does not exist.
        /// </summary>
        /// <returns>
        /// The existing or newly created user.
        /// </returns>
        Task<User> AddUserIfNotExists(string applicationId);

        Task<User> FindUserByApplicationId(string applicationId);
    }
}

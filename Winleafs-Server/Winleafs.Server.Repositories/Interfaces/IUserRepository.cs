using System.Threading.Tasks;
using Winleafs.Server.Models.Models;

namespace Winleafs.Server.Repositories.Interfaces
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User> FindUserByApplicationId(string applicationId);
    }
}

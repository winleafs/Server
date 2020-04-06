using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Winleafs.Server.Models.Models;
using Winleafs.Server.Repositories.Interfaces;

namespace Winleafs.Server.Repositories
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User> FindUserByApplicationId(string applicationId);
    }

    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(DbContext context) : base(context)
        {

        }

        public Task<User> FindUserByApplicationId(string applicationId)
        {
            return Queryable().FirstOrDefaultAsync(user => user.ApplicationId == applicationId);
        }
    }
}

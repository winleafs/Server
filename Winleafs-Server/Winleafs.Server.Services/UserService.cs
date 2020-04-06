using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Winleafs.Server.Models.Models;
using Winleafs.Server.Repositories;
using Winleafs.Server.Services.Interfaces;

namespace Winleafs.Server.Services
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

    public class UserService : BaseService<User>, IUserService
    {
        private IUserRepository _repository;
        private DbContext _context;

        public UserService(IUserRepository repository, DbContext context) : base(repository)
        {
            _repository = repository;
            _context = context;
        }

        public async Task<User> AddUserIfNotExists(string applicationId)
        {
            var user = await _repository.FindUserByApplicationId(applicationId);

            if (user == null)
            {
                user = new User
                {
                    ApplicationId = applicationId
                };

                await _repository.AddAsync(user);
                await _context.SaveChangesAsync();
            }

            return user;
        }

        public Task<User> FindUserByApplicationId(string applicationId)
        {
            return _repository.FindUserByApplicationId(applicationId);
        }
    }
}

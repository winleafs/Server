using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Template.Models.Interfaces;
using Template.Repositories.Interfaces;

namespace Template.Repositories
{
    public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity>
        where TEntity : class, IEntity
    {
        private readonly DbSet<TEntity> _dbSet;

        /// <summary>
        ///     Creates a new instance of the <see cref="BaseRepository{TEntity}" /> class.
        /// </summary>
        /// <param name="context">The context wanting to be used for operations.</param>
        protected BaseRepository(DbContext context)
        {
            _dbSet = context.Set<TEntity>();
        }

        /// <inheritdoc />
        public TEntity Add(TEntity entity)
        {
            CheckGivenEntity(entity);

            return _dbSet.Add(entity).Entity;
        }

        /// <inheritdoc />
        public async Task<TEntity> AddAsync(TEntity entity)
        {
            CheckGivenEntity(entity);

            return (await _dbSet.AddAsync(entity)).Entity;
        }

        /// <inheritdoc />
        public void Update(TEntity entity)
        {
            CheckGivenEntity(entity);

            _dbSet.Update(entity);
        }

        /// <inheritdoc />
        public TEntity Find(long id)
        {
            CheckGivenId(id);

            return _dbSet.Find(id);
        }

        /// <inheritdoc />
        public ValueTask<TEntity> FindAsync(long id)
        {
            CheckGivenId(id);

            return _dbSet.FindAsync(id);
        }

        /// <inheritdoc />
        public void Delete(TEntity entity)
        {
            CheckGivenEntity(entity);

            _dbSet.Remove(entity);
        }

        /// <summary>
        ///     Creates a new <see cref="IQueryable{T}" /> instance.
        /// </summary>
        /// <returns>A new instance of <see cref="IQueryable{T}" /></returns>
        protected IQueryable<TEntity> Queryable()
        {
            return _dbSet.AsQueryable();
        }

        /// <summary>
        ///     Checks if the given
        ///     <param name="entity"> is null.</param>
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity" /> is null.</exception>
        /// <param name="entity">The entity wanting to be checked.</param>
        protected void CheckGivenEntity(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
        }

        /// <summary>
        ///     Checks if the given
        ///     <param name="id"> is a valid value.</param>
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="id" /> is invalid.</exception>
        /// <param name="id"></param>
        protected void CheckGivenId(long id)
        {
            if (id < 1) throw new ArgumentException(nameof(id));
        }
    }
}
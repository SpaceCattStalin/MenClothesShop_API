using System.Linq.Expressions;

namespace Common.Interfaces
{
    public interface IRepository<T> where T : class, IEntity
    {
        IQueryable<T> Get(Expression<Func<T, bool>> filter, params string[] includes);
    }
}

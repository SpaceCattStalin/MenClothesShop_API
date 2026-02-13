//using Common.Interfaces;
//using Microsoft.EntityFrameworkCore;
//using Repositories.ApplicationDbContext;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;

//namespace Repositories
//{
//    public class Repository<T> : IRepository<T> where T : class, IEntity
//    {
//        private readonly AppDbContext _dbContext;

//        public Repository(Dictionary<Type, Func<AppDbContext>> dbContextLookup)
//        {
//            _dbContext = dbContextLookup[typeof(T)].Invoke();
//        }

//        public IQueryable<T> Get(Expression<Func<T, bool>> filter, params string[] includes)
//        {
//            filter ??= (t => true);
//            var query = _dbContext.Set<T>().Where(filter);
//            foreach (var include in includes ?? [])
//            {
//                query = query.Include(include);
//            }
//            return query;
//        }

//        public IQueryable<T> Get(Expression<Func<T, bool>> filter, params string[] includes)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using WhiteHotel.Domain.Entities;


namespace WhiteHotel.Application.Common.Interfaces
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperites = null, bool tracked = false);
        T Get(Expression<Func<T, bool>> filter, string? includeProperites = null, bool tracked = false);
        void Add(T entity);
        bool Any(Expression<Func<T, bool>>? filter);
        void Remove(T entity);
       

    }
}

using System.Linq.Expressions;
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Core.Common;
using Ecommerce.Infrastructure.Persistence;
using Ecommerce.Infrastructure.Persistence.Identity;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Repository;

public class EfRepository<T> : IEfRepository<T> where T : BaseEntity
{
    private readonly IDbContext _db;
    internal DbSet<T> dbset;

    public EfRepository(IDbContext db)
    {
        _db = db;
        this.dbset = _db.Set<T>();
    }

    public IEnumerable<T> GetAll(Expression<Func<T, bool>> Filter = null!, string IncludeProperty = null!)
    {
        IQueryable<T> query = dbset;
        if (Filter is not null)
        {
            query = query.Where(Filter);
        }
        if (IncludeProperty is not null)
        {
            foreach (var prop in IncludeProperty.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(prop);
            }
        }
        return query.ToList();
    }

    public T GetFirst(Expression<Func<T, bool>> Filter = null!, string IncludeProperty = null!)
    {
        IQueryable<T> query = dbset;
        if (Filter is not null)
        {
            query = query.Where(Filter);
        }
        if (IncludeProperty is not null)
        {
            foreach (var prop in IncludeProperty.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(prop);
            }
        }
        return query.FirstOrDefault()!;
    }
    public async Task<T> AddAsync(T entity)
    {
        await dbset.AddAsync(entity);
        await SaveChangeAsync();
        return entity;
    }
    public async Task<bool> AddRangeAsync(IEnumerable<T> entities)
    {
        await dbset.AddRangeAsync(entities);
        var result = await SaveChangeAsync();
        if (result < 1) return false;
        return true;
    }

    public async Task RemoveAsync(int id)
    {
        var entity = await dbset.FindAsync(id);
        dbset.Remove(entity!);
    }

    public void Remove(T entity)
    {
        dbset.Remove(entity);
    }

    public void RemoveRange(IEnumerable<T> entities)
    {
        dbset.AttachRange(entities);
        dbset.RemoveRange(entities);
    }

    public void Update(T entity)
    {
        dbset.Update(entity);
    }

    public async Task<int> SaveChangeAsync()
    {
        return await _db.SaveChangesAsync();
    }
}
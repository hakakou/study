using Android.Content;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;

namespace MauiApp4;

public class CrmContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "crm2.db");
        optionsBuilder.UseSqlite($"Filename={dbPath}");

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // 
        builder.Entity<Customer>().HasData(
            new Customer
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Phone = "123-456-7890",
                IsActive = true
            }
        );
    }
}

[INotifyPropertyChanged]
public partial class Customer
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    [ObservableProperty]
    public partial bool IsActive { get; set; }
}

public interface IRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(int id);
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task AddAsync(TEntity item);
    Task UpdateAsync(TEntity item);
    Task DeleteAsync(TEntity item);
}

public interface IRepository2<TEntity> : System.Data.Entity.IDbSet<TEntity> where TEntity : class
{
}

public class CustomerRepository : IRepository<Customer>
{
    readonly DbSet<Customer> DbSet;
    readonly CrmContext Context;

    public CustomerRepository(CrmContext context)
    {
        Context = context;
        DbSet = context.Set<Customer>();
    }

    Task<IEnumerable<Customer>> IRepository<Customer>.GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Customer>>(DbSet);
    }

    public async Task AddAsync(Customer item)
    {
        DbSet.Add(item);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Customer item)
    {
        DbSet.Remove(item);
        await Task.CompletedTask;
    }

    public Task<Customer?> GetByIdAsync(int id)
    {
        return DbSet.FindAsync(id).AsTask();
    }

    public async Task UpdateAsync(Customer item)
    {
        DbSet.Attach(item);
        Context.Entry(item).State = EntityState.Modified;
        await Task.CompletedTask;
    }
}

public interface IUnitOfWork<TEntity> where TEntity : class
{
    IRepository<TEntity> Items { get; }
    Task<int> SaveAsync();
}

public class CrmUnitOfWork : IUnitOfWork<Customer>, IDisposable
{
    readonly CrmContext Context;

    IRepository<Customer> customerRepository;

    public CrmUnitOfWork(CrmContext context)
    {
        Context = context;
    }

    public IRepository<Customer> Items => customerRepository ??= new CustomerRepository(Context);

    public Task<int> SaveAsync()
    {
        return Context.SaveChangesAsync();
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}
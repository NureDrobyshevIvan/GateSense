namespace Infrastructure.Data.UnitOfWork;

public interface IUnitOfWork : IAsyncDisposable
{
    public Task BeginTransactionAsync();
    
    public Task CommitTransactionAsync();
    
    public Task RollbackTransactionAsync();
    
    public Task SaveChangesAsync();
}
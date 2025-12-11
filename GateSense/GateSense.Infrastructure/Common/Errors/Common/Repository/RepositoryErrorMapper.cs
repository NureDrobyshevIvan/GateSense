using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Common.Errors.Common.Repository;

public static class RepositoryErrorMapper<T>
{
    public static Error Map(DbUpdateException ex)
    {
        // Concurrency errors
        if (ex is DbUpdateConcurrencyException)
        {
            return RepositoryErrors<T>.UpdateError;
        }
        
        if (ex.InnerException is SqlException sqlEx)
        {
            switch (sqlEx.Number)
            {
                // Unique constraints
                case 2627: // Violation of PRIMARY KEY or UNIQUE constraint
                case 2601: // Cannot insert duplicate key row
                    return RepositoryErrors<T>.AddError;
                // Foreign key constraints
                case 547:  
                    if (sqlEx.Message.Contains("DELETE", StringComparison.OrdinalIgnoreCase))
                    {
                        return RepositoryErrors<T>.DeleteError;
                    }
                    return RepositoryErrors<T>.UpdateError;
                default:
                    return RepositoryErrors<T>.UpdateError;
            }
        }

        return RepositoryErrors<T>.UpdateError;
    }
}
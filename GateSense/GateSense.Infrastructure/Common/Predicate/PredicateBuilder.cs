using System.Linq.Expressions;

namespace Infrastructure.Common.Predicate;

public class PredicateBuilder<T>
{
    private Expression<Func<T, bool>> _predicate;

    public PredicateBuilder()
    {
        _predicate = x => true; 
    }

    public void And(Expression<Func<T, bool>> expr)
    {
        _predicate = _predicate.And(expr); 
    }

    public void Or(Expression<Func<T, bool>> expr)
    {
        _predicate = _predicate.Or(expr); 
    }

    public void Not(Expression<Func<T, bool>> expr)
    {
        var notExpr = Expression.Lambda<Func<T, bool>>(
            Expression.Not(expr.Body), expr.Parameters);
        _predicate = _predicate.And(notExpr);
    }

    public Expression<Func<T, bool>> Build() => _predicate;
}

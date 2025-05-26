using System.Linq.Expressions;

namespace Codout.Framework.Common.Extensions;

/// <summary>
/// Extensões comuns para tipos relacionadas a Linq.
/// </summary>
public static class Linq
{
    #region ParseObjectValue
    /// <summary>
    /// Parses the object value.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <returns></returns>
    public static string ParseObjectValue(this LambdaExpression expression)
    {
        var result = string.Empty;

        if (expression.Body is MemberExpression body)
        {
            result = body.Member.Name;
        }
        else if (expression.Body.NodeType == ExpressionType.Convert)
        {
            if (expression.Body is UnaryExpression { Operand: MemberExpression m })
                result = m.Member.Name;
        }
        return result;
    }
    #endregion

    #region IsConstraint
    /// <summary>
    /// Determines whether the specified exp is constraint.
    /// </summary>
    /// <param name="exp">The exp.</param>
    /// <returns>
    /// 	<c>true</c> if the specified exp is constraint; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsConstraint(this Expression exp)
    {
        if (exp is not BinaryExpression binary)
            return false;

        var binExpType = typeof(BinaryExpression);

        return binary.Left.GetType() != binExpType && binary.Right.GetType() != binExpType;
    }
    #endregion

    #region GetConstantValue
    /// <summary>
    /// Gets the constant value.
    /// </summary>
    /// <param name="exp">The exp.</param>
    /// <returns></returns>
    public static object GetConstantValue(this Expression exp)
    {
        object result = null;
        if (exp is ConstantExpression expression)
            result = expression.Value;
        return result;
    }
    #endregion
}

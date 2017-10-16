using System;
using System.Linq.Expressions;

namespace Codout.Framework.Common.Extensions
{
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
            string result = String.Empty;
            if (expression.Body is MemberExpression)
            {
                var m = expression.Body as MemberExpression;
                result = m.Member.Name;
            }
            else if (expression.Body.NodeType == ExpressionType.Convert)
            {
                var u = expression.Body as UnaryExpression;
                if (u != null)
                {
                    var m = u.Operand as MemberExpression;
                    if (m != null)
                        result = m.Member.Name;
                }
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
            bool result = false;
            if (exp is BinaryExpression)
            {
                var binary = exp as BinaryExpression;
                Type binExpType = typeof(BinaryExpression);
                result = binary.Left.GetType() != binExpType && binary.Right.GetType() != binExpType;
            }
            return result;
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
            if (exp is ConstantExpression)
            {
                var c = (ConstantExpression)exp;
                result = c.Value;
            }
            return result;
        }
        #endregion
    }
}

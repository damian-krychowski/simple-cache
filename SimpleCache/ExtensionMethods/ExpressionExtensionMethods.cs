using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCache.ExtensionMethods
{
    internal static class ExpressionExtensionMethods
    {
        /// <summary>
        /// Retruns Func responsible for returnig value of the indicated property converted to string.
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="_obj">Object to extract property value from</param>
        /// <param name="_propertyName">Name of the property to extract</param>
        /// <returns></returns>
        public static Func<object, string> GetPropertyToStringGetter(this PropertyInfo _property)
        {
            MethodInfo toStringMethod = typeof(object).GetMethod("ToString", Type.EmptyTypes);

            ParameterExpression arg = Expression.Parameter(typeof(object), "x");
            UnaryExpression argConverted = Expression.Convert(arg, _property.DeclaringType);
            MemberExpression property = Expression.Property(argConverted, _property);

            if (_property.PropertyType == typeof(string))
            {
                return Expression.Lambda<Func<object, string>>(property, arg).Compile();
            }
            else
            {
                Expression toStringCall = Expression.Call(property, toStringMethod);
                return Expression.Lambda<Func<object, string>>(toStringCall, arg).Compile();
            }
        }

        /// <summary>
        /// Converts Expression of Func<T,OldType> int the Func<T,NewType> if the OldType items are convertiblu into NewType items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="OldType"></typeparam>
        /// <typeparam name="NewType"></typeparam>
        /// <param name="_expression"></param>
        /// <returns></returns>
        public static Expression<Func<T, NewType>> Cast<T, OldType, NewType>(this Expression<Func<T, OldType>> _expression)
        {
            try
            {
                return Expression.Lambda<Func<T, NewType>>(_expression.Body, _expression.Parameters);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Converts Expression of Func<T,OldType> int the Func<T,NewType> if the OldType items are convertiblu into NewType items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="OldType"></typeparam>
        /// <typeparam name="NewType"></typeparam>
        /// <param name="_expression"></param>
        /// <returns></returns>
        public static Expression<Func<NewType>> Cast<OldType, NewType>(this Expression<Func<OldType>> _expression)
        {
            try
            {
                return Expression.Lambda<Func<NewType>>(_expression.Body, _expression.Parameters);
            }
            catch
            {
                return null;
            }
        }

        public static bool Comapre<TArgument, TResult>(
            this Expression<Func<TArgument, TResult>> _expression, 
            Expression<Func<TArgument, TResult>> _toCompare)
        {
            if (_toCompare == null) return false;
            string original = _expression.ToString().RemoveTillFirst('.');
            string toCompare = _toCompare.ToString().RemoveTillFirst('.');
            return original == toCompare;
        }

        public static bool Comapre(
           this Expression _expression,
           Expression _toCompare)
        {
            if (_toCompare == null) return false;
            string original = _expression.ToString().RemoveTillFirst('.');
            string toCompare = _toCompare.ToString().RemoveTillFirst('.');
            return original == toCompare;
        }

        public static string GetIndicatedPropertyName<T>(this Expression<Func<T, IEnumerable>> _expression)
        {
            return _expression.Cast<T, IEnumerable, object>().GetIndicatedPropertyName();
        }

        public static string GetIndicatedPropertyName<T>(this Expression<Func<T, object>> _expression)
        {
            if (_expression.Body == null)
                throw new ArgumentNullException("propertyRefExpr", "propertyRefExpr is null.");

            MemberExpression memberExpr = _expression.Body as MemberExpression;
            if (memberExpr == null)
            {
                UnaryExpression unaryExpr = _expression.Body as UnaryExpression;
                if (unaryExpr != null && unaryExpr.NodeType == ExpressionType.Convert)
                    memberExpr = unaryExpr.Operand as MemberExpression;
            }

            if (memberExpr != null && memberExpr.Member.MemberType == MemberTypes.Property)
                return memberExpr.Member.Name;

            throw new ArgumentException("No property reference expression was found.",
                             "propertyRefExpr");
        }

        public static string GetIndicatedPropertyName<T>(this Expression<Func<T>> _expression)
        {
            if (_expression.Body == null)
                throw new ArgumentNullException("propertyRefExpr", "propertyRefExpr is null.");

            MemberExpression memberExpr = _expression.Body as MemberExpression;
            if (memberExpr == null)
            {
                UnaryExpression unaryExpr = _expression.Body as UnaryExpression;
                if (unaryExpr != null && unaryExpr.NodeType == ExpressionType.Convert)
                    memberExpr = unaryExpr.Operand as MemberExpression;
            }

            if (memberExpr != null && memberExpr.Member.MemberType == MemberTypes.Property)
                return memberExpr.Member.Name;

            throw new ArgumentException("No property reference expression was found.",
                             "propertyRefExpr");
        }

        public static Type GetIndicatedPropertyType<T>(this Expression<Func<T, object>> _expression)
        {
            if (_expression.Body == null)
                throw new ArgumentNullException("propertyRefExpr", "propertyRefExpr is null.");

            MemberExpression memberExpr = _expression.Body as MemberExpression;
            if (memberExpr == null)
            {
                UnaryExpression unaryExpr = _expression.Body as UnaryExpression;
                if (unaryExpr != null && unaryExpr.NodeType == ExpressionType.Convert)
                    memberExpr = unaryExpr.Operand as MemberExpression;
            }

            if (memberExpr != null && memberExpr.Member.MemberType == MemberTypes.Property)
                return ((PropertyInfo)memberExpr.Member).PropertyType;

            throw new ArgumentException("No property reference expression was found.",
                             "propertyRefExpr");
        }

        public static Expression<Func<T, object>> GetIndicatedPropertyOwnerExpression<T>(this Expression<Func<T, object>> _expression)
        {
            Expression reduced = null;
            MemberExpression expression = _expression.Body as MemberExpression;
            if (expression != null)
            {
                reduced = expression.Expression;
            }
            else
            {
                UnaryExpression unaryExpr = _expression.Body as UnaryExpression;
                if (unaryExpr != null && unaryExpr.NodeType == ExpressionType.Convert)
                    reduced = (unaryExpr.Operand as MemberExpression).Expression;
            }

            return Expression.Lambda<Func<T, object>>(reduced, _expression.Parameters);
        }

    }
}

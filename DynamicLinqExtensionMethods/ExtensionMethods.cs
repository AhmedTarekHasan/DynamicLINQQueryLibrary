using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DynamicLinqExtensionMethodsNS
{
    public static class ExtensionMethods
    {
        public static IQueryable<T> Where<T>(this IQueryable<T> Caller, OringGroup oringGroup)
        {
            Type ItemsType = typeof(T);
            ParameterExpression ItemTypeParameterExpression = Expression.Parameter(ItemsType, "paramDummyName");

            Expression OverAllExpression = GetCondition<T>(oringGroup, ItemTypeParameterExpression);
            return Caller.Where(Expression.Lambda<Func<T, bool>>(OverAllExpression, ItemTypeParameterExpression));
        }

        public static IQueryable<T> WhereNot<T>(this IQueryable<T> Caller, OringGroup oringGroup)
        {
            Type ItemsType = typeof(T);
            ParameterExpression ItemTypeParameterExpression = Expression.Parameter(ItemsType, "paramDummyName");

            Expression OverAllExpression = GetCondition<T>(oringGroup, ItemTypeParameterExpression);
            OverAllExpression = Expression.Not(OverAllExpression);
            return Caller.Where(Expression.Lambda<Func<T, bool>>(OverAllExpression, ItemTypeParameterExpression));
        }

        static Expression GetCondition<T>(OringGroup oringGroup, ParameterExpression ItemTypeParameterExpression)
        {
            if (oringGroup == null || oringGroup.Count == 0)
            {
                return null;
            }

            Expression OverAllExpression = MakeAndExpression<T>(oringGroup[0], ItemTypeParameterExpression);

            if (oringGroup.Count > 1)
            {
                for (var i = 1; i < oringGroup.Count; i++)
                {
                    if (oringGroup[i] == null || oringGroup[i].Count == 0)
                    {
                        continue;
                    }

                    OverAllExpression = Expression.Or(OverAllExpression, MakeAndExpression<T>(oringGroup[i], ItemTypeParameterExpression));
                }
            }

            return OverAllExpression;
        }

        static Expression MakeAndExpression<T>(AndingGroup andingGroup, ParameterExpression myParameterExpression)
        {
            if (andingGroup == null || andingGroup.Count == 0)
            {
                return null;
            }

            Type ItemsType = typeof(T);
            PropertyInfo FirstProperty = ItemsType.GetProperty(andingGroup[0].PropertyName);

            MemberExpression PropertyAccess = Expression.MakeMemberAccess(myParameterExpression, FirstProperty);
            ConstantExpression FirstConstantValue = Expression.Constant(andingGroup[0].Value);

            Expression OverallCondition = null;


            if (Enum.IsDefined(typeof(System.Linq.Expressions.ExpressionType), andingGroup[0].ComparisonOperator.ToString()))
            {
                System.Linq.Expressions.ExpressionType expType = (System.Linq.Expressions.ExpressionType)Enum.Parse(typeof(System.Linq.Expressions.ExpressionType), andingGroup[0].ComparisonOperator.ToString());
                OverallCondition = Expression.MakeBinary(expType, PropertyAccess, FirstConstantValue);
            }
            else
            {
                switch (andingGroup[0].ComparisonOperator)
                {
                    case OperatorType.RegexMatch:
                        OverallCondition = ExpressionExtension.RegexMatch(PropertyAccess, FirstConstantValue);
                        break;
                    case OperatorType.Contains:
                        OverallCondition = ExpressionExtension.Contains(PropertyAccess, FirstConstantValue);
                        break;
                }
            }

            if (andingGroup.Count > 1)
            {
                for (int i = 1; i < andingGroup.Count; i++)
                {
                    PropertyInfo CurrentPoperty = ItemsType.GetProperty(andingGroup[i].PropertyName);
                    MemberExpression CurrentPropertyAccess = Expression.MakeMemberAccess(myParameterExpression, CurrentPoperty);
                    ConstantExpression CurrentConstantValue = Expression.Constant(andingGroup[i].Value);
                    Expression CurrentCondition = null;
                    
                    if(Enum.IsDefined(typeof(System.Linq.Expressions.ExpressionType), andingGroup[i].ComparisonOperator.ToString()))
                    {
                        System.Linq.Expressions.ExpressionType expType = (System.Linq.Expressions.ExpressionType)Enum.Parse(typeof(System.Linq.Expressions.ExpressionType), andingGroup[i].ComparisonOperator.ToString());
                        CurrentCondition = Expression.MakeBinary(expType, CurrentPropertyAccess, CurrentConstantValue);
                    }
                    else
                    {
                        switch (andingGroup[i].ComparisonOperator)
                        {
                            case OperatorType.RegexMatch:
                                CurrentCondition = ExpressionExtension.RegexMatch(CurrentPropertyAccess, CurrentConstantValue);
                                break;
                            case OperatorType.Contains:
                                CurrentCondition = ExpressionExtension.Contains(CurrentPropertyAccess, CurrentConstantValue);
                                break;
                        }
                    }

                    OverallCondition = Expression.And(OverallCondition, CurrentCondition);
                }
            }

            return OverallCondition;
        }
    }

    static class ExpressionExtension
    {
        public static Expression RegexMatch(Expression left, Expression right)
        {
            return Expression.Call(typeof(ExpressionExtensionMethodsImplementations).GetMethod("RegexMatch",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
            , left, right);
        }

        public static Expression Contains(Expression left, Expression right)
        {
            return Expression.Call(typeof(ExpressionExtensionMethodsImplementations).GetMethod("Contains",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
            , left, right);
        }
    }

    static class ExpressionExtensionMethodsImplementations
    {
        static bool RegexMatch(string source, string pattern)
        {
            return Regex.IsMatch(source, pattern);
        }

        static bool Contains(string source, string pattern)
        {
            return source.Contains(pattern);
        }
    }

    public class AndingItem
    {
        #region Fields & Properties
        string propertyName;
        public string PropertyName
        {
            get { return propertyName; }
            set { propertyName = value; }
        }

        object value;
        public object Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        OperatorType comparisonOperator;
        public OperatorType ComparisonOperator
        {
            get { return comparisonOperator; }
            set { comparisonOperator = value; }
        }
        #endregion

        #region Constructors
        public AndingItem()
        {
        }
        public AndingItem(string _propertyName, object _value, OperatorType _comparisonOperator)
        {
            PropertyName = _propertyName;
            Value = _value;
            ComparisonOperator = _comparisonOperator;
        }
        #endregion
    }

    public class AndingGroup : List<AndingItem>
    {

    }

    public class OringGroup : List<AndingGroup>
    {

    }

    public enum OperatorType
    {
        Equal = 1,
        NotEqual = 2,
        GreaterThan = 3,
        GreaterThanOrEqual = 4,
        LessThan = 5,
        LessThanOrEqual = 6,
        RegexMatch = 7,
        Contains = 8
    }
}
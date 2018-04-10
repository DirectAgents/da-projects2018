using System;
using System.Linq.Expressions;

namespace CakeExtracter.Common
{
    public static class PropertyCompare
    {
        //NOTE: doesn't check read-only properties, virtual properties or child objects
        public static bool EqualTest<T>(T x, T y)
        {
            var props = typeof(T).GetProperties();
            bool equal = true;
            foreach (var prop in props)
            {
                if (prop.CanWrite && !prop.SetMethod.IsVirtual && Type.GetTypeCode(prop.PropertyType) != TypeCode.Object)
                {
                    if (!Object.Equals(prop.GetValue(x), prop.GetValue(y)))
                    {
                        equal = false;
                        break;
                    }
                }
            }
            return equal;
        }

        public static bool Equal<T>(T x, T y)
        {
            return Cache<T>.Compare(x, y);
        }
        static class Cache<T>
        {
            internal static readonly Func<T, T, bool> Compare;
            static Cache()
            {
                var props = typeof(T).GetProperties();
                if (props.Length == 0)
                {
                    Compare = delegate { return true; };
                    return;
                }
                var x = Expression.Parameter(typeof(T), "x");
                var y = Expression.Parameter(typeof(T), "y");

                Expression body = null;
                for (int i = 0; i < props.Length; i++)
                {
                    if (props[i].CanWrite && !props[i].SetMethod.IsVirtual && Type.GetTypeCode(props[i].PropertyType) != TypeCode.Object)
                    {
                        var propEqual = Expression.Equal(
                            Expression.Property(x, props[i]),
                            Expression.Property(y, props[i]));
                        if (body == null)
                        {
                            body = propEqual;
                        }
                        else
                        {
                            body = Expression.AndAlso(body, propEqual);
                        }
                    }
                }
                Compare = Expression.Lambda<Func<T, T, bool>>(body, x, y)
                              .Compile();
            }
        }
    }
}

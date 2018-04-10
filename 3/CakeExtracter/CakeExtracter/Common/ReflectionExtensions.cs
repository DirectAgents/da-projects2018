using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CakeExtracter.Common.ReflectionExtensions
{
    public static class ReflectionExtensions
    {
        public static IEnumerable<PropertyInfo> EnumerateProperties(this object target)
        {
            return target.GetType().GetProperties().AsEnumerable();
        }

        public static Type UnderlyingType(this MemberInfo member)
        {
            Type type;
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    type = ((FieldInfo)member).FieldType;
                    break;
                case MemberTypes.Property:
                    type = ((PropertyInfo)member).PropertyType;
                    break;
                case MemberTypes.Event:
                    type = ((EventInfo)member).EventHandlerType;
                    break;
                default:
                    throw new ArgumentException("member must be if type FieldInfo, PropertyInfo or EventInfo", "member");
            }
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        public static object Value(this MemberInfo member, object target)
        {
            if (member is PropertyInfo)
            {
                return (member as PropertyInfo).GetValue(target, null);
            }
            if (member is FieldInfo)
            {
                return (member as FieldInfo).GetValue(target);
            }
            throw new Exception("member must be either PropertyInfo or FieldInfo");
        }

        public static void Assign(this MemberInfo member, object target, object value)
        {
            if (member is PropertyInfo)
            {
                (member as PropertyInfo).SetValue(target, value, null);
            }
            else if (member is FieldInfo)
            {
                (member as FieldInfo).SetValue(target, value);
            }
            else
            {
                throw new Exception("destinationMember must be either PropertyInfo or FieldInfo");
            }
        }

        public static T MemberValue<T>(this object source, string memberName)
        {
            return (T)source.GetType().GetMember(memberName)[0].Value(source);
        }

        public static bool HasAttribute<T>(this MethodInfo method)
        {
            return (method.GetCustomAttributes(typeof(T), false).Length > 0);
        }
    }
}

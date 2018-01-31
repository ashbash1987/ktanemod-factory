using System;
using System.Reflection;

namespace FactoryAssembly
{
    internal static class EnumExtensions
    {
        internal static T GetAttributeOfType<T>(this Enum enumVal) where T : Attribute
        {
            T[] attributes = GetAttributesOfType<T>(enumVal);
            return (attributes != null && attributes.Length > 0) ? attributes[0] : null;
        }

        internal static T[] GetAttributesOfType<T>(this Enum enumVal) where T : Attribute
        {
            Type type = enumVal.GetType();
            MemberInfo[] memInfo = type.GetMember(enumVal.ToString());
            object[] attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (T[])attributes;
        }
    }
}

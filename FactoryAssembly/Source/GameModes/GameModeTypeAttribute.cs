using System;

namespace FactoryAssembly
{
    [AttributeUsage(AttributeTargets.All)]
    internal class GameModeTypeAttribute : Attribute
    {
        internal readonly Type Type;
        internal readonly string FriendlyName;

        internal GameModeTypeAttribute(Type type, string friendlyName)
        {
            Type = type;
            FriendlyName = friendlyName;
        }
    }
}

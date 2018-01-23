using System;

namespace FactoryAssembly
{
    [AttributeUsage(AttributeTargets.All)]
    public class GameModeTypeAttribute : Attribute
    {
        public readonly Type Type;
        public readonly string FriendlyName;

        public GameModeTypeAttribute(Type type, string friendlyName)
        {
            Type = type;
            FriendlyName = friendlyName;
        }
    }
}

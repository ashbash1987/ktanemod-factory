using System;

namespace FactoryAssembly
{
    [AttributeUsage(AttributeTargets.All)]
    public class GameModeTypeAttribute : Attribute
    {
        public readonly Type Type;
        public readonly Type[] Adaptations;
        public readonly string FriendlyName;

        public GameModeTypeAttribute(Type type, string friendlyName, params Type[] adaptations)
        {
            Type = type;
            Adaptations = adaptations;
            FriendlyName = friendlyName;
        }
    }
}

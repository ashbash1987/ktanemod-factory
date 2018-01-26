using System;

namespace FactoryAssembly
{
    [AttributeUsage(AttributeTargets.All)]
    public class GameModeTypeAttribute : Attribute
    {
        public readonly Type Type;
        public readonly Type[] Adaptations;
        public readonly string FriendlyName;
        public readonly bool RequireMultipleBombs;

        public GameModeTypeAttribute(Type type, string friendlyName, bool requireMultipleBombs, params Type[] adaptations)
        {
            Type = type;
            Adaptations = adaptations;
            FriendlyName = friendlyName;
            RequireMultipleBombs = requireMultipleBombs;
        }
    }
}

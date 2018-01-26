using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FactoryAssembly
{
    public static class ReflectionHelper
    {
        public static Type FindType(string fullName)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetSafeTypes()).FirstOrDefault(t => t.FullName.Equals(fullName));
        }

        private static IEnumerable<Type> GetSafeTypes(this Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(x => x != null);
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}

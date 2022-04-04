using System.Reflection;

namespace VueServer.Core.Helper
{
    public static class ModuleHelper
    {
        public static bool IsModuleExtensionDll(Assembly assembly)
        {
            var name = assembly.GetName().Name;
            if (name == "VueServer.Modules.Core")
            {
                return false;
            }

            if (name.StartsWith("VueServer.Modules"))
            {
                return true;
            }

            return false;
        }
    }
}

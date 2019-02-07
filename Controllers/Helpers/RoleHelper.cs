using System;
using System.Collections.Generic;
using System.Text;

namespace VueServer.Controllers.Helpers
{
    public static class RoleHelper
    {
        public static string BuildRoles (string[] roles)
        {
            if (roles == null || roles.Length == 0)
                return "";

            StringBuilder builder = new StringBuilder();
            foreach (var role in roles)
            {
                if (builder.Length > 0)
                    builder.Append(",");
                builder.Append(role);
            }            

            return builder.ToString();
        }
    }
}

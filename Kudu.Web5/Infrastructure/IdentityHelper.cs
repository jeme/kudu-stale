using System.Security.Principal;

namespace Kudu.Web5.Infrastructure
{
    public static class IdentityHelper
    {
        public static bool IsAnAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
using DirectAgents.Domain.Entities.Security;
using System.Collections.Generic;
using System.Security.Principal;

namespace DirectAgents.Domain.Abstract
{
    public interface ISecurityRepository
    {
        Group WindowsIdentityGroup(string windowsIdentity);
        IEnumerable<Group> GroupsForUser(IPrincipal user);
        IEnumerable<Role> RolesForUser(IPrincipal user);
        IEnumerable<Group> GroupsForIpAddress(string ipAddress);

        bool IsAdmin(IPrincipal user);
        bool IsAccountantOrAdmin(IPrincipal user);
        IEnumerable<string> AccountManagersForUser(IPrincipal user, bool replaceSlashesWithAmpersands);
    }
}

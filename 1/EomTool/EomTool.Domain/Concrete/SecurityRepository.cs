using System.Linq;
using EomTool.Domain.Abstract;
using EomTool.Domain.Entities;
using System.Security.Principal;
using System.Collections.Generic;

namespace EomTool.Domain.Concrete
{
    public class SecurityRepository : ISecurityRepository
    {
        EomToolSecurityEntities db;

        public SecurityRepository()
        {
            db = new EomToolSecurityEntities();
        }

        private List<Group> _allGroups = null;
        public List<Group> AllGroups
        {
            get
            {
                if (_allGroups == null)
                    _allGroups = db.Groups.ToList();
                return _allGroups;
            }
        }

        public Group WindowsIdentityGroup(string windowsIdentity)
        {
            var identityGroups = AllGroups.Where(g => g.WindowsIdentity.Split(',').Any(wi => wi.ToUpper() == windowsIdentity.ToUpper()));
            return identityGroups.FirstOrDefault();
        }

        public IEnumerable<Group> GroupsForUser(IPrincipal user)
        {
            var groups = AllGroups.Where(g => g.WindowsIdentity.Split(',').Any(wi => user.IsInRole(wi)));
            return groups;
        }
        public IEnumerable<Role> RolesForUser(IPrincipal user)
        {
            return GroupsForUser(user).SelectMany(g => g.Roles).Distinct();
        }

        public IEnumerable<Group> GroupsForIpAddress(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return null;
            var groups = AllGroups.Where(g => g.IpAddress == ipAddress);
            return groups;
        }

        public bool IsAccountantOrAdmin(IPrincipal user)
        {
            var groups = GroupsForUser(user);
            return groups.Any(g => g.Name == "Accountants" || g.Name == "Administrators");
        }

        public IEnumerable<string> AccountManagersForUser(IPrincipal user)
        {
            var roles = RolesForUser(user);
            var amNames = roles.SelectMany(r => r.Permissions).Distinct().Where(p => p.Name.StartsWith("AM: ")).Select(p => p.Name.Substring(4));
            return amNames;
        }
    }
}

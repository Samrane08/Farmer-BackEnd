using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Entity;

    public class ApplicationRole : IdentityRole
    {

        public int? DepartmentId { get; set; }

    public ApplicationRole(string roleName) : base(roleName) { }

    // Another constructor to initialize RoleName and DepartmentName
    public ApplicationRole(string roleName, int deptId) : base(roleName)
    {
        DepartmentId = deptId;
    }
    public ApplicationRole() { }

}
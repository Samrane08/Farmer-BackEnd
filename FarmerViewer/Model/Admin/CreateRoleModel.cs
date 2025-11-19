using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Admin
{
    public class CreateRoleModel
    {
        [Required]
        public string role { get; set; }=string.Empty;

        [Required]
        public string deptName { get; set; } = string.Empty;
    }
}

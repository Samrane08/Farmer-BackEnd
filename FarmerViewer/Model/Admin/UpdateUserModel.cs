using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Admin
{
    public class UpdateUserModel
    {
        public string PhoneNumber { get; set; }=string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}

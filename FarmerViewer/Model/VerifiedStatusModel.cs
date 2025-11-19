using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class VerifiedStatusModel
    {
        public bool? IsMobileVerified { get; set; }
        public bool? IsEmailVerified { get; set; }
        public bool? IsAadharVerified { get; set; }

        public bool? IsFirstLogin { get; set; }
    }
    public class Applicantdetails
    {
        public string EmailId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Entity
{
    public class logindetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }

        [MaxLength(255)]
        public string? UserId { get; set; }

        [MaxLength(50)]
        public string? UserName { get; set; }

        [MaxLength(200)]
        public string? Password { get; set; }

        [MaxLength(50)]
        public string? EmailId { get; set; }

        [MaxLength(10)]
        public string? MobileNo { get; set; }

        [MaxLength(150)]
        public string? FullName { get; set; }

        [MaxLength(150)]
        public string? FullName_mr { get; set; }

        public int? Age { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        [MaxLength(20)]
        public string? DOB { get; set; } 

        [MaxLength(255)]
        public string? UserIdentity { get; set; }

        public bool? IsMobileVerified { get; set; }

        public bool? IsEmailVerified { get; set; }

        public bool? IsAadharVerified { get; set; }

        public bool? IsFirstLogin {  get; set; }

        [MaxLength(255)]
        public string? CreatedBy { get; set; }

        public DateTime? CreatedOn { get; set; }

        [MaxLength(255)]
        public string? LastModifiedBy { get; set; }

        public DateTime? LastModified { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        [MaxLength(50)]
        public string? Domain { get; set; }

        public int? WorkflowId { get; set; }

        public int? DistrictId { get; set;}
    }
}

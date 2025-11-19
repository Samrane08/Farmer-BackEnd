using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Admin
{
    public class SchoolLoginCreationDto
    {
        public List<string> SchoolUDISCCode {  get; set; }
    }

    public class ScrutinyLogins
    {
        [Required]
        public int DistrictId { get; set; }
        [Required]
        public int WorkflowId { get; set; }
        [Required]
        public string UserName {  get; set; }=string.Empty;
    }
}

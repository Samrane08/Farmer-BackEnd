using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Model
{
    public class DDOModel
    {
        public string ddoid {  get; set; }=string.Empty;
        public string detailHead { get; set; }=string.Empty;
        public string ddO_Code { get; set; } = string.Empty;
    }
    public class WrapperDDOModel
    {
      
        public DDOModel data { get; set; } = new DDOModel();
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class SuccessResult
    {
        public HttpStatusCode statusCode { get; set; }
        public bool status { get; set; }
        
        public string Message {  get; set; }
        public SuccessResult(HttpStatusCode statusCode,bool status, string message)
        {
            this.statusCode = statusCode;
            this.status = status;
            this.Message = message;
        }
    }
}

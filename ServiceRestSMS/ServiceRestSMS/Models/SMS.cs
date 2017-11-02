using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceRestSMS.Models
{
    public class SMS
    {
        public string Mensaje { get; set; }
        public string ID_Instancia { get; set; }
        public bool Es_Control { get; set; }
    }
}
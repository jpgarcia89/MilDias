using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace ServiceRestSMS.Models.MovilGates
{
    [XmlRoot(ElementName = "MTResponse")]
    public class MTResponse
    {
        [XmlElement(ElementName = "Transaccion")]
        public Transaccion Transaccion { get; set; }
        [XmlElement(ElementName = "Texto")]
        public string Texto { get; set; }

        public MTResponse()
        {
            Transaccion = new Transaccion();
        }
    }
}

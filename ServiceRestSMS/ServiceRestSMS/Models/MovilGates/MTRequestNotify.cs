using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace ServiceRestSMS.Models.MovilGates
{
    [XmlRoot(ElementName = "MTRequestNotify")]
    public class MTRequestNotify
    {
        [XmlElement(ElementName = "Servicio")]
        public Servicio Servicio { get; set; }
        [XmlElement(ElementName = "Telefono")]
        public Telefono Telefono { get; set; }
        [XmlElement(ElementName = "Estado")]
        public Estado Estado { get; set; }
        [XmlElement(ElementName = "Info")]
        public string Info { get; set; }
        [XmlElement(ElementName = "TicketId")]
        public TicketId TicketId { get; set; }
    }

}
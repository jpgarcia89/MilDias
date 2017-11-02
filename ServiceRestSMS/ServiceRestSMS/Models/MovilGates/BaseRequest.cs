using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace ServiceRestSMS.Models.MovilGates
{
    [XmlRoot(ElementName = "Proveedor")]
    public class Proveedor
    {
        [XmlAttribute(AttributeName = "Id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "Password")]
        public string Password { get; set; }
    }

    [XmlRoot(ElementName = "Servicio")]
    public class Servicio
    {
        [XmlAttribute(AttributeName = "Id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "ContentType")]
        public string ContentType { get; set; }
    }

    [XmlRoot(ElementName = "Telefono")]
    public class Telefono
    {
        [XmlAttribute(AttributeName = "msisdn")]
        public string Msisdn { get; set; }
        [XmlAttribute(AttributeName = "IdTran")]
        public string IdTran { get; set; }
    }

    [XmlRoot(ElementName = "Contenido")]
    public class Contenido
    {
        [XmlAttribute(AttributeName = "PushText")]
        public string PushText { get; set; }
        [XmlAttribute(AttributeName = "DeliveryAfter")]
        public string DeliveryAfter { get; set; }
        [XmlAttribute(AttributeName = "Priority")]
        public string Priority { get; set; }
        [XmlText]
        public string Text { get; set; }
    }
}
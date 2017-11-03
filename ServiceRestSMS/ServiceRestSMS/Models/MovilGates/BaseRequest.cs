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
        [XmlAttribute(AttributeName = "RefId")]
        public string RefId { get; set; }
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

    [XmlRoot(ElementName = "Transaccion")]
    public class Transaccion
    {
        [XmlAttribute(AttributeName = "estado")]
        public string Estado { get; set; }
        [XmlAttribute(AttributeName = "IdTran")]
        public string IdTran { get; set; }
        [XmlAttribute(AttributeName = "Fecha")]
        public string Fecha { get; set; }
    }
    [XmlRoot(ElementName = "Estado")]
    public class Estado
    {
        [XmlAttribute(AttributeName = "deliverdate")]
        public string Deliverdate { get; set; }
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; }
        [XmlAttribute(AttributeName = "tran_status")]
        public string Tran_status { get; set; }
    }

    [XmlRoot(ElementName = "TicketId")]
    public class TicketId
    {
        [XmlElement(ElementName = "Info")]
        public string Info { get; set; }
        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }
        [XmlAttribute(AttributeName = "idtran")]
        public string Idtran { get; set; }
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; }
        [XmlAttribute(AttributeName = "tran_status")]
        public string Tran_status { get; set; }
        [XmlAttribute(AttributeName = "charge_date")]
        public string Charge_date { get; set; }
        [XmlAttribute(AttributeName = "tariff")]
        public string Tariff { get; set; }
    }
}
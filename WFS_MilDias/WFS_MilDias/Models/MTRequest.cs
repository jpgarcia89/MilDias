using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace WFS_MilDias.Models.MovilGates
{
    [XmlRoot(ElementName = "MTRequest")]
    public class MTRequest
    {
        [XmlElement(ElementName = "Proveedor")]
        public Proveedor Proveedor { get; set; }

        [XmlElement(ElementName = "Servicio")]
        public Servicio Servicio { get; set; }

        [XmlElement(ElementName = "Telefono")]
        public Telefono Telefono { get; set; }

        [XmlElement(ElementName = "Contenido")]
        public Contenido Contenido { get; set; }
    }
}
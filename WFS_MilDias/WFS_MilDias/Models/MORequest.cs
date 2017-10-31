using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace WFS_MilDias.Models.MovilGates
{   
    [Serializable()]
    [XmlRoot(ElementName = "MORequest")]
    public class MORequest
    {
        [XmlElement(ElementName = "Servicio")]
        public Servicio Servicio { get; set; }

        [XmlElement(ElementName = "Telefono")]
        public Telefono Telefono { get; set; }

        [XmlElement(ElementName = "Contenido")]
        public string Contenido { get; set; }
    }
}
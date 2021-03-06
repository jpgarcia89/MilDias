//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ServiceRestSMS.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Inscripcion
    {
        public long ID { get; set; }
        public long ID_EMBARAZADA { get; set; }
        public System.DateTime FECHA_ALTA { get; set; }
        public Nullable<System.DateTime> FECHA_BAJA { get; set; }
        public int MES { get; set; }
        public bool ACTIVO { get; set; }
        public int ID_TIPOINSTANCIA { get; set; }
        public string ID_INSTANCIA { get; set; }
        public string MOTIVO_BAJA { get; set; }
    
        public virtual Embarazada Embarazada { get; set; }
        public virtual TipoInstancia TipoInstancia { get; set; }
    }
}

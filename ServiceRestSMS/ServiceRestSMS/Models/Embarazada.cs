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
    
    public partial class Embarazada
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Embarazada()
        {
            this.Inscripcion = new HashSet<Inscripcion>();
        }
    
        public long ID { get; set; }
        public int ID_EMPRESA { get; set; }
        public int DNI { get; set; }
        public string TELEFONO { get; set; }
    
        public virtual Empresa Empresa { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Inscripcion> Inscripcion { get; set; }
    }
}

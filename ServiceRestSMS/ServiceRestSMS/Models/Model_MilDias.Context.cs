﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class MilDiasEntities : DbContext
    {
        public MilDiasEntities()
            : base("name=MilDiasEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Embarazada> Embarazada { get; set; }
        public virtual DbSet<Empresa> Empresa { get; set; }
        public virtual DbSet<Inscripcion> Inscripcion { get; set; }
        public virtual DbSet<LogMensaje> LogMensaje { get; set; }
        public virtual DbSet<LogMensajeControl> LogMensajeControl { get; set; }
        public virtual DbSet<TipoInstancia> TipoInstancia { get; set; }
        public virtual DbSet<TipoMensaje> TipoMensaje { get; set; }
        public virtual DbSet<TipoRespuestaControl> TipoRespuestaControl { get; set; }
    }
}

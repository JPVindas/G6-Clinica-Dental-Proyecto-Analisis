using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VistasClinicaSanRafael.Models
{
    public class Contabilidad
    {
        [Key] // Esto define 'IdContabilidad' como la clave primaria
        public int IdContabilidad { get; set; }

        public int IdFactura { get; set; }
        public int? IdFinanciamiento { get; set; }
        public decimal Ingresos { get; set; }
        public decimal Gastos { get; set; }
        public DateTime FechaReporte { get; set; }

        // Relaciones con otras entidades
        [ForeignKey("IdFactura")]
        public Factura Factura { get; set; }

        [ForeignKey("IdFinanciamiento")]
        public Financiamiento Financiamiento { get; set; }
    }
}

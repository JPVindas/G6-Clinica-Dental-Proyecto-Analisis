using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("Roles")]
    public class RolesModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }

        //  Relación con UsuariosModel (1 Rol puede tener muchos Usuarios)
        public virtual ICollection<UsuariosModel>? Usuarios { get; set; } = new List<UsuariosModel>();
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Entity
{
    //Noteid, title, Description, Colour, IsArchieved, IsDeleted, [Required] USerId, UserEntity User


    public class NoteEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NoteId { get; set; }

        [Required]
        [MaxLength(30)]
        public string Title {  get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        [MaxLength(30)]
        public string Color {  get; set; } = string.Empty;
        public bool IsArchieved {  get; set; } = false;
        public bool IsDeleted { get; set; } = false;

        [Required]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public UserEntity User { get; set; }
    }
}

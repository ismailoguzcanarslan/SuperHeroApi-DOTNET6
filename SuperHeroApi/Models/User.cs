using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SuperHeroApi.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Domain { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class UserInfo
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}

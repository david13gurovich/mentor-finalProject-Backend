using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace Domain
{
    public class User
    {
        [Key]  public string Id { get; set; }
        public string Name { get; set; }
        [EmailAddress] public string Email { get; set; }
        public string Password { get; set; }
        public List<Mission> Missions { get; set; } = new List<Mission>();
        public ScheduleSetting Schedule { get; set; } = new ScheduleSetting();
    }
}

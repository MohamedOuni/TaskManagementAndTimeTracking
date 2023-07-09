using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EY.TaskShare.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public Team Team { get; set; }
        public Role Role { get; set; }
        [JsonIgnore]
        public ICollection<Project> Projects { get; set; } = new List<Project>();
        [JsonIgnore]
        public ICollection<Tasks> Tasks { get; set; } = new List<Tasks>();

    }
}
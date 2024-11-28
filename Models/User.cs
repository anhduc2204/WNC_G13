using System;
using System.Collections.Generic;

namespace WNC_G13.Models
{
    public partial class User
    {
        public User()
        {
            Documents = new HashSet<Document>();
            Messages = new HashSet<Message>();
            Organizations = new HashSet<Organization>();
            Projects = new HashSet<Project>();
            UserProjects = new HashSet<UserProject>();
            Channels = new HashSet<ChatChannel>();
        }

        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Address { get; set; }
        public int Role { get; set; }

        public virtual ICollection<Document> Documents { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
        public virtual ICollection<Organization> Organizations { get; set; }
        public virtual ICollection<Project> Projects { get; set; }
        public virtual ICollection<UserProject> UserProjects { get; set; }
        public virtual ICollection<ProjectTask> ProjectTasks { get; set; }
        public virtual ICollection<ChatChannel> Channels { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace WNC_G13.Models.ViewModels
{
    public class MyTaskViewModel
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int? UserId { get; set; }
        public string? TaskName { get; set; }
        public string? Description { get; set; }
        public int Status { get; set; }
        public DateTime? DueDate { get; set; }
        public String Priority { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace WNC_G13.Models.ViewModels
{
    public partial class ProjectViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? CreatedBy { get; set; }
        public int Status { get; set; }
    }
}

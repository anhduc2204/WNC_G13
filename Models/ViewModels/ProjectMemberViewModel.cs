using System;
using System.Collections.Generic;

namespace WNC_G13.Models.ViewModels
{
    public partial class ProjectMemberViewModel
    {
        public int userId { get; set;}
        public String fullName { get; set;}
        public int taskTodo {get; set;}
        public int taskInProgress {get; set;}
        public int taskCompleted {get; set;}
    }
}

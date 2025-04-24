using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagementApp
{
    public class Task
    {
        public int Id { get; set; }
        public string TaskName { get; set; }
        public string TaskDescription { get; set; }
        public string TaskDate { get; set; }
        public string Priority { get; set; }
        public string Project { get; set; }

        public Task(int id, string taskName, string taskDescription, string taskDate, string priority, string project)
        {
            Id = id;
            TaskName = taskName;
            TaskDescription = taskDescription;
            TaskDate = taskDate;
            Priority = priority;
            Project = project;
        }


        public Task(string taskName, string taskDescription, string taskDate, string priority, string project)
        {
            TaskName = taskName;
            TaskDescription = taskDescription;
            TaskDate = taskDate;
            Priority = priority;
            Project = project;
        }
    }

}

using System;
namespace quest_web.Models
{
    public class DeleteStatus
    {

        public bool success { get; set; }
        public DeleteStatus()
        {
        }

        public DeleteStatus(bool success)
        {
            this.success = success;
        }
    }
}

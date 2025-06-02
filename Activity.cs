using System.Collections.Generic; 
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanitarianProjectManagement.Models
{
    [Table("Activities")]
    public class Activity
    {
        [Key]
        public int ActivityID { get; set; }

        [Required]
        public int OutputID { get; set; }

        [Required]
        public string ActivityDescription { get; set; }

        public string PlannedMonths { get; set; }
    }
}

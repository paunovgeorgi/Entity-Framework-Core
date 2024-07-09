using System.ComponentModel.DataAnnotations;
using Microsoft.Win32.SafeHandles;

namespace P01_HospitalDatabase.Data.Models
{
    public class Patient
    {
        public Patient()
        {
            this.Visitations = new HashSet<Visitation>();
            this.Diagnoses = new HashSet<Diagnose>();
            this.Prescriptions = new HashSet<PatientMedicament>();
        }

        [Key]
        public int PatientId { get; set; }

        [MaxLength(50)]
        public string FirstName { get; set; } = null!;

        [MaxLength(50)] 
        public string LastName { get; set; } = null!;

        [MaxLength(250)] 
        public string Address { get; set; } = null!;

        [MaxLength(80)]
        public string Email { get; set; } = null!;

        public bool HasInsurance { get; set; }

        public ICollection<Visitation> Visitations { get; set; }

        public ICollection<Diagnose> Diagnoses { get; set; }

        public ICollection<PatientMedicament> Prescriptions { get; set; }

    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppointmentHospital.Models;

namespace AppointmentHospital.Entity;

public class Collaborator
{
    [Key]
    public Guid CollaboratorId { get; set; }

    [Required]
    [MaxLength(100)]
    public string CollaboratorName { get; set; } = "";

    public bool Gender { get; set; }

    public string PhoneNumber { get; set; }
    public virtual ICollection<Appointment> Appointments { get; set; }

    [ForeignKey("CollaboratorId")]       
    public virtual User User { get; set; }
}

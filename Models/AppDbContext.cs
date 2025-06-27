using Microsoft.EntityFrameworkCore;
using healthmate_backend.Models;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Dose> Doses { get; set; }

    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Patient> Patients { get; set; }
	public DbSet<AvailableSlot> AvailableSlots { get; set; }

    public DbSet<Clinic> Clinics { get; set; }
    public DbSet<Reminder> Reminders { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    
    public DbSet<OtpVerification> OtpVerifications { get; set; }

    public DbSet<Admin> Admins { get; set; }

    public DbSet<AdminLog> AdminLogs { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToTable("Users");

        modelBuilder.Entity<Doctor>().ToTable("Doctors");
        modelBuilder.Entity<Patient>().ToTable("Patients");

        modelBuilder.Entity<Doctor>()
            .HasMany(d => d.Clinics)
            .WithMany(c => c.Doctors)
            .UsingEntity(j => j.ToTable("DoctorClinics"));

        modelBuilder.Entity<Reminder>()
            .HasOne(r => r.Patient)
            .WithMany(p => p.Reminders)
            .HasForeignKey(r => r.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Reminder>()
            .HasOne(r => r.CreatedByDoctor)
            .WithMany()
            .HasForeignKey(r => r.CreatedByDoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Reminder>()
            .HasOne(r => r.CreatedByPatient)
            .WithMany()
            .HasForeignKey(r => r.CreatedByPatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorId);
       
        
        
        
modelBuilder.Entity<AvailableSlot>()
    .HasOne(s => s.Doctor)
    .WithMany(d => d.AvailableSlots)
    .HasForeignKey(s => s.DoctorId)
    .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }

}
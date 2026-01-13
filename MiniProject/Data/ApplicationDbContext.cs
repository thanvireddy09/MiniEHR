using Microsoft.EntityFrameworkCore;
using MiniProject.Models;

namespace MiniProject.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<LabOrder> LabOrders { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Doctor> Doctors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Map Patient and inform EF Core there is a database trigger on the table using ToTable overload
            modelBuilder.Entity<Patient>(entity =>
            {
                // Use the ToTable overload that provides a TableBuilder to call HasTrigger
                entity.ToTable("Patient", "Healthcare", tableBuilder =>
                {
                    tableBuilder.HasTrigger("trg_Patient_Audit");
                });

                entity.HasKey(p => p.Id);
            });

            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.ToTable("Doctor", "Healthcare");
                entity.HasKey(d => d.Id);
                entity.Property(d => d.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()")
                      .IsRequired();
            });

            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.ToTable("Appointment", "Healthcare");
                entity.HasKey(a => a.Id);

                // Configure Status default and max length to match model
                entity.Property(a => a.Status)
                      .HasMaxLength(50)
                      .HasDefaultValue("Scheduled")
                      .IsRequired();

                // Ensure FK relationship to Doctor
                entity.HasOne(a => a.Doctor)
                      .WithMany()
                      .HasForeignKey(a => a.DoctorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}

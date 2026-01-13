using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MiniProject.Data;
using MiniProject.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MiniProject.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _context;

        public AppointmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Appointment>> GetAllAsync()
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task<Appointment?> GetByIdAsync(int id)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.LabOrders)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<int> CreateAsync(Appointment appointment)
        {
            // Build strongly-typed SqlParameters (explicit types and sizes)
            var patientIdParam = new SqlParameter("@PatientId", SqlDbType.Int)
            {
                Value = appointment.PatientId
            };

            var dateParam = new SqlParameter("@AppointmentDate", SqlDbType.DateTime2)
            {
                Value = appointment.AppointmentDate
            };

            var reasonParam = new SqlParameter("@Reason", SqlDbType.NVarChar, 255)
            {
                Value = (object?)appointment.Reason ?? DBNull.Value
            };

            var doctorIdParam = new SqlParameter("@DoctorId", SqlDbType.Int)
            {
                Value = (object?)appointment.DoctorId ?? DBNull.Value
            };

            var statusParam = new SqlParameter("@Status", SqlDbType.NVarChar, 50)
            {
                Value = (object?)appointment.Status ?? "Scheduled"
            };

            var newIdParam = new SqlParameter("@NewId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            // Use explicit named parameters in the EXEC string and mark only @NewId as OUTPUT
            var sql = "EXEC [Healthcare].[sp_CreateAppointment] " +
                      "@PatientId = @PatientId, " +
                      "@AppointmentDate = @AppointmentDate, " +
                      "@Reason = @Reason, " +
                      "@DoctorId = @DoctorId, " +
                      "@Status = @Status, " +
                      "@NewId = @NewId OUTPUT";

            try
            {
                await _context.Database.ExecuteSqlRawAsync(sql,
                    patientIdParam, dateParam, reasonParam, doctorIdParam, statusParam, newIdParam);

                return (int)(newIdParam.Value ?? 0);
            }
            catch (SqlException ex)
            {
                // If the stored procedure signature in the database doesn't accept @DoctorId (or differs),
                // fall back to an EF Core insert which matches the current model. This prevents runtime failures
                // when DB schema and code are out of sync.
                if (ex.Message != null && ex.Message.Contains("is not a parameter for procedure", StringComparison.OrdinalIgnoreCase))
                {
                    // Fallback: insert via EF Core so DoctorId is persisted
                    _context.Appointments.Add(appointment);
                    await _context.SaveChangesAsync();
                    return appointment.Id;
                }

                throw; // rethrow for other SQL errors
            }
        }

        public async Task UpdateAsync(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            // Load appointment including lab orders to check for dependents
            var appointment = await _context.Appointments
                .Include(a => a.LabOrders)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return;
            }

            // Prevent delete if there are related lab orders
            if (appointment.LabOrders != null && appointment.LabOrders.Count > 0)
            {
                throw new InvalidOperationException(
                    "Cannot delete appointment because there are existing lab orders. Delete or reassign lab orders before deleting the appointment.");
            }

            // Business rule: Only allow deletion if status is Completed
            if (!string.Equals(appointment.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Only appointments with status 'Completed' can be deleted.");
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Appointment>> GetByPatientIdAsync(int patientId)
        {
             return await _context.Appointments
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
        }
    }
}

using Microsoft.EntityFrameworkCore;
using MiniProject.Data;
using MiniProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniProject.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Report 1: Pending Lab Orders (Where, OrderBy)
        public async Task<List<LabOrder>> GetPendingLabOrdersAsync()
        {
            return await _context.LabOrders
                .Include(l => l.Appointment)
                .ThenInclude(a => a.Patient)
                .Where(l => l.Status == "Pending")
                .OrderBy(l => l.OrderDate)
                .ToListAsync();
        }

        // Report 2: Patients Without Follow-Up (Group Join / Select / Where)
        // Def: Patients who do NOT have any appointment with Date > Now
        public async Task<List<Patient>> GetPatientsWithoutFollowUpAsync()
        {
            var today = DateTime.Now;

            var query = from p in _context.Patients
                        join a in _context.Appointments.Where(x => x.AppointmentDate > today)
                        on p.Id equals a.PatientId into futureApps
                        from subApp in futureApps.DefaultIfEmpty()
                        where subApp == null // No future appointment
                        select p;

            return await query.Distinct().ToListAsync();
        }

        // Report 3: Doctor Productivity (GroupBy, Select, OrderBy)
        public async Task<List<DoctorAppointmentStats>> GetDoctorAppointmentStatsAsync()
        {
            return await _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.Doctor != null)
                .GroupBy(a => a.Doctor!.FullName)
                .Select(g => new DoctorAppointmentStats
                {
                    DoctorName = g.Key ?? "Unknown",
                    AppointmentCount = g.Count()
                })
                .OrderByDescending(x => x.AppointmentCount)
                .ToListAsync();
        }
    }
}

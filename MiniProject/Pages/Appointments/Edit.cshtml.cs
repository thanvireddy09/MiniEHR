using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MiniProject.Models;
using MiniProject.Services;
using System.Threading.Tasks;

namespace MiniProject.Pages.Appointments
{
    public class EditModel : PageModel
    {
        private readonly IAppointmentService _apptService;
        private readonly IPatientService _patientService;
        private readonly IDoctorService _doctorService;

        public EditModel(IAppointmentService apptService, IPatientService patientService, IDoctorService doctorService)
        {
            _apptService = apptService;
            _patientService = patientService;
            _doctorService = doctorService;
        }

        [BindProperty]
        public Appointment Appointment { get; set; } = default!;

        public SelectList PatientList { get; set; } = default!;
        public SelectList DoctorList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var appt = await _apptService.GetByIdAsync(id.Value);
            if (appt == null) return NotFound();
            Appointment = appt;

            var patients = await _patientService.GetAllAsync();
            PatientList = new SelectList(patients, "Id", "Name");

            var doctors = await _doctorService.GetAllAsync();
            DoctorList = new SelectList(doctors, "Id", "FullName");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var patients = await _patientService.GetAllAsync();
                PatientList = new SelectList(patients, "Id", "Name");

                var doctors = await _doctorService.GetAllAsync();
                DoctorList = new SelectList(doctors, "Id", "FullName");
                return Page();
            }

            await _apptService.UpdateAsync(Appointment);

            return RedirectToPage("./Index");
        }
    }
}

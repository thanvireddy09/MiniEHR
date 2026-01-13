using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniProject.Models;
using MiniProject.Services;
using System;
using System.Threading.Tasks;

namespace MiniProject.Pages.Patients
{
    public class DeleteModel : PageModel
    {
        private readonly IPatientService _service;

        public DeleteModel(IPatientService service)
        {
            _service = service;
        }

        // ? DO NOT bind Patient on POST
        public Patient Patient { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _service.GetByIdAsync(id.Value);

            if (patient == null)
            {
                return NotFound();
            }

            Patient = patient;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return RedirectToPage("./Index");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                var patient = await _service.GetByIdAsync(id);
                if (patient == null)
                {
                    return NotFound();
                }

                Patient = patient;
                return Page();
            }
        }
    }
}

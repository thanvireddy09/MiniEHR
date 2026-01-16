using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniProject.Models;
using MiniProject.Services;

namespace MiniProject.Pages.LabOrders
{
    public class EditModel : PageModel
    {
        private readonly ILabOrderService _labService;

        public EditModel(ILabOrderService labService)
        {
            _labService = labService;
        }

        [BindProperty]
        public LabOrder LabOrder { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var lab = await _labService.GetByIdAsync(id);
            if (lab == null)
            {
                return NotFound();
            }

            LabOrder = lab;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Remove navigation property from validation to avoid unrelated validation errors
            ModelState.Remove("LabOrder.Appointment");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Load the existing entity from the database and apply only allowed changes
            var existing = await _labService.GetByIdAsync(LabOrder.Id);
            if (existing == null)
            {
                return NotFound();
            }

            existing.TestName = LabOrder.TestName;
            existing.Status = LabOrder.Status;
            existing.Result = LabOrder.Result;

            await _labService.UpdateAsync(existing);

            return RedirectToPage("./Index");
        }
    }
}

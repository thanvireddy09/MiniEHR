using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniProject.Models;
using MiniProject.Services;

namespace MiniProject.Pages.LabOrders
{
    public class DeleteModel : PageModel
    {
        private readonly ILabOrderService _labService;

        public DeleteModel(ILabOrderService labService)
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
            if (LabOrder == null || LabOrder.Id == 0)
            {
                return BadRequest();
            }

            var existing = await _labService.GetByIdAsync(LabOrder.Id);
            if (existing == null)
            {
                return NotFound();
            }

            await _labService.DeleteAsync(LabOrder.Id);

            return RedirectToPage("./Index");
        }
    }
}

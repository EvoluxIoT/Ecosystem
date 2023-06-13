using EvoluxIoT.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EvoluxIoT.Web.Controllers
{
    public class SynapseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SynapseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Synapse
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = User.Identity?.Name;
            var applicationDbContext = _context.Synapse.Where(a => a.OwnerId == user).Include(s => s.Model);
            return View(await applicationDbContext.ToListAsync());
        }

        [HttpPost, ActionName("Unlink")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlink(string id)
        {
            if (_context.Synapse == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Synapse'  is null.");
            }
            var synapse = await _context.Synapse.FindAsync(id);
            if (synapse != null)
            {
                _context.Synapse.Remove(synapse);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private bool SynapseExists(string id)
        {
            return (_context.Synapse?.Any(e => e.Identifier == id)).GetValueOrDefault();
        }
    }
}

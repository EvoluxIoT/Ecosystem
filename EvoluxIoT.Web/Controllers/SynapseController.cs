using EvoluxIoT.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EvoluxIoT.Web.Services;

namespace EvoluxIoT.Web.Controllers
{
    public class SynapseController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly MqttClientService _mqtt;

        public SynapseController(ApplicationDbContext context, MqttClientService mqtt)
        {
            _context = context;
            _mqtt = mqtt;
        }

        // GET: Synapse
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = User.Identity?.Name;
            var applicationDbContext = _context.Synapse.Where(a => a.OwnerId == user).Include(s => s.Model);
            var synapses = await applicationDbContext.ToListAsync();
           
            foreach (var item in synapses)
            {
                if (await _mqtt.Heartbeat(item.Identifier))
                {
                    if (item.NetworkStatus == EvoluxIoT.Models.Synapse.SynapseNetworkStatus.Offline)
                    {
                        item.NetworkStatusSince = DateTime.Now;
                    }

                    item.NetworkStatus = EvoluxIoT.Models.Synapse.SynapseNetworkStatus.Online;
                }
                else
                {
                    if (item.NetworkStatus == EvoluxIoT.Models.Synapse.SynapseNetworkStatus.Online)
                    {
                        item.NetworkStatusSince = DateTime.Now;
                    }
                    item.NetworkStatus = EvoluxIoT.Models.Synapse.SynapseNetworkStatus.Offline;
                }

                


            }
            await _context.SaveChangesAsync();
            return View(synapses);
        }

        [Authorize]
        public async Task<IActionResult> Manage(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var synapse = await _context.Synapse
                .Include(s => s.Model)
                .FirstOrDefaultAsync(m => m.Identifier == id);
            if (synapse == null)
            {
                return NotFound();
            }

            if (synapse.OwnerId != User.Identity?.Name)
            {
                return Forbid();
            }

            _mqtt.SubscribeKeyboard(User.Identity.Name ,synapse.Identifier);

            return View(synapse);
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EvoluxIoT.Models.Synapse;
using EvoluxIoT.Web.Data;

namespace EvoluxIoT.Web.Controllers.Administration
{

    public class AdministrationSynapseTemplateController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdministrationSynapseTemplateController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AdministrationSynapseTemplate
        public async Task<IActionResult> Index()
        {
            return _context.SynapseTemplate != null ?
                        View("~/Views/Administration/SynapseTemplate/Index.cshtml", await _context.SynapseTemplate.ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.SynapseTemplate'  is null.");
        }

        // GET: AdministrationSynapseTemplate/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.SynapseTemplate == null)
            {
                return NotFound();
            }

            var synapseTemplate = await _context.SynapseTemplate
                .FirstOrDefaultAsync(m => m.Id == id);
            if (synapseTemplate == null)
            {
                return NotFound();
            }

            return View("~/Views/Administration/SynapseTemplate/Details.cshtml", synapseTemplate);
        }

        // GET: AdministrationSynapseTemplate/Create
        public IActionResult Create()
        {
            return View("~/Views/Administration/SynapseTemplate/Create.cshtml");
        }

        // POST: Administration/SynapseTemplate/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Model,Description,Enabled,BuildStatus,ReleasedAt")] SynapseTemplate synapseTemplate)
        {
            if (ModelState.IsValid)
            {
                synapseTemplate.UpdatedAt = synapseTemplate.CreatedAt = DateTime.Now;
                _context.Add(synapseTemplate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Administration/SynapseTemplate/Create.cshtml", synapseTemplate);
        }

        // GET: Administration/SynapseTemplate/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.SynapseTemplate == null)
            {
                return NotFound();
            }

            var synapseTemplate = await _context.SynapseTemplate.FindAsync(id);
            if (synapseTemplate == null)
            {
                return NotFound();
            }
            return View("~/Views/Administration/SynapseTemplate/Edit.cshtml", synapseTemplate);
        }

        // POST: Administration/SynapseTemplate/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Model,Description,Enabled,BuildStatus,ReleasedAt")] SynapseTemplate synapseTemplate)
        {
            if (id != synapseTemplate.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    synapseTemplate.UpdatedAt = DateTime.Now;
                    _context.Update(synapseTemplate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SynapseTemplateExists(synapseTemplate.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Administration/SynapseTemplate/Edit.cshtml", synapseTemplate);
        }

        // GET: Administration/SynapseTemplate/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.SynapseTemplate == null)
            {
                return NotFound();
            }

            var synapseTemplate = await _context.SynapseTemplate
                .FirstOrDefaultAsync(m => m.Id == id);
            if (synapseTemplate == null)
            {
                return NotFound();
            }

            return View("~/Views/Administration/SynapseTemplate/Delete.cshtml", synapseTemplate);
        }

        // POST: AdministrationSynapseTemplate/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.SynapseTemplate == null)
            {
                return Problem("Entity set 'ApplicationDbContext.SynapseTemplate'  is null.");
            }
            var synapseTemplate = await _context.SynapseTemplate.FindAsync(id);
            if (synapseTemplate != null)
            {
                _context.SynapseTemplate.Remove(synapseTemplate);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SynapseTemplateExists(int id)
        {
            return (_context.SynapseTemplate?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}

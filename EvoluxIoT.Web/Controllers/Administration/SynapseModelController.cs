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
    public class SynapseModelController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SynapseModelController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AdministrationSynapseModel
        public async Task<IActionResult> Index()
        {
              return _context.SynapseModel != null ? 
                          View("~/Views/Administration/SynapseModel/Index.cshtml", await _context.SynapseModel.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.SynapseModel'  is null.");
        }

        // GET: AdministrationSynapseModel/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.SynapseModel == null)
            {
                return NotFound();
            }

            var synapseModel = await _context.SynapseModel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (synapseModel == null)
            {
                return NotFound();
            }

            return View("~/Views/Administration/SynapseModel/Details.cshtml", synapseModel);
        }

        // GET: AdministrationSynapseModel/Create
        public IActionResult Create()
        {
            return View("~/Views/Administration/SynapseModel/Create.cshtml");
        }

        // POST: AdministrationSynapseModel/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Model,Description,Enabled,BuildStatus,ReleasedAt")] SynapseModel synapseModel)
        {
            if (ModelState.IsValid)
            {
                synapseModel.UpdatedAt = synapseModel.CreatedAt = DateTime.Now;
                _context.Add(synapseModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Administration/SynapseModel/Create.cshtml", synapseModel);
        }

        // GET: AdministrationSynapseModel/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.SynapseModel == null)
            {
                return NotFound();
            }

            var synapseModel = await _context.SynapseModel.FindAsync(id);
            if (synapseModel == null)
            {
                return NotFound();
            }
            return View("~/Views/Administration/SynapseModel/Edit.cshtml", synapseModel);
        }

        // POST: AdministrationSynapseModel/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Model,Description,Enabled,BuildStatus,ReleasedAt")] SynapseModel synapseModel)
        {
            if (id != synapseModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    synapseModel.UpdatedAt = DateTime.Now;
                    _context.Update(synapseModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SynapseModelExists(synapseModel.Id))
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
            return View("~/Views/Administration/SynapseModel/Edit.cshtml", synapseModel);
        }

        // GET: AdministrationSynapseModel/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.SynapseModel == null)
            {
                return NotFound();
            }

            var synapseModel = await _context.SynapseModel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (synapseModel == null)
            {
                return NotFound();
            }

            return View("~/Views/Administration/SynapseModel/Delete.cshtml", synapseModel);
        }

        // POST: AdministrationSynapseModel/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.SynapseModel == null)
            {
                return Problem("Entity set 'ApplicationDbContext.SynapseModel'  is null.");
            }
            var synapseModel = await _context.SynapseModel.FindAsync(id);
            if (synapseModel != null)
            {
                _context.SynapseModel.Remove(synapseModel);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SynapseModelExists(int id)
        {
          return (_context.SynapseModel?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}

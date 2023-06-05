using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EvoluxIoT.Models.Synapse;
using EvoluxIoT.Web.Data;
using System.Diagnostics;

namespace EvoluxIoT.Web.Controllers.Administration
{
    public class SynapseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SynapseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Synapse
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Synapse.Include(s => s.Model);
            return View("~/Views/Administration/Synapse/Index.cshtml",await applicationDbContext.ToListAsync());
        }

        // GET: Synapse/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.Synapse == null)
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

            return View("~/Views/Administration/Synapse/Details.cshtml", synapse);
        }

        // GET: Synapse/Create
        public IActionResult Create()
        {
            ViewData["ModelId"] = new SelectList(_context.SynapseTemplate, "Id", "Model");
            return View("~/Views/Administration/Synapse/Create.cshtml");
        }

        // POST: Synapse/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Identifier,ModelId,OwnerId")] Synapse synapse)
        {
#pragma warning disable CS8601 // Possível atribuição de referência nula.
            synapse.Model = await _context.SynapseTemplate.FindAsync(synapse.ModelId);
#pragma warning restore CS8601 // Possível atribuição de referência nula.
            ModelState.Clear();
            TryValidateModel(synapse);
            if (ModelState.IsValid)
            {
                _context.Add(synapse);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    Debug.WriteLine(error.ErrorMessage,"Validação");
                }
            }
            ViewData["ModelId"] = new SelectList(_context.SynapseTemplate, "Id", "Model", synapse.ModelId);
            return View("~/Views/Administration/Synapse/Create.cshtml", synapse);
        }

        // GET: Synapse/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.Synapse == null)
            {
                return NotFound();
            }

            var synapse = await _context.Synapse.FindAsync(id);
            if (synapse == null)
            {
                return NotFound();
            }
            ViewData["ModelId"] = new SelectList(_context.SynapseTemplate, "Id", "Model", synapse.ModelId);
            return View("~/Views/Administration/Synapse/Edit.cshtml", synapse);
        }

        // POST: Synapse/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Identifier,ModelId,OwnerId")] Synapse synapse)
        {
            if (id != synapse.Identifier)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(synapse);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SynapseExists(synapse.Identifier))
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
            ViewData["ModelId"] = new SelectList(_context.SynapseTemplate, "Id", "Model", synapse.ModelId);
            return View("~/Views/Administration/Synapse/Edit.cshtml", synapse);
        }

        // GET: Synapse/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.Synapse == null)
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

            return View("~/Views/Administration/Synapse/Delete.cshtml", synapse);
        }

        // POST: Synapse/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
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

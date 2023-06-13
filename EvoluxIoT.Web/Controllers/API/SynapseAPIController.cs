using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EvoluxIoT.Models.Synapse;
using EvoluxIoT.Web.Data;
using EvoluxIoT.Web.Models;
using System.Net;

namespace EvoluxIoT.Web.Controllers.API
{
    [Route("api/synapse")]
    [ApiController]
    public class SynapseAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SynapseAPIController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Synapse
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Synapse>>> GetSynapse()
        {
          if (_context.Synapse == null)
          {
              return NotFound();
          }
            return await _context.Synapse.ToListAsync();
        }

        // GET: api/Synapse/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Synapse>> GetSynapse(string id)
        {
          if (_context.Synapse == null)
          {
              return NotFound();
          }
            var synapse = await _context.Synapse.FindAsync(id);

            if (synapse == null)
            {
                return NotFound();
            }

            return synapse;
        }

        // PUT: api/Synapse/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSynapse(string id, Synapse synapse)
        {
            if (id != synapse.Identifier)
            {
                return BadRequest();
            }

            _context.Entry(synapse).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SynapseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Synapse
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Synapse>> PostSynapse(Synapse synapse)
        {
          if (_context.Synapse == null)
          {
              return Problem("Entity set 'ApplicationDbContext.Synapse'  is null.");
          }
            _context.Synapse.Add(synapse);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (SynapseExists(synapse.Identifier))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetSynapse", new { id = synapse.Identifier }, synapse);
        }

        // DELETE: api/Synapse/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSynapse(string id)
        {
            if (_context.Synapse == null)
            {
                return NotFound();
            }
            var synapse = await _context.Synapse.FindAsync(id);
            if (synapse == null)
            {
                return NotFound();
            }

            _context.Synapse.Remove(synapse);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/Synapse/link/5
        /// <summary>
        /// Links a synapse to a ownerid (email) if specified, else unlinks it and sets it to null
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPatch("link/{id}")]
        public async Task<IActionResult> LinkSynapse(string id, [FromBody] string? ownerid)
        {
            // Response object
            EvoluxActionResponse response = new EvoluxActionResponse();

            // Check if the database context is null
            if (_context.Synapse == null)
            {
                response.SetDatabaseContextEntityFailure();
                return response.ToJsonResult(HttpStatusCode.NotFound);
            }

            // Check if the synapse exists
            var synapse = await _context.Synapse.FindAsync(id);
            if (synapse == null)
            {
                response.SetEntityNotFoundFailure();
                return response.ToJsonResult(HttpStatusCode.NotFound);
            }

            // Assign the owner to the synapse
            synapse.OwnerId = ownerid;

            // Let the ORM know that the synapse has been modified
            _context.Entry(synapse).State = EntityState.Modified;


            // Save the changes
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!SynapseExists(id))
                {
                    response.SetEntityNotFoundFailure(ex);
                    return response.ToJsonResult(HttpStatusCode.NotFound);
                }
                else
                {
                    response.SetFailure(ex);
                    return response.ToJsonResult(HttpStatusCode.InternalServerError);
                }
            }

            response.SetSuccess(synapse);
            return response.ToJsonResult(HttpStatusCode.OK);
        }   


        private bool SynapseExists(string id)
        {
            return (_context.Synapse?.Any(e => e.Identifier == id)).GetValueOrDefault();
        }
    }
}

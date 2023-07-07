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
using EvoluxIoT.Web.Services;

namespace EvoluxIoT.Web.Controllers.API
{
    [Route("api/synapse")]
    [ApiController]
    public class SynapseAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        private readonly MqttClientService _mqtt;

        public SynapseAPIController(ApplicationDbContext context, MqttClientService mqtt)
        {
            _context = context;
            _mqtt = mqtt;
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

        [HttpGet("heartbeat/{owner}")]
        public async Task<ActionResult<Dictionary<string,bool>>> GetHeartbeat(string owner)
        {
              if (_context.Synapse == null)
                {
                  return NotFound();
              }

            Dictionary<string, bool> result = new Dictionary<string, bool>();

            foreach (var item in _context.Synapse.Where(s => s.OwnerId == owner))
            {
                if (await _mqtt.Heartbeat(item.Identifier))
                {
                    item.NetworkStatus = EvoluxIoT.Models.Synapse.SynapseNetworkStatus.Online;
                    result.Add(item.Identifier, true);
                }
                else
                {
                    item.NetworkStatus = EvoluxIoT.Models.Synapse.SynapseNetworkStatus.Offline;
                    result.Add(item.Identifier, false);
                }

                


            }
            await _context.SaveChangesAsync();





            return result;
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

        [HttpGet("asklink/{email}/{id}")]
        public async Task<IActionResult> AskLink(string email,string id)
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
                response.SetEntityNotFoundFailure(new Exception("This specified synapse doesn't exists!"));
                return response.ToJsonResult(HttpStatusCode.NotFound);
            }

            // Check if the synapse is already linked
            if (synapse.OwnerId != null)
            {
                response.SetEntityAlreadyLinkedFailure(new Exception("This synapse is already linked to another account!"));
                return response.ToJsonResult(HttpStatusCode.BadRequest);
            }

            // Generate a random code
            string code = new Random().Next(10000000,99999999).ToString();
            if (!await _mqtt.Heartbeat(synapse.Identifier)) { 
                               
                response.SetEntityNotFoundFailure(new Exception("This synapse must be turned on and connected to internet to be linked to a account"));
                return response.ToJsonResult(HttpStatusCode.FailedDependency);
            } else
            {
                await _mqtt.DisplayWrite(id, $"Use code {code} to link to EvoluIoT");
            }


            
            _mqtt.codes[id] = (code, email, DateTime.Now);
            // Return the code
            return response.ToJsonResult(HttpStatusCode.OK);
        }

        [HttpGet("confirmlink/{email}/{id}/{code}")]
        public async Task<IActionResult> ConfirmLink(string email, string id, string code)
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
                response.SetEntityNotFoundFailure(new Exception("This specified synapse doesn't exists!"));
                return response.ToJsonResult(HttpStatusCode.NotFound);
            }

            // Check if the synapse is already linked
            if (synapse.OwnerId != null)
            {
                response.SetEntityAlreadyLinkedFailure(new Exception("This synapse is already linked to another account!"));
                return response.ToJsonResult(HttpStatusCode.BadRequest);
            }

            (string, string, DateTime) codef;

            if (!_mqtt.codes.TryGetValue(id, out codef))
            {
                await Task.Delay(3500);
                response.SetEntityNotFoundFailure(new Exception("You must request the device pin first!"));
                return response.ToJsonResult(HttpStatusCode.Gone);
            }

            // Check if the code is correct
            if (codef.Item1 != code || codef.Item2 != email || (DateTime.Now - codef.Item3).TotalSeconds > 600)
            {
                _mqtt.codes.Remove(id);

                await _mqtt.DisplayWrite(id, $"EvoluxIoT linking process aborted!");
                await Task.Delay(3500);
                response.SetEntityNotFoundFailure(new Exception("The association process has a invalid pin, invalid email or has expired!"));
                return response.ToJsonResult(HttpStatusCode.Forbidden);
            }

            // Link the synapse
            _mqtt.codes.Remove(id);
            synapse.OwnerId = email;
            await _context.SaveChangesAsync();


            await _mqtt.DisplayWrite(id, $"Synapse linked to {email}!");

            // Return the code
            return response.ToJsonResult(HttpStatusCode.OK);
        }

       

        [HttpGet("digitalread/{id}/{port}")]
        public async Task<IActionResult> DigitalRead(string id, int port)
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
                response.SetEntityNotFoundFailure(new Exception("This specified synapse doesn't exists!"));
                return response.ToJsonResult(HttpStatusCode.NotFound);
            }

            // Check if the synapse is already linked
            if (synapse.OwnerId == null)
            {
                response.SetEntityAlreadyLinkedFailure(new Exception("This synapse is not linked to any account!"));
                return response.ToJsonResult(HttpStatusCode.BadRequest);
            }

            // Check if the synapse is online
            if (!await _mqtt.Heartbeat(synapse.Identifier))
            {
                response.SetEntityNotFoundFailure(new Exception("This synapse must be turned on and connected to internet"));
                return response.ToJsonResult(HttpStatusCode.FailedDependency);
            }
            response.SetSuccess(await _mqtt.DigitalRead(synapse.Identifier, port));

            // Return the code
            return response.ToJsonResult(HttpStatusCode.OK);
        }

        [HttpGet("analogread/{id}/{port}")]
        public async Task<IActionResult> AnalogRead(string id, int port)
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
                response.SetEntityNotFoundFailure(new Exception("This specified synapse doesn't exists!"));
                return response.ToJsonResult(HttpStatusCode.NotFound);
            }

            // Check if the synapse is already linked
            if (synapse.OwnerId == null)
            {
                response.SetEntityAlreadyLinkedFailure(new Exception("This synapse is not linked to any account!"));
                return response.ToJsonResult(HttpStatusCode.BadRequest);
            }

            // Check if the synapse is online
            if (!await _mqtt.Heartbeat(synapse.Identifier))
            {
                response.SetEntityNotFoundFailure(new Exception("This synapse must be turned on and connected to internet"));
                return response.ToJsonResult(HttpStatusCode.FailedDependency);
            }
            response.SetSuccess(await _mqtt.AnalogRead(synapse.Identifier, port));

            // Return the code
            return response.ToJsonResult(HttpStatusCode.OK);
        }

        [HttpGet("digitalwrite/{id}/{port}/{value}")]
        public async Task<IActionResult> DigitalWrite(string id, int port, bool value)
        {
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
                response.SetEntityNotFoundFailure(new Exception("This specified synapse doesn't exists!"));
                return response.ToJsonResult(HttpStatusCode.NotFound);
            }

            // Check if the synapse is already linked
            if (synapse.OwnerId == null)
            {
                response.SetEntityAlreadyLinkedFailure(new Exception("This synapse is not linked to any account!"));
                return response.ToJsonResult(HttpStatusCode.BadRequest);
            }

            // Check if the synapse is online
            if (!await _mqtt.Heartbeat(synapse.Identifier))
            {
                response.SetEntityNotFoundFailure(new Exception("This synapse must be turned on and connected to internet"));
                return response.ToJsonResult(HttpStatusCode.FailedDependency);
            }

            
            response.SetSuccess(await _mqtt.DigitalWrite(synapse.Identifier, port, value));

            // Return the code
            return response.ToJsonResult(HttpStatusCode.OK);


        }

        [HttpGet("displayread/{id}")]
        public async Task<IActionResult> DisplayRead(string id)
        {
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
                response.SetEntityNotFoundFailure(new Exception("This specified synapse doesn't exists!"));
                return response.ToJsonResult(HttpStatusCode.NotFound);
            }

            // Check if the synapse is already linked
            if (synapse.OwnerId == null)
            {
                response.SetEntityAlreadyLinkedFailure(new Exception("This synapse is not linked to any account!"));
                return response.ToJsonResult(HttpStatusCode.BadRequest);
            }

            // Check if the synapse is online
            if (!await _mqtt.Heartbeat(synapse.Identifier))
            {
                response.SetEntityNotFoundFailure(new Exception("This synapse must be turned on and connected to internet"));
                return response.ToJsonResult(HttpStatusCode.FailedDependency);
            }
            response.SetSuccess(await _mqtt.DisplayRead(synapse.Identifier));

            // Return the code
            return response.ToJsonResult(HttpStatusCode.OK);
        }

        [HttpGet("displaywrite/{id}/{text}")]
        public async Task<IActionResult> DisplayWrite(string id, string text)
        {
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
                response.SetEntityNotFoundFailure(new Exception("This specified synapse doesn't exists!"));
                return response.ToJsonResult(HttpStatusCode.NotFound);
            }

            // Check if the synapse is already linked
            if (synapse.OwnerId == null)
            {
                response.SetEntityAlreadyLinkedFailure(new Exception("This synapse is not linked to any account!"));
                return response.ToJsonResult(HttpStatusCode.BadRequest);
            }

            // Check if the synapse is online
            if (!await _mqtt.Heartbeat(synapse.Identifier))
            {
                response.SetEntityNotFoundFailure(new Exception("This synapse must be turned on and connected to internet"));
                return response.ToJsonResult(HttpStatusCode.FailedDependency);
            }

            response.SetSuccess(await _mqtt.DisplayWrite(synapse.Identifier, text));

            // Return the code
            return response.ToJsonResult(HttpStatusCode.OK);
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

        [HttpPost("reboot/{id}")]
        public async Task<IActionResult> RebootSynapse(string id)
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

            // Send the reboot command to the synapse
            

            // Let the ORM know that the synapse has been modified
            _context.Entry(synapse).State = EntityState.Modified;

            response.Success = await _mqtt.Reboot(id) == true;
            return response.ToJsonResult(HttpStatusCode.OK);
        }


        private bool SynapseExists(string id)
        {
            return (_context.Synapse?.Any(e => e.Identifier == id)).GetValueOrDefault();
        }
    }
}

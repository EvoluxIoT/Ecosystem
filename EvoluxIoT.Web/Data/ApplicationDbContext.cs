using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EvoluxIoT.Models.Synapse;

namespace EvoluxIoT.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<EvoluxIoT.Models.Synapse.Synapse> Synapse { get; set; } = default!;
        public DbSet<EvoluxIoT.Models.Synapse.SynapseModel> SynapseModel { get; set; } = default!;
    }
}
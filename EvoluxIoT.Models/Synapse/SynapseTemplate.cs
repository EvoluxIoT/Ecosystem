using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace EvoluxIoT.Models.Synapse
{
    public enum SynapseTemplateBuildStatus
    {

        /// <summary>
        /// This template is being planned by it's creators
        /// </summary>
        Planning = 0,

        /// <summary>
        /// This template is being tested by it's creators
        /// </summary>
        Testing = 1,

        /// <summary>
        /// This template is ready to be used by everyone
        /// </summary>
        Production = 2,

        [Display(Name = "End of Life")]
        /// <summary>
        /// This template have reached his end of life
        /// </summary>
        EndOfLife = 3,
    }

    /// <summary>
    /// Represents a new Synapse Template, a model defining a group of devices enabled to connect and interact with the EvoluxIoT ecossystem
    /// </summary>
    public class SynapseTemplate
    {
        /// <summary>
        /// Synapse Template ID within the database
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Synapse Template Name up to 64 characters (ex. "SynapsePod")
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string Name { get; set; } = "SynapsePod";

        /// <summary>
        /// Model representing the Synapse Model up to 32 characters (ex. "Crystal", "")
        /// </summary>
        [Required]
        [MaxLength(32)]
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Optional detailed description of this Synapse Model up to 128 characters
        /// </summary>
        [MaxLength(128)]
        [DataType(DataType.MultilineText)]
        
        public string? Description { get; set; } = string.Empty;

        /// <summary>
        /// Template enabled for Templateion usage?
        /// </summary>
        [Required]
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Current Template release status
        /// </summary>
        [Required]
        [Display(Name = "Template Status")]
        public SynapseTemplateBuildStatus BuildStatus { get; set; } = SynapseTemplateBuildStatus.Planning;

        /// <summary>
        /// When was this Synapse Template released?
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime? ReleasedAt { get; set; } = null;

        /// <summary>
        /// When was this Synapse Template created?
        /// </summary>
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// When was this Synapse Template last updated?
        /// </summary>
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        override public string ToString()
        {
            return $"{Name} {Model}";
        }
    }
}

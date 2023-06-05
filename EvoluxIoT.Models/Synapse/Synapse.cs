using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EvoluxIoT.Models.Synapse
{
    public enum SynapseNetworkStatus
    {
        Offline = 0,
        Online = 1,
    }

    public class Synapse
    {
        [Key]
        public string Identifier { get; set; }

        /// <summary>
        /// Synapse hostname since last network request
        /// </summary>
        [MaxLength(64)]
        public string Hostname { get; set; } = String.Empty;

        /// <summary>
        /// When was this Synapse product created?
        /// </summary>
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// When was this Synapse product last updated?
        /// </summary>
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// When was this Synapse activated by the user?
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime? ActivatedAt { get; set; } = null;

        /// <summary>
        /// Synapse current network status
        /// </summary>
        [Required]
        public SynapseNetworkStatus NetworkStatus { get; set; } = SynapseNetworkStatus.Offline;

        /// <summary>
        /// When was this Synapse product last updated?
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime? NetworkStatusSince { get; set; } = null;

        public int ModelId { get; set; }        
        public SynapseTemplate Model { get; set; }

        public string? OwnerId { get; set; } = null;

    }
}

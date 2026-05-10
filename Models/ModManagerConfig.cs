using System.Collections.Generic;

namespace CortexCommandModManager.Models
{
    public class ModManagerConfig
    {
        public List<ModInfo> AvailableMods { get; set; } = new List<ModInfo>();
    }
}

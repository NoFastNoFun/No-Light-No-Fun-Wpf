using System.Collections.Generic;

namespace Core.Models
{
    public class Config
    {
        // High-level config blocks (from JSON)
        public Dictionary<string, List<string>> Groups { get; set; } = new();
        public Dictionary<string, List<UniDest>> Universes { get; set; } = new();
        public List<RouteSpec> Routes { get; set; } = new();

        // Generated for the router (pre-computed flattened mapping)
        public List<MapEntry> Mapping { get; set; } = new();

        // Misc settings
        public List<Patch> Patch { get; set; } = new();
        public double MaxFPS { get; set; } = 30.0;
        public int EhubPort { get; set; } = 8765;
        public int ArtNetPort { get; set; } = 6454;
    }

    public class UniDest
    {
        public string IP { get; set; } = string.Empty;
        public ushort Universe { get; set; }
        public ushort Channel { get; set; }
    }

    public class RouteSpec
    {
        public string Group { get; set; } = string.Empty;
        public string UniBank { get; set; } = string.Empty;
        public ushort Channel { get; set; }
        public string Select { get; set; } = "RGB";
        public bool Enable { get; set; } = true;
    }
} 
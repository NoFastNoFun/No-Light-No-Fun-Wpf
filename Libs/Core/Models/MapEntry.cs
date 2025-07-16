namespace Core.Models
{
    public class MapEntry
    {
        public uint Entity { get; set; }
        public string Controller { get; set; } = string.Empty;
        public byte Universe { get; set; }
        public ushort Channel { get; set; }
        public string SelectRGBW { get; set; } = "RGB";
        public bool Enable { get; set; } = true;
        public string Description { get; set; } = string.Empty;
    }
} 
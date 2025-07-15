using Core.Models;

namespace Core.Dtos
{
    public class ConfigDto {
        public List<ConfigItem> Items { get; set; } = new List<ConfigItem>();
    }
}

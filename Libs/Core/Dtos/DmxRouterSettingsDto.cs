
namespace Core.Dtos
{
    public class DmxRouterSettingsDto {
        public string Ip {
            get; set;
        }
        public int Port {
            get; set;
        }
        public List<UniverseMapDto> Universes { get; set; } = new();
    }
}

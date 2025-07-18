namespace Core.Dtos
{
    public class AppConfigDto {
        public int ListeningPort {
            get; set;
        }
        public int ListeningUniverse {
            get; set;
        }
        public List<PatchMapEntryDto> PatchMap { get; set; } = new();
        public List<DmxRouterSettingsDto> Routers { get; set; } = new();
    }
}

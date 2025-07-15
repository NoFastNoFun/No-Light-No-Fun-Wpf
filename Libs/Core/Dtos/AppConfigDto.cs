namespace Core.Dtos
{
    public class AppConfigDto {
        public List<PatchMapEntryDto> PatchMap { get; set; } = new();
        public List<DmxRouterSettingsDto> Routers { get; set; } = new();
        public int ListeningPort { get; set; } = 8765;
        public int ListeningUniverse { get; set; } = 1;
    }
}

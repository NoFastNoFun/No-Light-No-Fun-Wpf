using System.Text.Json;
using Core.Dtos;

namespace Services.Config
{
    /// <summary>
    /// Interface pour un service JSON générique.
    /// </summary>
    public interface IJsonFileService<TDto>
        where TDto : new() {
        TDto Load();
        void Save(TDto dto);
        string Serialize(TDto dto);
    }

    /// <summary>
    /// Implémentation de IJsonFileService qui lit/écrit un fichier JSON.
    /// </summary>
    public class JsonFileConfigService<TDto> : IJsonFileService<TDto>
        where TDto : new() {
        readonly string _path;
        readonly JsonSerializerOptions _opts = new JsonSerializerOptions { WriteIndented = true };

        public JsonFileConfigService(string path) {
            _path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public TDto Load() {
            if (!File.Exists(_path))
                return new TDto();

            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<TDto>(json, _opts)
                   ?? new TDto();
        }

        public void Save(TDto dto) {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var json = JsonSerializer.Serialize(dto, _opts);
            File.WriteAllText(_path, json);
        }

        public string Serialize(TDto dto) {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));
            return JsonSerializer.Serialize(dto, _opts);
        }
    }
}

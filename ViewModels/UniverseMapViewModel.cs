using Core.Dtos;
using Core.Models;

namespace No_Fast_No_Fun_Wpf.ViewModels {
    public class UniverseMapViewModel : BaseViewModel {
        private int _entityIdStart;
        public int EntityIdStart {
            get => _entityIdStart;
            set => SetProperty(ref _entityIdStart, value);
        }

        private int _entityIdEnd;
        public int EntityIdEnd {
            get => _entityIdEnd;
            set => SetProperty(ref _entityIdEnd, value);
        }

        private byte _universe;
        public byte Universe {
            get => _universe;
            set => SetProperty(ref _universe, value);
        }

        private int _startAddress;
        public int StartAddress {
            get => _startAddress;
            set => SetProperty(ref _startAddress, value);
        }

        // Constructeur "empty" pour ajout manuel (DataGrid etc)
        public UniverseMapViewModel() {
        }

        // Constructeur depuis UniverseMapDto (chargement depuis fichier JSON)
        public UniverseMapViewModel(UniverseMapDto dto) {
            EntityIdStart = dto.EntityStart;
            EntityIdEnd = dto.EntityEnd;
            Universe = dto.Universe;
            StartAddress = dto.StartAddress;
        }

        // Constructeur depuis le modèle (optionnel, pour logique avancée)
        public UniverseMapViewModel(UniverseMap model) {
            EntityIdStart = model.EntityIdStart;
            EntityIdEnd = model.EntityIdEnd;
            Universe = model.Universe;
            StartAddress = model.StartAddress;
        }

        // Conversion vers DTO pour sauvegarde
        public UniverseMapDto ToDto() {
            return new UniverseMapDto {
                EntityStart = this.EntityIdStart,
                EntityEnd = this.EntityIdEnd,
                Universe = this.Universe,
                StartAddress = this.StartAddress
            };
        }

        // Conversion vers modèle (si tu en as besoin en code backend)
        public UniverseMap ToModel() {
            return new UniverseMap {
                EntityIdStart = this.EntityIdStart,
                EntityIdEnd = this.EntityIdEnd,
                Universe = this.Universe,
                StartAddress = this.StartAddress
            };
        }
    }
}

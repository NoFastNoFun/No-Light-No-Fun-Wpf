using Core.Dtos;

namespace Core.Models
{
    public class UniverseMap {
        public int EntityIdStart {
            get; set;
        }
        public int EntityIdEnd {
            get; set;
        }
        public byte Universe {
            get; set;
        }
    
        public int StartAddress {
            get; set;
        }
        public static UniverseMap FromDto(UniverseMapDto dto) {
            return new UniverseMap {
                EntityIdStart = dto.EntityStart,
                EntityIdEnd = dto.EntityEnd,
                Universe = dto.Universe,
                StartAddress = dto.StartAddress
                
            };
        }
    }
}

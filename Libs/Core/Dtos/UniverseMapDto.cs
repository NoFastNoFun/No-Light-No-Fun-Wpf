using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Dtos
{
    public class UniverseMapDto {
        public int EntityStart {
            get; set;
        }
        public int EntityEnd {
            get; set;
        }
        public byte Universe {
            get; set;
        }
        public int StartAddress {
            get; set;
        }
    }
}

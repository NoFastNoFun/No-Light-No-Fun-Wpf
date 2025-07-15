using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Dtos
{
    public class EntityJsonDto
    {
        public int Id {
            get; set;
        }
        public string Name {
            get; set;
        }
        public string Strip {
            get; set;
        }
        public double X {
            get; set;
        }
        public double Y {
            get; set;
        }
        public double Z {
            get; set;
        }
    }
}

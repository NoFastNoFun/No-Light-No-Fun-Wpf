﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class StreamEntry {
        public string Name {
            get; set;
        }
        public string Url {
            get; set;
        }
        public bool IsActive {
            get; set;
        }
    }
}

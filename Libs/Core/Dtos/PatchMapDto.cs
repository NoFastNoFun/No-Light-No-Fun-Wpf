﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Dtos
{
    public class PatchMapDto {
        public List<PatchMapEntryDto> Items { get; set; } = new List<PatchMapEntryDto>();
    }
}

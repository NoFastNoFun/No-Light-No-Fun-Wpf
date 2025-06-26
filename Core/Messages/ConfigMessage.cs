using System;
using System.Collections.Generic;
using System.Reflection;

namespace No_Fast_No_Fun_Wpf.Core.Messages {
    public class ConfigMessage {
        public IReadOnlyList<Models.ConfigItem> Items {
            get;
        }

        ConfigMessage(List<Models.ConfigItem> items) => Items = items;

        // data starts at buffer[offset]
        public static ConfigMessage Parse(byte[] buffer, int offset = 6) {
            var items = new List<Models.ConfigItem>();
            ushort count = BitConverter.ToUInt16(buffer, offset);
            offset += 2;
            for (int i = 0; i < count; i++) {
                ushort sIdx = BitConverter.ToUInt16(buffer, offset);
                offset += 2;
                ushort sId = BitConverter.ToUInt16(buffer, offset);
                offset += 2;
                ushort eIdx = BitConverter.ToUInt16(buffer, offset);
                offset += 2;
                ushort eId = BitConverter.ToUInt16(buffer, offset);
                offset += 2;
                items.Add(new Models.ConfigItem(sIdx, sId, eIdx, eId));
            }
            return new ConfigMessage(items);
        }
    }
}

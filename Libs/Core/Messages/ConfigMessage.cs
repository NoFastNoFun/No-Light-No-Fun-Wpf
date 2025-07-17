using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Core.Models;

namespace Core.Messages {
    public class ConfigMessage {
        public IReadOnlyList<ConfigItem> Items {
            get;
        }

        public ConfigMessage(List<ConfigItem> items) {
            Items = items ?? throw new ArgumentNullException(nameof(items));
        }

        // Nouveau format binaire : ushort start, ushort end, byte universe, byte lenIP, bytes IP (UTF8)
        public static ConfigMessage Parse(byte[] buffer, int offset = 6) {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset + 2 > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            var items = new List<ConfigItem>();
            ushort count = BitConverter.ToUInt16(buffer, offset);
            offset += 2;

            // Chaque item = 8 octets (ushort startIndex, ushort startId, ushort endIndex, ushort endId)
            int itemSize = 8;

            for (int i = 0; i < count; i++) {
                // ... ici, tu parses le buffer
                ushort startIndex = BitConverter.ToUInt16(buffer, offset);
                offset += 2;
                ushort startId = BitConverter.ToUInt16(buffer, offset);
                offset += 2;
                ushort endIndex = BitConverter.ToUInt16(buffer, offset);
                offset += 2;
                ushort endId = BitConverter.ToUInt16(buffer, offset);
                offset += 2;

                // Pour compatibilité avec ta structure :
                ushort startEntityId = startId;
                ushort endEntityId = endId;
                byte universe = 1;
                string controllerIp = "";

                items.Add(new ConfigItem(startEntityId, endEntityId, universe, controllerIp));
            }

            return new ConfigMessage(items);
        }

    }
}

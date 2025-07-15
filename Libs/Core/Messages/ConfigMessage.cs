using System;
using System.Collections.Generic;
using Core.Models;

namespace Core.Messages {
    public class ConfigMessage {
        public IReadOnlyList<ConfigItem> Items {
            get;
        }

        // Le constructeur doit être public pour que Parse puisse l'appeler
        public ConfigMessage(List<ConfigItem> items) {
            Items = items ?? throw new ArgumentNullException(nameof(items));
        }

        // Décodage brut du buffer UDP (offset 6 pour sauter l'en-tête)
        public static ConfigMessage Parse(byte[] buffer, int offset = 6) {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset + 2 > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            var items = new List<ConfigItem>();
            ushort count = BitConverter.ToUInt16(buffer, offset);
            offset += 2;

            for (int i = 0; i < count; i++) {
                if (offset + 8 > buffer.Length)
                    throw new ArgumentException("Buffer trop petit pour lire tous les ConfigItems");

                ushort startIndex = BitConverter.ToUInt16(buffer, offset);
                offset += 2;
                ushort startId = BitConverter.ToUInt16(buffer, offset);
                offset += 2;
                ushort endIndex = BitConverter.ToUInt16(buffer, offset);
                offset += 2;
                ushort endId = BitConverter.ToUInt16(buffer, offset);
                offset += 2;

                items.Add(new ConfigItem(startIndex, startId, endIndex, endId));
            }

            return new ConfigMessage(items);
        }
    }
}

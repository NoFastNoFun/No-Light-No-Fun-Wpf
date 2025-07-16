using System;
using System.Collections.Generic;
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

            for (int i = 0; i < count; i++) {
                if (offset + 5 > buffer.Length)
                    throw new ArgumentException("Buffer trop petit pour lire ConfigItem");

                ushort startEntityId = BitConverter.ToUInt16(buffer, offset);
                offset += 2;
                ushort endEntityId = BitConverter.ToUInt16(buffer, offset);
                offset += 2;
                byte universe = buffer[offset];
                offset += 1;

                byte ipLength = buffer[offset];
                offset += 1;
                if (offset + ipLength > buffer.Length)
                    throw new ArgumentException("Buffer trop petit pour lire ControllerIp");

                string controllerIp = Encoding.UTF8.GetString(buffer, offset, ipLength);
                offset += ipLength;

                items.Add(new ConfigItem(startEntityId, endEntityId, universe, controllerIp));
            }

            return new ConfigMessage(items);
        }
    }
}

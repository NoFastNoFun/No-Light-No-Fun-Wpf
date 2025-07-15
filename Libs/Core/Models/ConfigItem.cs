namespace Core.Models {
    public class ConfigItem {
        // Nouveau constructeur officiel
        public ConfigItem(ushort startEntityId, ushort endEntityId,
                          byte startUniverse, byte endUniverse,
                          string controllerIp) {
            StartEntityId = startEntityId;
            EndEntityId = endEntityId;
            StartUniverse = startUniverse;
            EndUniverse = endUniverse;
            ControllerIp = controllerIp;
        }

        // Ancienne surcharge pour eHub
        public ConfigItem(ushort startIndex, ushort startId,
                          ushort endIndex, ushort endId)
            : this(startIndex, endIndex,
                   startUniverse: 0, endUniverse: 0,
                   controllerIp: string.Empty) {
        }

        public ushort StartEntityId {
            get; set;
        }
        public ushort EndEntityId {
            get; set;
        }
        public byte StartUniverse {
            get; set;
        }
        public byte EndUniverse {
            get; set;
        }
        public string ControllerIp {
            get; set;
        }

        public ConfigItem() {
        }
    }
}

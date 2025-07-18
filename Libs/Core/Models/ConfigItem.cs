namespace Core.Models {
    public class ConfigItem {
        // Nouveau constructeur officiel
        public ConfigItem(ushort startEntityId, ushort endEntityId, byte universe, string controllerIp) {
            StartEntityId = startEntityId;
            EndEntityId = endEntityId;
            Universe = universe;
            ControllerIp = controllerIp;
        }

        public ushort StartEntityId {
            get; set;
        }
        public ushort EndEntityId {
            get; set;
        }
        public byte Universe {
            get; set;
        }
        public string ControllerIp {
            get; set;
        }


        public ConfigItem() {
        }
    }
}

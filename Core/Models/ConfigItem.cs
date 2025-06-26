namespace No_Fast_No_Fun_Wpf.Core.Models {
    public class ConfigItem {
        public ushort StartIndex {
            get;
        }
        public ushort StartId {
            get;
        }
        public ushort EndIndex {
            get;
        }
        public ushort EndId {
            get;
        }

        public ConfigItem(ushort startIndex, ushort startId, ushort endIndex, ushort endId) {
            StartIndex = startIndex;
            StartId = startId;
            EndIndex = endIndex;
            EndId = endId;
        }
    }
}

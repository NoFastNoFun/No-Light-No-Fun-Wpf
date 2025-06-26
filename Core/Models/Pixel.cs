namespace No_Fast_No_Fun_Wpf.Core.Models {
    public class Pixel {
        public ushort Entity {
            get;
        }
        public byte R {
            get;
        }
        public byte G {
            get;
        }
        public byte B {
            get;
        }

        public Pixel(ushort entity, byte r, byte g, byte b) {
            Entity = entity;
            R = r;
            G = g;
            B = b;
        }
    }
}

using System.IO.Compression;
using Core.Models;

namespace Core.Messages {
    public class UpdateMessage {
        public IReadOnlyList<Pixel> Pixels {
            get;
        }

        public UpdateMessage(List<Pixel> pixels) => Pixels = pixels;

        public static UpdateMessage Parse(byte[] buffer, int offset = 6) {
            ushort pixelCount = BitConverter.ToUInt16(buffer, offset);
            offset += 2;
            ushort compressedLen = BitConverter.ToUInt16(buffer, offset);
            offset += 2;

            byte[] raw;
            using (var msIn = new MemoryStream(buffer, offset, compressedLen))
            using (var gz = new GZipStream(msIn, CompressionMode.Decompress))
            using (var msOut = new MemoryStream()) {
                gz.CopyTo(msOut);
                raw = msOut.ToArray();
            }

            var pixels = new List<Pixel>(pixelCount);
            for (int i = 0; i < pixelCount; i++) {
                int idx = i * 5;
                ushort entity = BitConverter.ToUInt16(raw, idx);
                byte r = raw[idx + 2], g = raw[idx + 3], b = raw[idx + 4];
                pixels.Add(new Pixel(entity, r, g, b));
            }
            return new UpdateMessage(pixels);
        }
    }
}

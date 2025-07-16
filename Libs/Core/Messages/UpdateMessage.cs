using System.IO.Compression;
using Core.Models;

namespace Core.Messages {
    public class UpdateMessage {
        public IReadOnlyList<Pixel> Pixels {
            get;
        }

        public UpdateMessage(List<Pixel> pixels) => Pixels = pixels;

        public static UpdateMessage Parse(byte[] buffer, int offset = 6) {
            var pixels = new List<Pixel>();
            int pixelSize = 5;
            int count = (buffer.Length - offset) / pixelSize;
            for (int i = 0; i < count; i++) {
                int idx = offset + i * pixelSize;
                if (idx + pixelSize > buffer.Length)
                    break; // Ignore les pixels incomplets
                ushort entity = BitConverter.ToUInt16(buffer, idx);
                byte r = buffer[idx + 2];
                byte g = buffer[idx + 3];
                byte b = buffer[idx + 4];
                if (entity < 100 || entity > 19858)
                    continue;
                try {
                    pixels.Add(new Pixel(entity, r, g, b));
                } catch (ArgumentException ex) {
                    System.Diagnostics.Debug.WriteLine($"[CRASH PIXEL] entity={entity} r={r} g={g} b={b} : {ex.Message}");
                    throw;
                }
            }
            return new UpdateMessage(pixels);
        }
    }
}

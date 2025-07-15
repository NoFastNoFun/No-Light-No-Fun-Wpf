using System;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic; // Added missing import

namespace TestEhubSender {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("=== eHuB Protocol Test Sender (Unity Simulation) ===");
            Console.WriteLine();
            Console.WriteLine("This simulates Unity sending eHuB protocol packets:");
            Console.WriteLine("1. eHuB header (4 bytes)");
            Console.WriteLine("2. Packet type (2 bytes)");
            Console.WriteLine("3. Compressed pixel data");
            Console.WriteLine();
            Console.WriteLine("Make sure the desktop application is running first!");
            Console.WriteLine("Press any key to start sending eHuB packets...");
            Console.ReadKey();
            
            using var client = new UdpClient();
            var endpoint = new IPEndPoint(IPAddress.Loopback, 8765);
            
            Console.WriteLine("Sending eHuB packets...");
            
            int packetCounter = 0;
            while (true) {
                // Create test pixels
                var pixels = new List<(ushort entity, byte r, byte g, byte b)> {
                    (100, 255, 0, 0),      // Red
                    (101, 0, 255, 0),      // Green  
                    (102, 0, 0, 255),      // Blue
                    (103, 255, 255, 0),    // Yellow
                    (104, 255, 0, 255),    // Magenta
                    (105, 0, 255, 255),    // Cyan
                };
                
                // Send Update message (type 1)
                var updatePacket = CreateEhubUpdatePacket(pixels);
                client.Send(updatePacket, updatePacket.Length, endpoint);
                
                Console.WriteLine($"Sent eHuB Update packet {packetCounter + 1} with {pixels.Count} pixels");
                
                packetCounter++;
                Thread.Sleep(1000); // Send every second
            }
        }
        
        static byte[] CreateEhubUpdatePacket(List<(ushort entity, byte r, byte g, byte b)> pixels) {
            // 1. Create raw pixel data
            var rawData = new List<byte>();
            foreach (var (entity, r, g, b) in pixels) {
                rawData.AddRange(BitConverter.GetBytes(entity)); // 2 bytes entity
                rawData.Add(r); // 1 byte R
                rawData.Add(g); // 1 byte G
                rawData.Add(b); // 1 byte B
            }
            
            // 2. Compress the data
            byte[] compressedData;
            using (var ms = new MemoryStream()) {
                using (var gz = new GZipStream(ms, CompressionMode.Compress)) {
                    gz.Write(rawData.ToArray(), 0, rawData.Count);
                }
                compressedData = ms.ToArray();
            }
            
            // 3. Build eHuB packet
            var packet = new List<byte>();
            
            // eHuB header (4 bytes)
            packet.AddRange(Encoding.ASCII.GetBytes("eHuB"));
            
            // Packet type: Update = 1 (2 bytes, big endian)
            packet.Add(0);
            packet.Add(1);
            
            // Pixel count (2 bytes, big endian)
            packet.Add((byte)(pixels.Count >> 8));
            packet.Add((byte)(pixels.Count & 0xFF));
            
            // Compressed data length (2 bytes, big endian)
            packet.Add((byte)(compressedData.Length >> 8));
            packet.Add((byte)(compressedData.Length & 0xFF));
            
            // Compressed data
            packet.AddRange(compressedData);
            
            return packet.ToArray();
        }
        
        static byte[] CreateEhubConfigPacket() {
            var packet = new List<byte>();
            
            // eHuB header (4 bytes)
            packet.AddRange(Encoding.ASCII.GetBytes("eHuB"));
            
            // Packet type: Config = 2 (2 bytes, big endian)
            packet.Add(0);
            packet.Add(2);
            
            // Config data would go here...
            // For now, just return basic packet
            return packet.ToArray();
        }
    }
} 
using System;

namespace TestPacketParsing {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Testing UDP packet parsing logic...");
            
            // Test packet: Entity 100, RGB(255, 128, 64)
            byte[] testPacket = {
                0, 0, 0, 100,  // Entity ID 100 in big endian
                255, 128, 64   // RGB values
            };
            
            // Parse like the desktop version does
            uint entityId = (uint)((testPacket[0] << 24) | (testPacket[1] << 16) | (testPacket[2] << 8) | testPacket[3]);
            byte r = testPacket[4];
            byte g = testPacket[5];
            byte b = testPacket[6];
            
            Console.WriteLine($"Parsed: Entity={entityId}, RGB({r},{g},{b})");
            Console.WriteLine("Expected: Entity=100, RGB(255,128,64)");
            
            if (entityId == 100 && r == 255 && g == 128 && b == 64) {
                Console.WriteLine("✅ Packet parsing is correct!");
            } else {
                Console.WriteLine("❌ Packet parsing is incorrect!");
            }
            
            // Test backend-style parsing for comparison
            uint entityIdBackend = BitConverter.ToUInt32(testPacket, 0);
            if (BitConverter.IsLittleEndian) {
                // Convert from little endian to big endian
                entityIdBackend = (uint)((entityIdBackend << 24) | ((entityIdBackend & 0xFF00) << 8) | 
                                       ((entityIdBackend & 0xFF0000) >> 8) | (entityIdBackend >> 24));
            }
            
            Console.WriteLine($"Backend-style parsing: Entity={entityIdBackend}");
            
            if (entityId == entityIdBackend) {
                Console.WriteLine("✅ Both parsing methods match!");
            } else {
                Console.WriteLine("❌ Parsing methods don't match!");
            }
        }
    }
} 
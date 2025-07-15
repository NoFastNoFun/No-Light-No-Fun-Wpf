using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TestCompleteFlow {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("=== Desktop LED Matrix - Complete Flow Test ===");
            Console.WriteLine();
            Console.WriteLine("This test verifies the complete flow:");
            Console.WriteLine("1. UDP packet reception");
            Console.WriteLine("2. Packet parsing");
            Console.WriteLine("3. DMX routing");
            Console.WriteLine("4. Art-Net output");
            Console.WriteLine();
            Console.WriteLine("Make sure the desktop application is running first!");
            Console.WriteLine("Press any key to start sending test packets...");
            Console.ReadKey();
            
            using var client = new UdpClient();
            var endpoint = new IPEndPoint(IPAddress.Loopback, 8765);
            
            Console.WriteLine("Sending test packets...");
            
            // Test different entity ranges to verify routing
            var testCases = new[] {
                (100, 255, 0, 0),      // Router 1, Universe 0
                (2000, 0, 255, 0),     // Router 1, Universe 15  
                (5100, 0, 0, 255),     // Router 2, Universe 32
                (7000, 255, 255, 0),   // Router 2, Universe 47
                (10100, 255, 0, 255),  // Router 3, Universe 64
                (12000, 0, 255, 255),  // Router 3, Universe 79
                (15100, 255, 128, 0),  // Router 4, Universe 96
                (17000, 128, 0, 255)   // Router 4, Universe 111
            };
            
            foreach (var (entityId, r, g, b) in testCases) {
                var packet = CreatePacket(entityId, r, g, b);
                client.Send(packet, packet.Length, endpoint);
                
                Console.WriteLine($"Sent: Entity {entityId} = RGB({r},{g},{b})");
                Thread.Sleep(500); // Wait 500ms between packets
            }
            
            Console.WriteLine();
            Console.WriteLine("Test complete! Check the desktop application for:");
            Console.WriteLine("- UDP packet reception in console");
            Console.WriteLine("- DMX routing calculations");
            Console.WriteLine("- Art-Net frame transmission");
            Console.WriteLine("- UI preview updates");
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
        
        static byte[] CreatePacket(int entityId, byte r, byte g, byte b) {
            var packet = new byte[7];
            
            // Entity ID in big endian (4 bytes)
            packet[0] = (byte)((entityId >> 24) & 0xFF);
            packet[1] = (byte)((entityId >> 16) & 0xFF);
            packet[2] = (byte)((entityId >> 8) & 0xFF);
            packet[3] = (byte)(entityId & 0xFF);
            
            // RGB values (3 bytes)
            packet[4] = r;
            packet[5] = g;
            packet[6] = b;
            
            return packet;
        }
    }
} 
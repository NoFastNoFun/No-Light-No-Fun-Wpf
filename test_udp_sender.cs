using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TestUdpSender {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("UDP Test Sender for Desktop LED Matrix");
            Console.WriteLine("Sending test packets to localhost:8765");
            
            using var client = new UdpClient();
            var endpoint = new IPEndPoint(IPAddress.Loopback, 8765);
            
            // Test packet format: 4 bytes entity ID (big endian) + 3 bytes RGB
            var testPackets = new byte[][] {
                CreatePacket(100, 255, 0, 0),    // Red
                CreatePacket(101, 0, 255, 0),    // Green  
                CreatePacket(102, 0, 0, 255),    // Blue
                CreatePacket(103, 255, 255, 0),  // Yellow
                CreatePacket(104, 255, 0, 255),  // Magenta
                CreatePacket(105, 0, 255, 255),  // Cyan
            };
            
            int packetIndex = 0;
            while (true) {
                var packet = testPackets[packetIndex % testPackets.Length];
                client.Send(packet, packet.Length, endpoint);
                
                Console.WriteLine($"Sent packet {packetIndex + 1}: Entity {100 + (packetIndex % 6)}, RGB({packet[4]},{packet[5]},{packet[6]})");
                
                packetIndex++;
                Thread.Sleep(1000); // Send every second
            }
        }
        
        static byte[] CreatePacket(uint entityId, byte r, byte g, byte b) {
            var packet = new byte[7];
            
            // Entity ID in big endian (4 bytes)
            packet[0] = (byte)(entityId >> 24);
            packet[1] = (byte)(entityId >> 16);
            packet[2] = (byte)(entityId >> 8);
            packet[3] = (byte)(entityId);
            
            // RGB values (3 bytes)
            packet[4] = r;
            packet[5] = g;
            packet[6] = b;
            
            return packet;
        }
    }
} 
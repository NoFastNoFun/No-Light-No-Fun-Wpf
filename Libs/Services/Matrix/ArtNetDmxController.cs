using System.Net.Sockets;
using System.Text;

namespace Services.Matrix
{
    public class ArtNetDmxController : IDisposable {
        private readonly UdpClient _udp;
        public delegate void FrameSentHandler(string ip, byte universe, int length);
        public event FrameSentHandler? FrameSent;

        public ArtNetDmxController() {
            _udp = new UdpClient();
            _udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _udp.ExclusiveAddressUse = false;
        }

        /// <summary>
        /// Envoie une trame DMX sur une IP et un port Art-Net donnés.
        /// </summary>
        /// <param name="ip">Adresse IP du nœud Art-Net.</param>
        /// <param name="port">Port UDP (généralement 6454).</param>
        /// <param name="universe">Numéro d'univers DMX (0-255).</param>
        /// <param name="data">Données DMX (512 octets maximum).</param>
        public void SendDmxFrame(string ip, int port, byte universe, byte[] data) {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length > 512)
                throw new ArgumentException("DMX data length cannot exceed 512 bytes.");

            var packet = BuildArtDmxPacket(universe, data);
            _udp.Send(packet, packet.Length, ip, port);

            // Ici on notifie les abonnés
            FrameSent?.Invoke(ip, universe, packet.Length);
        }


        private byte[] BuildArtDmxPacket(byte universe, byte[] dmxData) {
            // En-tête "Art-Net" suivi de \0
            var header = Encoding.ASCII.GetBytes("Art-Net\0");
            // Op-code OpDmx = 0x5000 (LSB first)
            byte[] opDmx = { 0x00, 0x50 };
            // Version hi/lo
            byte protVerHi = 0x00;
            byte protVerLo = 0x0E;
            // Sequence, Physical
            byte sequence = 0;
            byte physical = 0;
            // Universe (LSB, MSB)
            byte uniLo = universe;
            byte uniHi = 0;
            // Longueur du payload
            int length = dmxData.Length;
            byte lenHi = (byte)(length >> 8);
            byte lenLo = (byte)(length & 0xFF);

            var packet = new byte[header.Length + opDmx.Length + 8 + dmxData.Length];
            int offset = 0;

            // Copy header
            Buffer.BlockCopy(header, 0, packet, offset, header.Length);
            offset += header.Length;
            // Copy opDmx
            Buffer.BlockCopy(opDmx, 0, packet, offset, opDmx.Length);
            offset += opDmx.Length;
            // Protocol version
            packet[offset++] = protVerHi;
            packet[offset++] = protVerLo;
            // Sequence, Physical
            packet[offset++] = sequence;
            packet[offset++] = physical;
            // Universe
            packet[offset++] = uniLo;
            packet[offset++] = uniHi;
            // Length hi/lo
            packet[offset++] = lenHi;
            packet[offset++] = lenLo;
            // DMX data
            Buffer.BlockCopy(dmxData, 0, packet, offset, dmxData.Length);

            return packet;
        }

        public void Dispose() {
            _udp?.Close();
        }
    }
}

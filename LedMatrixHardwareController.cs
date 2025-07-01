using System;
using System.Collections.Generic;
using System.IO.Ports;
using Core.Models;


namespace No_Fast_No_Fun_Wpf.Services.Matrix {
    /// <summary>  
    /// Contrôleur matériel : envoie directement les octets R,G,B via port série.  
    /// </summary>  
    public class LedMatrixHardwareController : ILedMatrixController, IDisposable {
        public (int Width, int Height) Size {
            get;
        }
        private readonly SerialPort _serialPort;

        public LedMatrixHardwareController(int width, int height, string portName, int baudRate = 115200) {
            Size = (width, height);
            _serialPort = new SerialPort(portName, baudRate) {
                // tu peux configurer Parity, StopBits, etc. ici  
                WriteTimeout = 500
            };
            _serialPort.Open();
        }

        public void ApplyFrame(IEnumerable<Pixel> pixels) {
            var buffer = new List<byte>();
            foreach (var p in pixels) {
                // envoi de l'ID entité en 2 bytes little-endian puis R,G,B  
                buffer.AddRange(BitConverter.GetBytes(p.Entity));
                buffer.Add(p.R);
                buffer.Add(p.G);
                buffer.Add(p.B);
            }

            if (_serialPort.IsOpen)
                _serialPort.Write(buffer.ToArray(), 0, buffer.Count);
        }

        public void Dispose() {
            if (_serialPort.IsOpen)
                _serialPort.Close();
            _serialPort.Dispose();
        }
    }
}

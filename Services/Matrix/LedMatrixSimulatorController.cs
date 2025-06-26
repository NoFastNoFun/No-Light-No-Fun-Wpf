using System;
using System.Collections.Generic;
using No_Fast_No_Fun_Wpf.Core.Models;

namespace No_Fast_No_Fun_Wpf.Services.Matrix {
    /// <summary>
    /// Contrôleur de simulation WPF : lève un événement à chaque frame reçue.
    /// </summary>
    public class LedMatrixSimulatorController : ILedMatrixController {
        public (int Width, int Height) Size {
            get;
        }

        /// <summary>
        /// Event levé quand une nouvelle frame arrive, pour que l'UI l'affiche.
        /// </summary>
        public event Action<IEnumerable<Pixel>> FrameUpdated;

        public LedMatrixSimulatorController(int width, int height) {
            Size = (width, height);
        }

        public void ApplyFrame(IEnumerable<Pixel> pixels) {
            FrameUpdated?.Invoke(pixels);
        }
    }
}

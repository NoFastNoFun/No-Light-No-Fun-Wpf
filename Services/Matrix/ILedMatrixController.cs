using System.Collections.Generic;
using No_Fast_No_Fun_Wpf.Core.Models;

namespace No_Fast_No_Fun_Wpf.Services.Matrix {
    /// <summary>
    /// Abstraction d’une matrice LED : taille + application d’une frame de pixels.
    /// </summary>
    public interface ILedMatrixController {
        /// <summary>
        /// Dimensions de la matrice (colonnes, lignes).
        /// </summary>
        (int Width, int Height) Size {
            get;
        }

        /// <summary>
        /// Envoie une nouvelle frame (liste de pixels) à la matrice ou au simulateur.
        /// </summary>
        void ApplyFrame(IEnumerable<Pixel> pixels);
    }
}

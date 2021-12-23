using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FractalMachineLib
{
    public class Linear
    {
        public List<Linear> Instructions = new List<Linear>();
        public List<Reader.Piece> Arguments = new List<Reader.Piece>();

        // Ancillary informations
        public Reader.Piece Piece;
    }
}

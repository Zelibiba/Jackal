using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    internal interface IOrientable
    {
        abstract List<Orientation> Orientations { get; }

        int Rotate(int rotation)
        {
            if (rotation == 0)
                return 0;

            if (rotation < 0 || rotation > 3)
                throw new ArgumentException("Wrong cell rotation!");

            for (int i = 0; i < Orientations.Count; i++)
            {
                int orient = (Orientations[i] == Orientation.LeftUp) ? 24 : (int)Orientations[i];
                orient *= (int)Math.Pow(2, rotation);
                if (orient / 16 != 0 && orient != 24)
                    orient /= 16;
                Orientations[i] = (orient == 24) ? Orientation.LeftUp : (Orientation)orient;
            }

            return rotation * 90;
        }
    }
}

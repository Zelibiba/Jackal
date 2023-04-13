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
        int Rotate(int rotation, ref List<Orientation> orientations)
        {
            if (rotation == 0)
                return 0;

            if (rotation < 0 || rotation > 3)
                throw new ArgumentException("Wrong cell rotation!");

            for (int i = 0; i < orientations.Count; i++)
            {
                int orient = (orientations[i] == Orientation.LeftUp) ? 24 : (int)orientations[i];
                orient *= (int)Math.Pow(2, rotation);
                if (orient / 16 != 0 && orient != 24)
                    orient /= 16;
                orientations[i] = (orient == 24) ? Orientation.LeftUp : (Orientation)orient;
            }

            return rotation * 90;
        }
    }
}

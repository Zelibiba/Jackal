using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jackal.Models.Pirates;

namespace Jackal.Models.Cells
{
    public class ResidentCell : Cell
    {
        public ResidentCell(int row, int column, ResidentType type) : base(row, column, type.ToString())
        {
            _type = type;
        }

        readonly ResidentType _type;

        public override void Open()
        {
            base.Open();
            Player owner = Pirates[0].Owner;
            switch (_type)
            {
                case ResidentType.Ben:
                    AddPirate(new Ben(owner)); break;
                case ResidentType.Friday:
                    AddPirate(new Friday(owner)); break;
                case ResidentType.Missioner:
                    AddPirate(new Missioner(owner)); break;
            }
        }
    }
}

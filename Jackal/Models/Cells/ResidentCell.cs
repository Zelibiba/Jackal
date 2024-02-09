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
            Player manager = Pirates[0].Manager;
            Pirate pirate = _type switch
            {
                ResidentType.Ben => new Ben(this, owner, manager),
                ResidentType.Friday => new Friday(this, owner, manager),
                ResidentType.Missioner => new Missioner(this, owner, manager),
                _ => throw new NotImplementedException()
            };
            AddPirate(pirate, 0);
        }
    }
}

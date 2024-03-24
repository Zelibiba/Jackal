using Jackal.Models.Pirates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class FortressCell : Cell
    {
        public FortressCell(int row, int column, bool putana) : base(row, column, putana ? "Putana" : "Fortress")
        {
            Putana = putana;
            if (Putana)
                enterSound = Sounds.Putana;
        }

        /// <summary>
        /// Флаг того, что крепость содержит путану.
        /// </summary>
        public bool Putana { get; }

        public override void Open()
        {
            base.Open();
            enterSound = Sounds.Usual;
        }

        public override bool IsGoldFriendly(Pirate pirate) => false;
        public override bool CanBeSelectedBy(Pirate pirate) => IsFriendlyTo(pirate);
    }
}

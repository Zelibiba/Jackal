using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells.Utilites
{
    public class NodeCell : Cell
    {
        public NodeCell(NodeOwnerCell owner, bool isStandable = true, int number = 0) : base(owner.Row, owner.Column, owner.Image, isStandable, number: number)
        {
            _owner = owner;
        }

        protected readonly NodeOwnerCell _owner;

        public override void Open()
        {
            base.Open();
            if (!_owner.IsOpened)
                _owner.Open();
        }
    }
}

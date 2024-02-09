using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells.Utilites
{
    public class NodeOwnerCell : Cell
    {
        public NodeOwnerCell(int row, int column, string image) : base(row, column, image, number: 0)
        {
            IObservable<IChangeSet<Cell>> nodesSet = Nodes.ToObservableChangeSet();

            nodesSet.AutoRefresh(cell => cell.Gold)
                    .Subscribe(_ => Gold = Nodes.Sum(x => x.Gold));
            nodesSet.AutoRefresh(cell => cell.Galeon)
                    .Subscribe(_ => Galeon = Nodes.Sum(x => x.Galeon));

            nodesSet.AutoRefresh(cell => cell.IsPreOpened)
                    .Subscribe(_ => IsPreOpened = Nodes.Any(x => x.IsPreOpened));

            nodesSet.AutoRefresh(cell => cell.CanBeSelected)
                    .Subscribe(_ => CanBeSelected = Nodes.Any(x => x.CanBeSelected));
            this.WhenAnyValue(cell => cell.CanBeSelected)
                .Where(x => !x)
                .Subscribe(_ =>
                {
                    foreach (Cell cell in Nodes)
                        cell.CanBeSelected = false;
                });
        }

        public override void Open()
        {
            base.Open();
            foreach (Cell cell in Nodes)
            {
                if (!cell.IsOpened)
                    cell.Open();
            }
        }

        public override void SetCoordinates(int row, int column)
        {
            base.SetCoordinates(row, column);
            foreach (Cell cell in Nodes)
                cell.SetCoordinates(row, column);
        }
        public override void SetSelectableCoords(Map map)
        {
            base.SetSelectableCoords(map);
            foreach (Cell cell in Nodes)
                cell.SetSelectableCoords(map);
        }
    }
}

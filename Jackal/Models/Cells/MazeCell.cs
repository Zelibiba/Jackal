using DynamicData;
using DynamicData.Binding;
using Jackal.Models.Pirates;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class MazeCell : Cell
    {
        public MazeCell(int row, int column, int mazeLevel) : base(row, column, "Maze" + mazeLevel)
        {
            if (mazeLevel < 2 || mazeLevel > 5)
                throw new ArgumentException("Wrong mazeLevel!");

            Nodes.Clear();
            for (int i = 0; i < mazeLevel; i++)
                Nodes.Add(new MazeNodeCell(Row, Column, this, i + 1));

            Nodes.ToObservableChangeSet()
                 .AutoRefresh(cell => cell.Gold)
                 .Subscribe(_ =>
                 {
                     int summ = 0;
                     foreach (Cell cell in Nodes)
                         summ += cell.Gold;
                     Gold = summ;
                 });
            Nodes.ToObservableChangeSet()
                 .AutoRefresh(cell => cell.Galeon)
                 .Select(_ =>
                 {
                     foreach (Cell cell in Nodes)
                     {
                         if (cell.Galeon)
                             return true;
                     }
                     return false;
                 }).Subscribe(x => Galeon = x);
            Nodes.ToObservableChangeSet()
                 .AutoRefresh(cell => cell.CanBeSelected)
                 .Subscribe(_ =>
                 {
                     foreach (Cell cell in Nodes)
                     {
                         if (cell.CanBeSelected)
                         {
                             CanBeSelected = true;
                             break;
                         }
                     }
                 });
            this.WhenAnyValue(cell => cell.CanBeSelected)
                .Where(x => !x)
                .Subscribe(_ =>
                {
                    foreach (Cell cell in Nodes)
                        cell.CanBeSelected = false;
                });
        }


        public override Cell GetSelectedCell(Pirate pirate)
        {
            int nodeNumber = Pirates.Contains(pirate) ? pirate.MazeNodeNumber : 0;
            return Nodes[nodeNumber];
        }

        public override void Open()
        {
            base.Open();
            foreach (Cell cell in Nodes)
                cell.Open();
        }

        public override void SetCoordinates(int row, int column)
        {
            base.SetCoordinates(row, column);
            foreach (Cell cell in Nodes)
                cell.SetCoordinates(row, column);
        }
        public override void SetSelectableCoords(ObservableMap map)
        {
            foreach(Cell cell in Nodes)
                cell.SetSelectableCoords(map);
        }
    }
}

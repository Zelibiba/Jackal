using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData.Binding;
using Jackal.Models.Cells;

namespace Jackal.Models
{

    public static class Game
    {
        public static readonly int MapSize = 13;
        public static ObservableMap Map { get; private set; }


        public static void CreateMap()
        {
            Map = new ObservableMap(MapSize);
            for(int i = 0; i < MapSize; i++)
            {
                for (int j = 0; j < MapSize; j++)
                    Map.Add(new Cell(i, j, "Field"));
            }
            Map[5,5] = new Cell(0, 0, "Ben");
        }
    }
}

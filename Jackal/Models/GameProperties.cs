using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models
{
    public class GameProperties : ReactiveObject
    {
        [Reactive] public MapType MapType { get; set; } = MapType.Quadratic;
    }
}

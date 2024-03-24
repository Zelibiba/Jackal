using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jackal.Models.Pirates;

namespace Jackal.Models.Cells
{
    public class ShipCell : Cell
    {
        public ShipCell(ShipPlacement shipPlacement, Player owner) : base(shipPlacement.InitialCoordinates.Row, shipPlacement.InitialCoordinates.Column, "Ship")
        {
            Open();
            _owner = owner;
            Manager = owner;
            _owner.SetShip(this);

            Directions = shipPlacement.Directions;
            ShipRegion = shipPlacement.Region;
            MovableCoords = new List<Coordinates>();
        }

        /// <summary>
        /// Направление выхода с корабля.
        /// </summary>
        readonly public Coordinates[] Directions;
        /// <summary>
        /// Координаты клеток, по которым корабль может ходить в целом.
        /// </summary>
        public readonly Coordinates[] ShipRegion;
        /// <summary>
        /// Координаты, на которые корабль может переместиться в данный момент.
        /// </summary>
        readonly public List<Coordinates> MovableCoords;

        public override bool IsShip => true;
        readonly Player _owner;
        /// <summary>
        /// Игрок, управляющий кораблём.
        /// </summary>
        public Player Manager { get; set; }

        protected override Team Team => _owner.Team;
        public override Team ShipTeam => _owner.Team;
        /// <summary>
        /// Флаг того, что корабль может перемещаться.
        /// </summary>
        public bool CanMove => Pirates.Any(pirate => pirate.IsFighter);

        public override int Gold
        {
            get => 0;
            set
            {
                _owner.Gold++;
                Game.CurrentGold--;
            }
        }
        public override int Galeon
        {
            get => 0;
            set
            {
                _owner.Gold += 3;
                Game.CurrentGold -= 3;
            }
        }

        public override void SetSelectableCoords(Map map)
        {
            SelectableCoords.Clear();
            SelectableCoords.AddRange(map.AdjacentCellsCoords(this, Directions));
            MovableCoords.Clear();
            MovableCoords.AddRange(ShipRegion.Where(coords => (coords - Coords).Abs() <= 1 && coords != Coords));
        }

        public override bool CanBeSelectedBy(Pirate pirate) => pirate.Cell is SeaCell || IsFriendlyTo(pirate);
        public override MovementResult AddPirate(Pirate pirate, int delay =0)
        {
            if (IsFriendlyTo(pirate))
            {
                enterSound = pirate.Treasure ? Sounds.GetGold : Sounds.Usual;
                base.AddPirate(pirate, delay);
                pirate.Gold = false;
                pirate.Galeon = false;
            }
            else
            {
                Game.AudioPlayer?.Play(Sounds.Kill);
                pirate.Cell = this;
                pirate.Kill();
            }

            return MovementResult.End;
        }
        public void AddHittedPirate(Pirate pirate)
        {
            enterSound = Sounds.Hit;
            base.AddPirate(pirate, delay: 150);
        }
    }
}

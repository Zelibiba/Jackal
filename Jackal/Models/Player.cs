using Jacal;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using System.IO;
using Jackal.Models.Pirates;
using Jackal.Models.Cells;
using DynamicData;
using System.Collections.ObjectModel;
using DynamicData.Binding;
using DynamicData.Aggregation;
using System.Reactive.Linq;

namespace Jackal.Models
{
    /// <summary>
    /// Класс описания Игрока.
    /// </summary>
    public class Player : ReactiveObject
    {
        /// <summary>
        /// Заглушка игрока для корректной работы <see cref="Pirate.Empty"/>
        /// </summary>
        public Player() { }


        /// <summary>
        /// конструктор игрока.
        /// </summary>
        /// <param name="index">Индекс игрока.</param>
        /// <param name="name">Имя игрока.</param>
        /// <param name="team">Команда игрока.</param>
        public Player(int index, string name, Team team)
        {
            Index = index;
            Name = name;
            IntAlliance = index;
            Team = team;

            Alliance = team;

            Pirates = new ObservableCollection<Pirate>();
            Pirates.ToObservableChangeSet()
                   .ToCollection()
                   .Select(pirates => pirates.Count(pirate => pirate.IsFighter) >= 3)
                   .ToPropertyEx(this, p => p.IsEnoughtPirates);
            Pirates.Add(new Pirate(this));
            Pirates.Add(new Pirate(this));
            Pirates.Add(new Pirate(this));
            Pirates.Add(new Friday(this));
        }

        /// <summary>
        /// Индекс игрока.
        /// </summary>
        /// <remarks>
        /// Необходим для определения игрока для сети.
        /// </remarks>
        public readonly int Index;
        /// <summary>
        /// Имя игрока.
        /// </summary>
        [Reactive] public string Name { get; set; }

        /// <summary>
        /// Команда игрока.
        /// </summary>
        [Reactive] public Team Team { get; set; }
        /// <summary>
        /// Номер альянса игрока.
        /// </summary>
        [Reactive] public int IntAlliance { get; set; }
        /// <summary>
        /// Объединение команд альянса игрока.
        /// </summary>
        public Team Alliance { get;private set; }

        /// <summary>
        /// Флаг того, что игрок готов к запуску игры.
        /// </summary>
        [Reactive] public bool IsReady { get; set; }
        /// <summary>
        /// Метод копирует основные параметры игрока.
        /// </summary>
        /// <param name="player">игрок, с котого копируют.</param>
        public void Copy(Player player)
        {
            Name = player.Name;
            IntAlliance = player.IntAlliance;
            Team = player.Team;
            IsReady = player.IsReady;
        }


        /// <summary>
        /// Лист пиратов, подкоторольных игроку.
        /// </summary>
        public ObservableCollection<Pirate> Pirates { get; }
        /// <summary>
        /// Флаг того, что у игрока нет возможности рожать пиратов.
        /// </summary>
        [ObservableAsProperty] public bool IsEnoughtPirates { get; }

        /// <summary>
        /// Корабль игрока.
        /// </summary>
        public ShipCell Ship { get; private set; }
        /// <summary>
        /// Метод для установки корабля игрока.
        /// </summary>
        /// <param name="ship">Устанавливаемый корабль.</param>
        public void SetShip(ShipCell ship)
        {
            Ship = ship;
            foreach (Pirate pirate in Pirates)
                Ship.AddPirate(pirate);
        }

        /// <summary>
        /// Флаг того, что игрок совершает ход в текущий момент.
        /// </summary>
        [Reactive] public bool Turn { get; set; }

        /// <summary>
        /// Количество сокровищ игрока.
        /// </summary>
        [Reactive] public int Gold { get; set; }
        /// <summary>
        /// Количество бутылок с ромом у игрока.
        /// </summary>
        [Reactive] public int Bottles { get; set; }
        /// <summary>
        /// Флаг того, что игрок может пользоваться ромом.
        /// </summary>
        public bool RumIsBlocked { get; set; }
    }
}

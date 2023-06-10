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
        /// <param name="isControllable">Флаг контролья данного игрока клиентом.</param>
        public Player(int index, string name, Team team, bool isControllable = false)
        {
            Index = index;
            Name = name;
            IntAlliance = index;
            Team = team;
            IsControllable = isControllable;

            Alliance = team;

            Pirates = new ObservableCollection<Pirate>();
            Pirates.ToObservableChangeSet()
                   .ToCollection()
                   .Select(pirates => pirates.Count(pirate => pirate.IsFighter) >= 3)
                   .ToPropertyEx(this, p => p.IsEnoughtPirates);
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
        /// Флаг того, что игрок контролируется клиентом.
        /// </summary>
        public bool IsControllable { get; }

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
        /// Корабль команды игрока.
        /// </summary>
        public ShipCell Ship { get; private set; }
        /// <summary>
        /// Корабль, управляемый игроком.
        /// </summary>
        public ShipCell ManagedShip { get; set; }
        /// <summary>
        /// Метод для установки корабля игрока.
        /// </summary>
        /// <param name="ship">Устанавливаемый корабль.</param>
        public void SetShip(ShipCell ship)
        {
            Ship = ship;
            ManagedShip = ship;
            for (int i = 0; i < 3; i++)
                ship.AddPirate(new Pirate(this));
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
        
        public bool CannabisStarter { get; set; }
        /// <summary>
        /// Флаг того, что игрок может пользоваться ромом.
        /// </summary>
        public bool IsRumBlocked { get; set; }
        /// <summary>
        /// Метод копирует список пиратов и управляемый корабль, а также изменяет блокировку рома.
        /// </summary>
        /// <param name="pirates">Спиок пиратов.</param>
        /// <param name="ship">Управляемый корабль.</param>
        /// <param name="blockRum">Флаг блокировки рома <see cref="IsRumBlocked"/>.</param>
        public void SetPiratesAndShip(IEnumerable<Pirate> pirates, ShipCell ship, bool blockRum)
        {
            Pirates.Clear();
            Pirates.AddRange(pirates);
            foreach (Pirate pirate in Pirates)
                pirate.Manager = this;
            ManagedShip = ship;
            ManagedShip.Manager = this;
            IsRumBlocked = blockRum;
        }
        /// <summary>
        /// <inheritdoc cref="SetPiratesAndShip(IEnumerable{Pirate}, ShipCell, bool)" path="/summary"/>
        /// </summary>
        /// <param name="soursePlayer">Игрок, предоставляющий пиратов и корабль для копирования.</param>
        /// <param name="blockRum"></param>
        public void SetPiratesAndShip(Player soursePlayer, bool blockRum)
        {
            SetPiratesAndShip(soursePlayer.Pirates, soursePlayer.ManagedShip, blockRum);
        }
    }
}

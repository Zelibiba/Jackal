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
using System.Drawing.Printing;

namespace Jackal.Models
{
    /// <summary>
    /// Класс описания Игрока.
    /// </summary>
    public class Player : ReactiveObject
    {
        /// <summary>
        /// Заглушка игрока для корректной работы <see cref="Pirate.Empty"/> и <see cref="Player.Ally"/>.
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
            Team = team;
            IsControllable = isControllable;

            Pirates = new ObservableCollection<Pirate>();
            Pirates.ToObservableChangeSet()
                   .ToCollection()
                   .Select(pirates => pirates.Count(pirate => pirate.IsFighter) >= 3)
                   .ToPropertyEx(this, p => p.IsEnoughtPirates);

            _allies = new ObservableCollection<Player>();
            _allies.ToObservableChangeSet()
                   .AutoRefresh(player => player.Bottles)
                   .ToCollection()
                   .Select(players => players.Any(p => p.Bottles > 0))
                   .ToPropertyEx(this, player => player.CanUseRum);
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
        /// Цвет альянса игрока.
        /// </summary>
        [Reactive] public AllianceIdentifier AllianceIdentifier { get; set; }
        /// <summary>
        /// Объединение команд альянса игрока.
        /// </summary>
        public Team Alliance { get; private set; }
        /// <summary>
        /// Список игроков в альянсе.
        /// </summary>
        /// <remarks>Необходим для совместного пользования ромом. Первый элемент - сам игрок.
        readonly ObservableCollection<Player> _allies;
        /// <summary>
        /// Метод задаёт альянс для игрока.
        /// </summary>
        /// <param name="allies">Список игроков в альянсе.</param>
        public void SetAlliance(IEnumerable<Player> allies)
        {
            Alliance = Team.None;
            foreach (Player player in allies)
                Alliance |= player.Team;

            _allies.Clear();
            _allies.Add(this);
            _allies.AddRange(allies.Except(_allies));
        }
        /// <summary>
        /// Метод возвращает команду для двойника в альянсе (Смещённая на 1 команда).
        /// </summary>
        /// <returns></returns>
        public Team GetAllyTeam()
        {
            if ((int)Team * 2 > 32) return (Team)((int)Team / 32);
            else return (Team)((int)Team * 2);
        }

        /// <summary>
        /// Флаг того, что игрок готов к запуску игры.
        /// </summary>
        [Reactive] public bool IsReady { get; set; }
        /// <summary>
        /// Метод копирует основные параметры игрока.
        /// </summary>
        /// <param name="player">Игрок, с котого копируют.</param>
        /// <param name="alliedTeam">Копировать команду не напрямую, а через <see cref="GetAllyTeam"/></param>
        public void Copy(Player player, bool alliedTeam = false)
        {
            Name = player.Name;
            AllianceIdentifier = player.AllianceIdentifier;
            Team = alliedTeam ? player.GetAllyTeam() : player.Team;
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
                ship.AddPirate(new Pirate(ship, this));
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
        /// Флаг того, что у альянса данного игрока есть бутылка.
        /// </summary>
        [ObservableAsProperty] public bool CanUseRum { get; }
        /// <summary>
        /// Метод уменьшает количество бутылок у альянса на одну.
        /// </summary>
        public void UseBottle() => _allies.First(p => p.Bottles > 0).Bottles--;

        /// <summary>
        /// Число разов того, что с этого игрока начался ход конопли.
        /// </summary>
        public int CannabisIndex { get; set; } = 0;
        /// <summary>
        /// Флаг того, что игрок может пользоваться ромом.
        /// </summary>
        [Reactive] public bool IsRumBlocked { get; set; }


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
        /// <param name="blockRum">Флаг блокировки рома <see cref="IsRumBlocked"/>.</param>
        public void SetPiratesAndShip(Player soursePlayer, bool blockRum)
        {
            SetPiratesAndShip(soursePlayer.Pirates, soursePlayer.ManagedShip, blockRum);
        }
    }
}

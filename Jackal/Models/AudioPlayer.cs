using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models
{
    /// <summary>
    /// Перечисление звуком.
    /// </summary>
    public enum Sounds
    {
        Usual,
        Sea,
        Gold,
        Galeon,
        GetGold,
        Gun,
        Pit,
        Ok,
        Hit,
        Kill,
        Bottles,
        Rum,
        Maze,
        FridayMissioner,
        DrunkMissioner,
        Missioner,
        Ben,
        Friday,
        Airplane,
        AirplaneEnter,
        Horse,
        Cannabis,
        EarthQuake,
        LightHouse,
        Putana
    }
    /// <summary>
    /// Класс Аудиоплеера.
    /// </summary>
    public class AudioPlayer : IDisposable
    {
        readonly LibVLC _libVLS;
        MediaPlayer _mediaPlayer;
        /// <summary>
        /// Словарь аудиофайлов.
        /// </summary>
        readonly Dictionary<Sounds, Media[]> _soundFiles;
        readonly Random _random;

        /// <summary>
        /// Класс Аудиоплеера.
        /// </summary>
        public AudioPlayer()
        {
            _libVLS = new("--reset-plugins-cache");
            DirectoryInfo dir = new(Properties.SoundsFolder);
            string[] filenames = (from file in dir.GetFiles("*.mp3")
                                  select file.Name.Split('.')[0]).ToArray();

            _soundFiles = new Dictionary<Sounds, Media[]>();
            foreach (Sounds sound in Enum.GetValues(typeof(Sounds)))
            {
                int count = filenames.Count(x => x.StartsWith($"{sound}_") || x == sound.ToString());
                _soundFiles[sound] = new Media[count];
                if (count == 1)
                    _soundFiles[sound][0] = new(_libVLS, new Uri(Path.Combine(Properties.SoundsFolder, $"{sound}.mp3")));
                else
                {
                    for (int i = 0; i < count; i++)
                        _soundFiles[sound][i] = new(_libVLS, new Uri(Path.Combine(Properties.SoundsFolder, $"{sound}_{i + 1}.mp3")));
                }
            }
            _mediaPlayer = new(_libVLS);
            _random = new();
        }

        /// <summary>
        /// Метод запускает проигрышь аудиофайла.
        /// </summary>
        /// <param name="sound"></param>
        public void Play(Sounds sound)
        {
            Media[] medias = _soundFiles[sound];
            int i = medias.Length == 1 ? 0 : _random.Next(0, medias.Length);
            _mediaPlayer = new(medias[i]);
            _mediaPlayer.Play();
        }

        public void Dispose()
        {
            foreach (Media[] medias in _soundFiles.Values)
            {
                foreach (Media media in medias)
                    media.Dispose();
            }
            _mediaPlayer?.Dispose();
            _libVLS.Dispose();
        }
    }
}

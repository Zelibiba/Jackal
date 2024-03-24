using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
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
        Ok,
        Hit,
        Kill,
        Bottles,
        Rum,
        FridayMissioner,
        DrunkMissioner,
        Missioner,
        Ben,
        Friday,
        Airplane,
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
        readonly Dictionary<Sounds, Media> _soundFiles;

        // <summary>
        /// Класс Аудиоплеера.
        /// </summary>
        public AudioPlayer()
        {
            _libVLS = new("--reset-plugins-cache");
            _soundFiles = new Dictionary<Sounds, Media>();
            foreach (Sounds sound in Enum.GetValues(typeof(Sounds)))
                _soundFiles[sound] = new Media(_libVLS, new Uri(Path.Combine(Properties.SoundsFolder, sound.ToString() + ".mp3")));
            _mediaPlayer = new(_libVLS);
        }

        /// <summary>
        /// Метод запускает проигрышь аудиофайла.
        /// </summary>
        /// <param name="sound"></param>
        public void Play(Sounds sound)
        {
            _mediaPlayer = new(_soundFiles[sound]);
            _mediaPlayer.Play();
        }

        public void Dispose()
        {
            foreach (Media media in _soundFiles.Values)
                media.Dispose();
            _mediaPlayer?.Dispose();
            _libVLS.Dispose();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

namespace WindowsGame2
{
    public class SoundManager : GameComponent
    {
        #region Fields and Properties

        private ContentManager _content;

        private Dictionary<string, Song> Songs { get; set; }
        private Dictionary<string, SoundEffect> Sounds { get; set; }

        private Dictionary<SoundEffectInstance, string> PlayingSounds { get; set; }

        public string CurrentSong { get; private set; }

        public static readonly string MenuSelection = "MenuSelection";
        public static readonly string CarCrash = "CarCrash";
        public static readonly string CarSteering = "CarSteering";

        public static readonly string MenuSong = "MenuSong";
        public static readonly string GameSong = "GameSong";

        private Dictionary<string, Queue<SoundEffectInstance>> effectsPool;
        private Dictionary<string, Queue<SoundEffectInstance>> loopedEffectsPool;

        #endregion

        #region Initialization

        public SoundManager(Game game) : base(game)
        {
            _content = game.Content;
            Songs = new Dictionary<string, Song>();
            Sounds = new Dictionary<string, SoundEffect>();

            PlayingSounds = new Dictionary<SoundEffectInstance, string>();

            effectsPool = new Dictionary<string, Queue<SoundEffectInstance>>();
            effectsPool[SoundManager.MenuSelection] = new Queue<SoundEffectInstance>();
            effectsPool[SoundManager.CarCrash] = new Queue<SoundEffectInstance>();
            effectsPool[SoundManager.CarSteering] = new Queue<SoundEffectInstance>();

            loopedEffectsPool = new Dictionary<string, Queue<SoundEffectInstance>>();
        }

        public void Initalize(ContentManager Content)
        {
            LoadSound(SoundManager.CarCrash, "Sounds/crash");
            LoadSound(SoundManager.MenuSelection, "Sounds/menu_selection");
            LoadSound(SoundManager.CarSteering, "Sounds/CarSteering");

            for (int i = 0; i < 25; i++)
            {
                effectsPool[SoundManager.CarCrash].Enqueue(Sounds[SoundManager.CarCrash].CreateInstance());
                effectsPool[SoundManager.MenuSelection].Enqueue(Sounds[SoundManager.MenuSelection].CreateInstance());

            }

            SoundEffectInstance sound;
            for (int i = 0; i < 4; i++)
            {
                sound = Sounds[SoundManager.CarSteering].CreateInstance();
                sound.IsLooped = true;
                effectsPool[SoundManager.CarSteering].Enqueue(sound);
            }

            LoadSong(SoundManager.MenuSong, "Sounds/MenuSong");
            LoadSong(SoundManager.GameSong, "Sounds/GameSong");
        }

        #endregion

        #region Managing Songs

        public void LoadSong(string songName, string songPath)
        {
            Songs[songName] = _content.Load<Song>(songPath);
        }


        public void PlaySong(string songName, bool repeat)
        {
            // If the song is already playing, do nothing.
            if (CurrentSong == songName) return;

            // If another song is playing, stop it.
            StopSong();

            CurrentSong = songName;
            MediaPlayer.IsRepeating = repeat;
            MediaPlayer.Play(Songs[CurrentSong]);
        }

        public void PauseSong()
        {
            if (CurrentSong == null) return;
            MediaPlayer.Pause();
        }

        public void ResumeSong()
        {
            if (CurrentSong == null) return;

            MediaPlayer.Resume();
        }

        public void StopSong()
        {
            if (CurrentSong == null) return;

            CurrentSong = null;
            MediaPlayer.Stop();
        }

        #endregion

        #region Managing Sounds

        public void LoadSound(string soundName, string soundPath)
        {
            Sounds[soundName] = _content.Load<SoundEffect>(soundPath);
        }

        public void PlaySound(string soundName)
        {
            PlaySound(soundName, 1f, 0f, 0f, false);
        }

        public void PlaySound(string soundName, float volume, float pitch, float pan, bool repeat)
        {
            SoundEffectInstance sound = GetSound(soundName, volume, pitch, pan, repeat);
            PlayingSounds[sound] = soundName;
            sound.Play();
        }

        public SoundEffectInstance GetSound(string soundName)
        {
            return GetSound(soundName, 1f, 0f, 0f, true);
        }

        public SoundEffectInstance GetSound(string soundName, float volume, float pitch, float pan, bool repeat)
        {
            SoundEffectInstance sound;
            if (effectsPool[soundName].Count > 0)
            {
                sound = effectsPool[soundName].Dequeue();
            }
            else
            {
                sound = Sounds[soundName].CreateInstance();

            }

            sound.Volume = volume;
            sound.Pitch = pitch;
            sound.Pan = pan;
            // TODO: create pool for looped effects
            //sound.IsLooped = repeat;

            return sound;
        }

        public void PoolSound(SoundEffectInstance sound, string soundName)
        {
            sound.Stop();
            effectsPool[soundName].Enqueue(sound);
        }

        public void StopAllSounds()
        {
            for (int i = 0; i < PlayingSounds.Count; i++)
            {
                SoundEffectInstance key = PlayingSounds.ElementAt(i).Key;
                string value = PlayingSounds.ElementAt(i).Value;

                key.Pause();
                effectsPool[value].Enqueue(key);
                PlayingSounds.Remove(key);
                i--;
            }
        }

        #endregion

        #region Update

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            for (int i = 0; i < PlayingSounds.Count; i++)
            {
                if (PlayingSounds.ElementAt(i).Key.State == SoundState.Stopped)
                {
                    SoundEffectInstance key = PlayingSounds.ElementAt(i).Key;
                    string value = PlayingSounds.ElementAt(i).Value;

                    effectsPool[value].Enqueue(key);
                    PlayingSounds.Remove(key);

                    i--;
                }
            }
        }

        #endregion
    }
}

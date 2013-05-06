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
        private static readonly string[] CarCrashes = { "CarCrash1", "CarCrash2", "CarCrash3", "CarCrash4" };

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
            LoadSound(SoundManager.CarCrashes[0], "Sounds/CarCrash1");
            LoadSound(SoundManager.CarCrashes[1], "Sounds/CarCrash2");
            LoadSound(SoundManager.CarCrashes[2], "Sounds/CarCrash3");
            LoadSound(SoundManager.CarCrashes[3], "Sounds/CarCrash4");

            LoadSound(SoundManager.MenuSelection, "Sounds/menu_selection");
            LoadSound(SoundManager.CarSteering, "Sounds/CarSteering");
            
            SoundEffectInstance sound;

            for (int i = 0; i < 25; i++)
            {
                sound = Sounds[SoundManager.MenuSelection].CreateInstance();
                sound.Volume = 0.5f;
                effectsPool[SoundManager.MenuSelection].Enqueue(sound);
            }

            
            for (int i = 0; i < 4; i++)
            {
                sound = Sounds[SoundManager.CarSteering].CreateInstance();
                sound.Volume = 0.2f;
                sound.IsLooped = true;
                effectsPool[SoundManager.CarSteering].Enqueue(sound);

                for (int j = 0; j < 6; j++)
                {
                    sound = Sounds[SoundManager.CarCrashes[i]].CreateInstance();
                    sound.Volume = 1f;
                    effectsPool[SoundManager.CarCrash].Enqueue(sound);
                }
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
            MediaPlayer.Volume = 1f;
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
                sound.Volume = volume;
                sound.Pitch = pitch;
                sound.Pan = pan;
                sound.IsLooped = repeat;
            }
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

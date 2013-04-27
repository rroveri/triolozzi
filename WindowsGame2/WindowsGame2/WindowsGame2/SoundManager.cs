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
    class SoundManager : GameComponent
    {
        #region Fields and Properties

        private ContentManager _content;

        private Dictionary<string, Song> Songs { get; set; }
        private Dictionary<string, SoundEffect> Sounds { get; set; }

        private List<SoundEffectInstance> PlayingSounds { get; set; }

        public string CurrentSong { get; private set; }

        #endregion

        #region Initialization

        public SoundManager(Game game) : base(game)
        {
            _content = game.Content;
            Songs = new Dictionary<string, Song>();
            Sounds = new Dictionary<string, SoundEffect>();

            PlayingSounds = new List<SoundEffectInstance>();
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
            SoundEffectInstance sound = Sounds[soundName].CreateInstance();

            sound.Volume = volume;
            sound.Pitch = pitch;
            sound.Pan = pan;
            sound.IsLooped = repeat;

            sound.Play();
            PlayingSounds.Add(sound);
        }

        public void StopAllSounds()
        {
            for (int i = 0; i < PlayingSounds.Count; i++)
            {
                PlayingSounds[i].Pause();
                PlayingSounds[i].Dispose();
                PlayingSounds.RemoveAt(i);
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
                if (PlayingSounds[i].State == SoundState.Stopped)
                {
                    PlayingSounds[i].Dispose();
                    PlayingSounds.RemoveAt(i);
                    i--;
                }
            }
        }

        #endregion
    }
}

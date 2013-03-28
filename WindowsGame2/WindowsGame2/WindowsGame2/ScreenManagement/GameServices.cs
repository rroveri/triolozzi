using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace WindowsGame2
{
    public static class GameServices
    {
        private static GameServiceContainer container;

        /// <summary>
        /// Return the singleton instance of the GameServiceContainer.
        /// </summary>
        public static GameServiceContainer Instance
        {
            get
            {
                if (container == null)
                {
                    container = new GameServiceContainer();
                }
                return container;
            }
        }

        public static T GetService<T>()
        {
            return (T)Instance.GetService(typeof(T));
        }

        public static void AddService<T>(T Service)
        {
            Instance.AddService(typeof(T), Service);
        }

        public static void DeleteService<T>()
        {
            Instance.RemoveService(typeof(T));
        }

    }
}

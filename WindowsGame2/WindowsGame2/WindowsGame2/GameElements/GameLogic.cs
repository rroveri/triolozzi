using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace WindowsGame2.GameElements
{
    public class GameLogic
    {
        #region Fields

        /// <summary>
        /// The maximum number of players.
        /// </summary>
        private const int kMaximumPlayers = 4;
        private int nPlayers;

        /// <summary>
        /// The number of laps needed to finish the game.
        /// </summary>
        private const int kMaximumLaps = 5;

        /// <summary>
        /// The crucial points in a track.
        /// </summary>
        private int[] _crucialPoints;

        private int _pointsCount;

        private int[] taken;

        /// <summary>
        /// The number of cars that have been eleminated during a short race.
        /// </summary>
        private int _eliminatedCars;

        /// <summary>
        /// A list that tells whether a crucial point has been reached or not during one lap.
        /// The number of crucial points is usually low (3 points: 1/4th, 1/2th, 3/4th of the race).
        /// </summary>
        private Dictionary<int, bool> didReachCrucialPoint;

        /// <summary>
        /// The current number of laps.
        /// </summary>
        public int Laps { get; private set; }

        /// <summary>
        /// The ranking position for each car in the race.
        /// </summary>
        public int[] Ranking { get; private set; }

        #endregion

        #region Initialization

        public GameLogic(int[] crucialPoints, int pointsCount, int nPlayers)
        {
            Laps = 0;

            // Set up the reference points in the track
            _crucialPoints = crucialPoints;
            _pointsCount = pointsCount;
            didReachCrucialPoint = new Dictionary<int, bool>(4);
            ResetCrucialPoints();

            // Set up the ranking and elimination lists
            taken = new int[kMaximumPlayers];
            _eliminatedCars = 0;

            Ranking = new int[kMaximumPlayers];
            isMiniRaceOver = false;

            this.nPlayers = nPlayers;
        }

        public void RestartMiniRace()
        {
            isMiniRaceOver = false;
        }

        #endregion

        #region Update

        public void Update(List<Car> Cars, Matrix Transform, GraphicsDeviceManager GraphicsDevice)
        {
            isMiniRaceOver = false;
            // Update rankings
            UpdateRankings(Cars);

            // Deactivate cars that went off-screen
            UpdateEliminateCars(Cars, Transform, GraphicsDevice);

            // Update track advancement. Use the first car only to keep track of advancement.
            UpdateTrackAdvancement(Cars);

            // Check if only one player remained (=> winner)
            UpdateRemainingCars(Cars);

            LastCarIndex = Ranking[Cars.Count - 1 - _eliminatedCars];
        }

        #endregion

        #region Queries

        public bool isMiniRaceOver { get; private set; }

        public bool isGameOver()
        {
            return (Laps >= kMaximumLaps);
        }

        /// <summary>
        /// The index of the last car that is still racing (not eliminated yet).
        /// </summary>
        public int LastCarIndex { get; private set; }

        #endregion

        #region Update Game Logic

        private void UpdateRankings(List<Car> Cars)
        {
            for (int i = 0; i < Cars.Count; i++)
            {
                taken[i] = 0;
            }
            for (int i = 0; i < Cars.Count; i++)
            {
                int newMax = -1;
                int newIndex = 0;
                for (int ii = 0; ii < Cars.Count; ii++)
                {
                    if (Cars[ii].isActive && Cars[ii].currentMiddlePoint > newMax && taken[ii] == 0)
                    {
                        newMax = Cars[ii].currentMiddlePoint;
                        newIndex = ii;
                    }
                }
                taken[newIndex] = 1;
                Ranking[i] = newIndex;
            }
        }

        private void UpdateEliminateCars(List<Car> Cars, Matrix Transform, GraphicsDeviceManager GraphicsDevice)
        {
            for (int i = 0; i < Cars.Count; i++)
            {
                if (Cars[i].isActive)
                {
                    // Compute screen coordinates
                    Vector2 screenPosition = Vector2.Transform(Cars[i].Position, Transform);

                    // Check if car is off screen
                    if (screenPosition.X < 0 || screenPosition.X > GraphicsDevice.PreferredBackBufferWidth || screenPosition.Y < 0 || screenPosition.Y > GraphicsDevice.PreferredBackBufferHeight)
                    {
                        // Check if car is in the last position
                        if (Ranking[Cars.Count - 1 - _eliminatedCars] == i)
                        {
                            // Deactivate car and add index to the array with the order of elimination
                            Cars[i].isActive = false;

                            // Immediately update the score of the eliminated car
                            UpdateScore(Cars[i], Cars.Count - 1 - _eliminatedCars);

                            _eliminatedCars++;
                            break;
                        }
                    }
                }
            }
        }

        private void UpdateTrackAdvancement(List<Car> Cars)
        {
            int point = 0;
            for (int i = 0; i < Cars.Count; i++)
            {
                point = Cars[i].currentMiddlePoint;
                if (didReachCrucialPoint.ContainsKey(point))
                {
                    didReachCrucialPoint[point] = true;
                }
            }

            // Transform the point for 1-Lap computation
            point = point % (_pointsCount + 40);

            if (point == 0 && !didReachCrucialPoint.ContainsValue(false))
            {
                Laps++;
                ResetCrucialPoints();
            }
        }

        private void UpdateRemainingCars(List<Car> Cars)
        {
            // Is the first player the only one left in the game?
            if (_eliminatedCars == Cars.Count - 1)
            {
                isMiniRaceOver = true;
                _eliminatedCars = 0;
                UpdateScore(Cars[Ranking[0]], 0);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="car"></param>
        /// <param name="position">Must be between [0, kMaximumPlayers-1]</param>
        public void UpdateScore(Car car, int position)
        {
            int scoreMultiplier = 3;
            int newScore = (position == 0) ? scoreMultiplier*nPlayers : -scoreMultiplier * position;

            newScore += car.score;
            car.score = Math.Min(Math.Max(1, newScore), 54); // Score must be between [1,54]
        }

        private void ResetCrucialPoints()
        {
            for (int i = 0; i < _crucialPoints.Length; i++)
            {
                didReachCrucialPoint[_crucialPoints[i]] = false;
            }
        }

        #endregion
    }
}

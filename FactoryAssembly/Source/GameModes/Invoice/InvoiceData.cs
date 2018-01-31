using System;
using System.Collections.Generic;
using System.Linq;

namespace FactoryAssembly
{
    internal static class InvoiceData
    {
        internal static bool Enabled
        {
            get;
            set;
        }

        internal static string MissionName
        {
            get;
            set;
        }

        internal static FactoryGameModePicker.GameMode GameMode
        {
            get;
            set;
        }

        internal static int BombCount
        {
            get
            {
                return StartedBombs.Count();
            }
        }

        internal static TimeSpan TotalRealDuration
        {
            get
            {
                return StartedBombs.Max((x) => x.RealWorldEndTime) - StartedBombs.Min((x) => x.RealWorldStartTime);
            }
        }

        internal static TimeSpan TotalRealActiveDuration
        {
            get
            {
                return TimeSpan.FromSeconds(StartedBombs.Sum((x) => (x.RealWorldActiveTime).TotalSeconds));
            }
        }

        internal static TimeSpan TotalRealIdleDuration
        {
            get
            {
                return TotalRealDuration - TotalRealActiveDuration;
            }
        }

        internal static TimeSpan TotalBombRemainingTime
        {
            get
            {
                return TimeSpan.FromSeconds(StartedBombs.Sum((x) => x.EndRemainingTime));
            }
        }

        internal static TimeSpan TotalActiveDuration
        {
            get
            {
                return TimeSpan.FromSeconds(StartedBombs.Sum((x) => x.ActiveBombTime));
            }
        }

        internal static int TotalStrikes
        {
            get
            {
                return StartedBombs.Sum((x) => x.EndStrikesCount - x.StartStrikesCount);
            }
        }

        internal static int TotalSolvableModuleCount
        {
            get
            {
                return StartedBombs.Sum((x) => x.SolvableModuleCount);
            }
        }

        internal static int TotalModuleCount
        {
            get
            {
                return StartedBombs.Sum((x) => x.TotalModuleCount);
            }
        }

        internal static int TotalSolvedModuleCount
        {
            get
            {
                return StartedBombs.Sum((x) => x.SolvedModuleCount);
            }
        }

        internal static TimeSpan InitialTime
        {
            get
            {
                return TimeSpan.FromSeconds(StartedBombs.First().StartRemainingTime);
            }
        }

        internal static int InitialStrikesToLose
        {
            get
            {
                return StartedBombs.First().StrikesToLose;
            }
        }

        internal static int InitialModuleCount
        {
            get
            {
                return StartedBombs.First().TotalModuleCount;
            }
        }

        internal static TimeSpan FinalTime
        {
            get
            {
                return TimeSpan.FromSeconds(StartedBombs.Last().EndRemainingTime);
            }
        }

        private static IEnumerable<BombData> StartedBombs
        {
            get
            {
                return _bombProgress.Where((x) => x.Started);
            }
        }

        private static List<BombData> _bombProgress = new List<BombData>();
        private static Dictionary<int, BombData> _bombProgressLookup = new Dictionary<int, BombData>();

        internal static void ClearData()
        {
            MissionName = null;

            _bombProgress.Clear();
            _bombProgressLookup.Clear();
        }

        internal static BombData GetBombData(int bombIndex)
        {
            if (Enabled)
            {
                return _bombProgress[bombIndex];
            }

            return null;
        }

        internal static BombData GetBombDataForBomb(int bombID)
        {
            if (!Enabled)
            {
                return null;
            }

            BombData bombData = null;
            if (_bombProgressLookup.TryGetValue(bombID, out bombData))
            {
                return bombData;
            }

            bombData = new BombData();
            _bombProgress.Add(bombData);
            _bombProgressLookup[bombID] = bombData;

            return bombData;
        }
    }
}

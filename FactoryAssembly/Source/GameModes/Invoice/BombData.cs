using System;

namespace FactoryAssembly
{
    internal class BombData
    {
        public float StartRemainingTime
        {
            get;
            set;
        }

        public float EndRemainingTime
        {
            get;
            set;
        }

        public float ActiveBombTime
        {
            get
            {
                return StartRemainingTime - EndRemainingTime;
            }
        }

        public DateTime RealWorldStartTime
        {
            get;
            set;
        }

        public DateTime RealWorldEndTime
        {
            get;
            set;
        }

        public TimeSpan RealWorldActiveTime
        {
            get
            {
                return RealWorldEndTime - RealWorldStartTime;
            }
        }

        public int StartStrikesCount
        {
            get;
            set;
        }

        public int StrikesToLose
        {
            get;
            set;
        }

        public int EndStrikesCount
        {
            get;
            set;
        }

        public int StrikeCount
        {
            get
            {
                return EndStrikesCount - StartStrikesCount;
            }
        }

        public int SolvableModuleCount
        {
            get;
            set;
        }

        public int NeedyModuleCount
        {
            get;
            set;
        }

        public int TotalModuleCount
        {
            get
            {
                return SolvableModuleCount + NeedyModuleCount;
            }
        }

        public int SolvedModuleCount
        {
            get;
            set;
        }

        public bool Started
        {
            get;
            set;
        }

        public bool Complete
        {
            get;
            set;
        }
    }
}

using System;
using System.Collections.Generic;

namespace FactoryAssembly
{
    internal abstract class FactoryGameMode
    {
        internal abstract float RemainingTime
        {
            get;
        }

        protected FactoryRoom Room
        {
            get;
            private set;
        }

        protected IEnumerable<FactoryBomb> Bombs
        {
            get
            {
                return _bombs;
            }
        }

        protected IEnumerable<FactoryGameModeAdaptation> Adaptations
        {
            get
            {
                return _adaptations;
            }
        }

        private List<FactoryBomb> _bombs = new List<FactoryBomb>();
        private List<FactoryGameModeAdaptation> _adaptations = new List<FactoryGameModeAdaptation>();

        internal virtual void Setup(FactoryRoom room)
        {
            Room = room;

            List<Bomb> bombs = SceneManager.Instance.GameplayState.Bombs;
            foreach (Bomb bomb in bombs)
            {
                _bombs.Add(bomb.gameObject.AddComponent<FactoryBomb>());
            }
        }

        internal void AddAdaptation(Type adpatationType)
        {
            _adaptations.Add(Activator.CreateInstance(adpatationType) as FactoryGameModeAdaptation);
        }

        internal virtual void Update()
        {
        }

        /// <summary>
        /// Starts the 'current' bomb.
        /// </summary>
        /// <remarks>Invoked by animation event.</remarks>
        internal virtual void OnStartBomb()
        {
        }

        /// <summary>
        /// Ends the 'old' bomb.
        /// </summary>
        /// <remarks>Invoked by animation event.</remarks>
        internal virtual void OnEndBomb()
        {
        }

        protected FactoryBomb AddAnotherBomb(int bombIndex)
        {
            Bomb newBomb = Room.CreateBombWithCurrentMission(bombIndex);
            FactoryBomb newFactoryBomb = newBomb.gameObject.AddComponent<FactoryBomb>();
            _bombs.Add(newFactoryBomb);

            return newFactoryBomb;
        }

        protected void DestroyBomb(FactoryBomb bomb)
        {
            _bombs.Remove(bomb);
            Room.DestroyBomb(bomb.InternalBomb);
        }
    }
}

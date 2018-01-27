using System;
using System.Collections.Generic;
using UnityEngine;

namespace FactoryAssembly
{
    public abstract class FactoryGameMode : MonoBehaviour
    {
        public abstract float RemainingTime
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

        public virtual void Setup(FactoryRoom room)
        {
            Room = room;

            List<Bomb> bombs = SceneManager.Instance.GameplayState.Bombs;
            foreach (Bomb bomb in bombs)
            {
                _bombs.Add(bomb.gameObject.AddComponent<FactoryBomb>());
            }
        }

        public void AddAdaptation(Type adpatationType)
        {
            _adaptations.Add(Activator.CreateInstance(adpatationType) as FactoryGameModeAdaptation);
        }

        /// <summary>
        /// Starts the 'current' bomb.
        /// </summary>
        /// <remarks>Invoked by animation event.</remarks>
        public virtual void OnStartBomb()
        {
        }

        /// <summary>
        /// Ends the 'old' bomb.
        /// </summary>
        /// <remarks>Invoked by animation event.</remarks>
        public virtual void OnEndBomb()
        {
        }

        protected FactoryBomb AddAnotherBomb()
        {
            Bomb newBomb = Room.CreateBombWithCurrentMission();
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

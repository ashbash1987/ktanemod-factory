using Assets.Scripts.Missions;
using System.Collections.Generic;
using UnityEngine;

namespace FactoryAssembly
{
    public abstract class FactoryGameMode : MonoBehaviour
    {
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

        private List<FactoryBomb> _bombs = new List<FactoryBomb>();

        public virtual void Setup(FactoryRoom room)
        {
            Room = room;

            List<Bomb> bombs = SceneManager.Instance.GameplayState.Bombs;
            foreach (Bomb bomb in bombs)
            {
                _bombs.Add(bomb.gameObject.AddComponent<FactoryBomb>());
            }
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
    }
}

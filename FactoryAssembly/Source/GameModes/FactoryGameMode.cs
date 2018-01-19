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
            get;
            private set;
        }

        public virtual void Setup(FactoryRoom room)
        {
            Room = room;

            List<Bomb> bombs = SceneManager.Instance.GameplayState.Bombs;
            List<FactoryBomb> factoryBombs = new List<FactoryBomb>();
            foreach (Bomb bomb in bombs)
            {
                factoryBombs.Add(bomb.gameObject.AddComponent<FactoryBomb>());
            }

            Bombs = factoryBombs;
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
    }
}

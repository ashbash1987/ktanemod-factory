using System.Collections;

namespace FactoryAssembly
{
    internal class InfiniteSequenceMode : FiniteSequenceMode
    {
        /// <summary>
        /// Requests the next bomb to show up.
        /// </summary>
        protected override void GetNextBomb()
        {
            //Need to pre-emptively add a bomb before the "last" bomb each time
            if (_bombQueue.Count == 1)
            {
                FactoryBomb nextBomb = AddAnotherBomb();
                nextBomb.SetupStartPosition(Room.InitialSpawn);
                _bombQueue.Enqueue(nextBomb);
            }

            base.GetNextBomb();

            Room.StartCoroutine(DelayDestroyBomb(_oldBomb));
        }

        private IEnumerator DelayDestroyBomb(FactoryBomb bomb)
        {
            if (bomb == null || bomb.InternalBomb == null)
            {
                yield break;
            }

            while (!bomb.Ended)
            {
                yield return null;
            }

            DestroyBomb(bomb);
        }
    }
}

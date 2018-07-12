using System.Collections;
using UnityEngine;

namespace FactoryAssembly
{
    internal class InfiniteSequenceMode : FiniteSequenceMode
    {
        /// <summary>
        /// Requests the next bomb to show up.
        /// </summary>
        protected override void GetNextBomb()
        {
            if (_bombQueue.Count == 1)
            {
                Room.StartCoroutine(DelayCreateBomb());
            }

            base.GetNextBomb();

            Room.StartCoroutine(DelayDestroyBomb(_oldBomb));
        }

        private IEnumerator DelayCreateBomb()
        {
            yield return new WaitForSeconds(0.2f);

            FactoryBomb nextBomb = AddAnotherBomb(InvoiceData.BombCount);
            nextBomb.SetupStartPosition(Room.InitialSpawn);
            _bombQueue.Enqueue(nextBomb);
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

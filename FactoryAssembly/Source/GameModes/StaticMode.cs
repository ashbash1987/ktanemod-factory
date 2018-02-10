using System.Linq;
using UnityEngine;

namespace FactoryAssembly
{
    internal class StaticMode : FactoryGameMode
    {
        internal override float RemainingTime
        {
            get
            {
                if (Bombs.Count() == 0)
                {
                    return float.PositiveInfinity;
                }

                return Bombs.Min((x) => x.Timer.TimeRemaining);
            }
        }

        private const float SPAWN_LENGTH = 3.0f;
        private const float MAXIMUM_BOMB_SPACING = 0.6f;

        internal override void Setup(FactoryRoom room)
        {
            base.Setup(room);

            InvoiceData.Enabled = false;

            FactoryBomb[] bombs = Bombs.ToArray();

            Vector3 vanillaBombSpawnPosition = Room.VanillaBombSpawn.position;

            float bombSpacing = Mathf.Min(SPAWN_LENGTH / (bombs.Length - 1.0f), MAXIMUM_BOMB_SPACING);

            vanillaBombSpawnPosition.x -= (bombs.Length - 1) * bombSpacing * 0.5f;

            for (int bombIndex = 0; bombIndex < bombs.Length; ++bombIndex)
            {
                bombs[bombIndex].transform.position = vanillaBombSpawnPosition;
                bombs[bombIndex].SetupHoldableOrigin(vanillaBombSpawnPosition);
                bombs[bombIndex].EnableBomb();

                vanillaBombSpawnPosition.x += bombSpacing;
            }
        }              
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FactoryAssembly
{
    public class FactoryRoom : MonoBehaviour
    {
        /// <remarks>
        /// Due to issues using custom assembly in Unity not exposing fields, have to discover required transforms by name instead. Not the greatest option, but it works.
        /// </remarks>
        private static readonly string[] CONVEYOR_BELT_NODE_NAMES = { "ConveyorBeltNodeA", "ConveyorBeltNodeB" };

        private Animator _conveyorBeltAnimator = null;
        private KMGameplayRoom _room = null;

        private Queue<FactoryBomb> _bombs = new Queue<FactoryBomb>();
        private FactoryBomb _currentBomb = null;
        private FactoryBomb _oldBomb = null;

        private Transform[] _conveyorBeltNodes = null;
        private int _nextBeltNodeIndex = 0;

        private bool _startedRoom = false;

        /// <summary>
        /// Unity event.
        /// </summary>
        private void Awake()
        {
            _conveyorBeltAnimator = GetComponent<Animator>();
            _room = GetComponent<KMGameplayRoom>();

            _room.OnLightChange += OnLightChange;
        }

        /// <summary>
        /// Unity event.
        /// </summary>
        private void Start()
        {
            //Get the conveyor belt nodes
            _conveyorBeltNodes = new Transform[CONVEYOR_BELT_NODE_NAMES.Length];
            for (int nodeIndex = 0; nodeIndex < CONVEYOR_BELT_NODE_NAMES.Length; ++nodeIndex)
            {
                _conveyorBeltNodes[nodeIndex] = transform.Find(CONVEYOR_BELT_NODE_NAMES[nodeIndex]);
            }

            StartCoroutine(FindBombs());
        }

        private IEnumerator FindBombs()
        {
            yield return null;
            yield return null;

            Bomb[] bombs = FindObjectsOfType<Bomb>();

            foreach (Bomb bomb in bombs)
            {
                FactoryBomb factoryBomb = bomb.gameObject.AddComponent<FactoryBomb>();
                factoryBomb.MoveOffscreen(_conveyorBeltNodes[0]);
                _bombs.Enqueue(factoryBomb);
            }
        }

        /// <summary>
        /// Unity event.
        /// </summary>
        private void Update()
        {
            if (_currentBomb != null && _currentBomb.IsReadyToShip)
            {
                GetNextBomb();
            }
        }

        private void OnLightChange(bool on)
        {
            if (on && !_startedRoom)
            {
                _startedRoom = true;
                GetNextBomb();
            }
        }

        /// <summary>
        /// Requests the next bomb to show up.
        /// </summary>
        private void GetNextBomb()
        {
            _oldBomb = _currentBomb;

            if (_bombs.Count != 0)
            {
                _currentBomb = _bombs.Dequeue();
                _currentBomb.AttachToConveyor(_conveyorBeltNodes[_nextBeltNodeIndex]);
                _nextBeltNodeIndex = (_nextBeltNodeIndex + 1) % _conveyorBeltNodes.Length;
            }
            else
            {
                _currentBomb = null;
            }

            _conveyorBeltAnimator.SetTrigger("NextBomb");
        }

        /// <summary>
        /// Starts the 'current' bomb.
        /// </summary>
        /// <remarks>Invoked by animation event.</remarks>
        private void StartBomb()
        {
            if (_currentBomb != null)
            {
                _currentBomb.StartBomb();
            }
        }

        /// <summary>
        /// Ends the 'old' bomb.
        /// </summary>
        /// <remarks>Invoked by animation event.</remarks>
        private void EndBomb()
        {
            if (_oldBomb != null)
            {
                _oldBomb.EndBomb();
                _oldBomb = null;
            }
        }                
    }
}

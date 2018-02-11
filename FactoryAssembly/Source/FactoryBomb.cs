using Events;
using System;
using System.Collections;
using UnityEngine;

namespace FactoryAssembly
{
    internal class FactoryBomb : MonoBehaviour
    {
        internal bool IsReadyToShip
        {
            get
            {
                return InternalBomb.IsSolved() && _holdable.HoldState == FloatingHoldable.HoldStateEnum.Idle;
            }
        }

        internal Selectable Selectable
        {
            get;
            private set;
        }

        internal Bomb InternalBomb
        {
            get;
            private set;
        }

        internal TimerComponent Timer
        {
            get;
            private set;
        }

        internal bool Ended
        {
            get;
            private set;
        }

        private const int NORMAL_SELECTABLE_COLLIDER_LAYER_INDEX = 11;
        private const int DISABLED_SELECTABLE_COLLIDER_LAYER_INDEX = 12;

        private static readonly Vector3 OFFSCREEN_POSITION = new Vector3(0.0f, -1000.0f, 0.0f);

        private FloatingHoldable _holdable = null;
        private SelectableArea _selectableArea = null;
        private Vector3 _targetStartPosition = Vector3.zero;
        private bool _timerStarted = false;

        #region Unity Lifecycle
        /// <summary>
        /// Unity event.
        /// </summary>
        private void Awake()
        {
            Ended = false;

            InternalBomb = GetComponent<Bomb>();
            Timer = InternalBomb.GetTimer();
            _holdable = GetComponentInChildren<FloatingHoldable>();
            _selectableArea = GetComponentInChildren<SelectableArea>();
            Selectable = GetComponent<Selectable>();

            ChangeTimerVisibility(false);
            SetSelectableLayer(false);

            BombEvents.OnBombSolved += OnAnyBombSolved;
            BombEvents.OnBombDetonated += OnAnyBombDetonated;
        }

        private void Update()
        {
            if (!_timerStarted && Timer.IsUpdating)
            {
                OnBombTimerStart();
                _timerStarted = true;
            }
        }

        private void OnDestroy()
        {
            BombEvents.OnBombSolved -= OnAnyBombSolved;
            BombEvents.OnBombDetonated -= OnAnyBombDetonated;
        }
        #endregion

        #region Public (Internal) Methods
        /// <summary>
        /// Sets up/overrides the holdable's origin position when the bomb is placed down.
        /// </summary>
        internal void SetupHoldableOrigin(Vector3 position)
        {
            _holdable.OrigPosition = position;
        }

        /// <summary>
        /// Sets up/overrides the holdable's origin orientation when the bomb is placed down.
        /// </summary>
        internal void SetupHoldableOrientation(Quaternion rotation)
        {
            _holdable.OrigRotation = rotation;
        }

        /// <summary>
        /// Sets up the initial starting position of the bomb when it will initally enter the room.
        /// </summary>
        internal void SetupStartPosition(Transform conveyorBeltNode)
        {
            _targetStartPosition = conveyorBeltNode.transform.position;
            _targetStartPosition.y = transform.position.y;

            SetupHoldableOrientation(Quaternion.Euler(0.0f, UnityEngine.Random.Range(-90.0f, 90.0f), 0.0f));

            //Can't deactivate the bomb temporarily, as it causes coroutines to stop which are necessary for various initialization sequences of modules, widgets, etc!
            //So instead of SetActive(false), move it to somewhere offscreen a long way away, and *hope* for the best!
            transform.position = OFFSCREEN_POSITION;
        }

        /// <summary>
        /// Attaches this bomb to the conveyor belt at the given node.
        /// </summary>
        /// <remarks>
        /// Ensure that the bomb has had SetupStartPosition() called beforehand to setup the initial position.
        /// </remarks>
        internal void AttachToConveyor(Transform conveyorBeltNode)
        {
            transform.position = _targetStartPosition;
            transform.rotation = _holdable.OrigRotation;

            transform.SetParent(conveyorBeltNode, true);

            ChangeTimerVisibility(false);
        }

        internal void BombConfigured()
        {
            OnBombConfigured();
        }

        /// <summary>
        /// Manually starts the bomb, if the bomb's normal behavior is overridden.
        /// </summary>
        internal void StartBomb()
        {
            StartCoroutine(StartBombCoroutine());
        }

        /// <summary>
        /// Manually enables the bomb.
        /// </summary>
        internal void EnableBomb()
        {
            SetSelectableLayer(true);
        }

        /// <summary>
        /// Manually disables the bomb.
        /// </summary>
        internal void DisableBomb()
        {
            SetSelectableLayer(false);
        }

        /// <summary>
        /// Manually 'ends' the bomb, although that is a loose term.
        /// </summary>
        internal void EndBomb()
        {
            //Can't destroy the bomb, as it causes problems when trying to leave the gameplay room after defusal, as it needs the bomb for the time remaining value!
            //So instead of DestroyObject(..), move it to somewhere offscreen a long way away, and *hope* for the best!
            transform.SetParent(null, true);
            transform.position = OFFSCREEN_POSITION;
            Ended = true;
        }
        #endregion

        #region Private Methods
        private IEnumerator StartBombCoroutine()
        {
            ChangeTimerVisibility(true);
            SetSelectableLayer(true);

            ActivateBomb();

            yield return new WaitForSeconds(3.0f);

            StartTimer();
        }

        private void ChangeTimerVisibility(bool on)
        {
            Timer.text.gameObject.SetActive(on);
            Timer.LightGlow.enabled = on;
        }

        private void ActivateBomb()
        {
            InternalBomb.WidgetManager.ActivateAllWidgets();

            if (!InternalBomb.HasDetonated)
            {
                foreach (BombComponent component in InternalBomb.BombComponents)
                {
                    component.Activate();
                }
            }
        }

        private void StartTimer()
        {
            Timer.StartTimer();
        }

        private void SetSelectableLayer(bool enable)
        {
            _selectableArea.gameObject.layer = enable ? NORMAL_SELECTABLE_COLLIDER_LAYER_INDEX : DISABLED_SELECTABLE_COLLIDER_LAYER_INDEX;
        }

        private void OnBombConfigured()
        {
            BombData bombData = InvoiceData.GetBombDataForBomb(GetInstanceID());
            if (bombData == null)
            {
                return;
            }

            bombData.RealWorldStartTime = DateTime.Now;
            bombData.StartRemainingTime = Timer.TimeRemaining;
            bombData.StartStrikesCount = InternalBomb.NumStrikes;
            bombData.StrikesToLose = InternalBomb.NumStrikesToLose;
            bombData.SolvableModuleCount = InternalBomb.GetSolvableComponentCount();
            bombData.Started = true;
        }

        private void OnBombTimerStart()
        {
            BombData bombData = InvoiceData.GetBombDataForBomb(GetInstanceID());
            if (bombData == null)
            {
                return;
            }

            bombData.RealWorldStartTime = DateTime.Now;
        }

        private void OnAnyBombSolved()
        {
            BombData bombData = InvoiceData.GetBombDataForBomb(GetInstanceID());
            if (bombData == null || bombData.Complete || !InternalBomb.IsSolved())
            {
                return;
            }

            bombData.RealWorldEndTime = DateTime.Now;
            bombData.EndRemainingTime = Mathf.Max(Timer.TimeRemaining, 0.0f);
            bombData.EndStrikesCount = InternalBomb.NumStrikes;
            bombData.SolvedModuleCount = InternalBomb.GetSolvedComponentCount();

            bombData.Complete = true;
        }

        private void OnAnyBombDetonated()
        {
            BombData bombData = InvoiceData.GetBombDataForBomb(GetInstanceID());
            if (bombData == null || bombData.Complete)
            {
                return;
            }

            bombData.RealWorldEndTime = DateTime.Now;
            bombData.EndRemainingTime = Mathf.Max(Timer.TimeRemaining, 0.0f);
            bombData.EndStrikesCount = InternalBomb.NumStrikes;
            bombData.SolvedModuleCount = InternalBomb.GetSolvedComponentCount();

            bombData.Complete = true;
        }
        #endregion
    }
}

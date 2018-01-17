using System.Collections;
using UnityEngine;

namespace FactoryAssembly
{
    public class FactoryBomb : MonoBehaviour
    {
        public bool IsReadyToShip
        {
            get
            {
                return _bomb.IsSolved() && _holdable.HoldState == FloatingHoldable.HoldStateEnum.Idle;
            }
        }

        public Selectable Selectable
        {
            get;
            private set;
        }

        private const int NORMAL_SELECTABLE_COLLIDER_LAYER_INDEX = 11;
        private const int DISABLED_SELECTABLE_COLLIDER_LAYER_INDEX = 12;

        private static readonly Vector3 OFFSCREEN_POSITION = new Vector3(0.0f, -1000.0f, 0.0f);

        private Bomb _bomb = null;
        private TimerComponent _timer = null;
        private FloatingHoldable _holdable = null;
        private SelectableArea _selectableArea = null;
        private Vector3 _targetStartPosition = Vector3.zero;

        #region Unity Lifecycle
        /// <summary>
        /// Unity event.
        /// </summary>
        private void Awake()
        {
            _bomb = GetComponent<Bomb>();
            _timer = _bomb.GetTimer();
            _holdable = GetComponentInChildren<FloatingHoldable>();
            _selectableArea = GetComponentInChildren<SelectableArea>();
            Selectable = GetComponent<Selectable>();

            ChangeTimerVisibility(false);
            SetSelectableLayer(false);
        }
        #endregion

        #region Public Methods
        public void SetupStartPosition(Transform conveyorBeltNode)
        {
            _targetStartPosition = conveyorBeltNode.transform.position;
            _targetStartPosition.y = transform.position.y;

            _holdable.OrigRotation = Quaternion.Euler(0.0f, Random.Range(-90.0f, 90.0f), 0.0f);

            //Can't deactivate the bomb temporarily, as it causes coroutines to stop which are necessary for various initialization sequences of modules, widgets, etc!
            //So instead of SetActive(false), move it to somewhere offscreen a long way away, and *hope* for the best!
            transform.position = OFFSCREEN_POSITION;
        }

        public void AttachToConveyor(Transform conveyorBeltNode)
        {
            transform.position = _targetStartPosition;
            transform.rotation = _holdable.OrigRotation;

            transform.SetParent(conveyorBeltNode, true);

            ChangeTimerVisibility(false);
        }

        public void StartBomb()
        {
            StartCoroutine(StartBombCoroutine());
        }

        public void DisableBomb()
        {
            SetSelectableLayer(false);
        }

        public void EndBomb()
        {
            //Can't destroy the bomb, as it causes problems when trying to leave the gameplay room after defusal, as it needs the bomb for the time remaining value!
            //So instead of DestroyObject(..), move it to somewhere offscreen a long way away, and *hope* for the best!
            transform.SetParent(null, true);
            transform.position = OFFSCREEN_POSITION;
        }
        #endregion

        #region Private Methods
        private IEnumerator StartBombCoroutine()
        {
            ChangeTimerVisibility(true);
            SetSelectableLayer(true);

            yield return new WaitForSeconds(1.0f);

            ActivateBomb();

            yield return new WaitForSeconds(2.0f);

            StartTimer();
        }

        private void ChangeTimerVisibility(bool on)
        {
            _timer.text.gameObject.SetActive(on);
            _timer.LightGlow.enabled = on;
        }

        private void ActivateBomb()
        {
            _bomb.WidgetManager.ActivateAllWidgets();

            if (!_bomb.HasDetonated)
            {
                foreach (BombComponent component in _bomb.BombComponents)
                {
                    component.Activate();
                }
            }
        }

        private void StartTimer()
        {
            _timer.StartTimer();
        }

        private void SetSelectableLayer(bool enable)
        {
            _selectableArea.gameObject.layer = enable ? NORMAL_SELECTABLE_COLLIDER_LAYER_INDEX : DISABLED_SELECTABLE_COLLIDER_LAYER_INDEX;
        }
        #endregion
    }
}

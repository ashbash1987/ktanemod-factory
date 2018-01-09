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

        private Bomb _bomb = null;
        private TimerComponent _timer = null;
        private FloatingHoldable _holdable = null;

        private bool _active = false;

        private void Awake()
        {
            _bomb = GetComponent<Bomb>();
            _timer = _bomb.GetTimer();
            _holdable = GetComponentInChildren<FloatingHoldable>();
        }

        private void Update()
        {
            if (!_active && _timer.IsUpdating)
            {
                _timer.StopTimer();
            }
        }

        public void MoveOffscreen(Transform conveyorBeltNode)
        {
            Vector3 targetPosition = conveyorBeltNode.transform.position;
            targetPosition.y = transform.position.y;
            transform.position = targetPosition;

            _holdable.OrigRotation = Quaternion.Euler(0.0f, Random.Range(-90.0f, 90.0f), 0.0f);
            transform.rotation = _holdable.OrigRotation;
        }

        public void AttachToConveyor(Transform conveyorBeltNode)
        {
            transform.SetParent(conveyorBeltNode, true);
        }

        public void StartBomb()
        {
            if (_active)
            {
                return;
            }

            _active = true;

            if (!_timer.IsUpdating)
            {
                _timer.StartTimer();
            }
        }

        public void EndBomb()
        {
            //Can't destroy the bomb, as it causes problems when trying to leave the gameplay room after defusal, as it needs the bomb for the time remaining value!
            //So instead of DestroyObject(..), move it to somewhere offscreen a long way away, and *hope* for the best!
            transform.position = new Vector3(0.0f, -100000.0f, 0.0f);
        }
    }
}

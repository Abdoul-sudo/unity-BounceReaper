using UnityEngine;

namespace BounceReaper
{
    public class AimController : MonoBehaviour
    {
        // 1. SerializeField
        [Header("Config")]
        [SerializeField] private float _minAngle = 10f;
        [SerializeField] private float _maxAngle = 170f;

        [Header("Visual")]
        [SerializeField] private LineRenderer _aimLine;
        [SerializeField] private float _aimLineLength = 3f;

        // 2. Private fields
        private Camera _camera;
        private bool _isAiming;
        private bool _canAim;
        private Vector2 _aimDirection;

        // 3. Properties
        public bool IsAiming => _isAiming;
        public Vector2 AimDirection => _aimDirection;

        // 4. Lifecycle
        private void Awake()
        {
            _camera = Camera.main;
            if (_aimLine != null)
            {
                _aimLine.positionCount = 2;
                _aimLine.enabled = false;
            }
        }

        private void Update()
        {
            if (!_canAim) return;

            // Mouse/touch input
            if (Input.GetMouseButtonDown(0))
            {
                StartAim();
            }
            else if (Input.GetMouseButton(0) && _isAiming)
            {
                UpdateAim();
            }
            else if (Input.GetMouseButtonUp(0) && _isAiming)
            {
                ReleaseAim();
            }
        }

        // 5. Public API
        public void EnableAiming()
        {
            _canAim = true;
        }

        public void DisableAiming()
        {
            _canAim = false;
            _isAiming = false;
            if (_aimLine != null)
                _aimLine.enabled = false;
        }

        // 6. Private methods
        private void StartAim()
        {
            _isAiming = true;
            if (_aimLine != null)
                _aimLine.enabled = true;
        }

        private void UpdateAim()
        {
            if (!BallManager.IsAvailable) return;

            Vector2 launchPos = BallManager.Instance.LaunchPosition;
            Vector2 mouseWorld = _camera.ScreenToWorldPoint(Input.mousePosition);

            _aimDirection = (mouseWorld - launchPos).normalized;

            // Clamp to upward angles only
            float angle = Mathf.Atan2(_aimDirection.y, _aimDirection.x) * Mathf.Rad2Deg;
            angle = Mathf.Clamp(angle, _minAngle, _maxAngle);
            _aimDirection = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            // Update visual line
            if (_aimLine != null)
            {
                Vector3 start = new Vector3(launchPos.x, launchPos.y, 0);
                Vector3 end = start + (Vector3)_aimDirection * _aimLineLength;
                _aimLine.SetPosition(0, start);
                _aimLine.SetPosition(1, end);
            }
        }

        private void ReleaseAim()
        {
            _isAiming = false;
            if (_aimLine != null)
                _aimLine.enabled = false;

            // Only fire if aiming upward
            if (_aimDirection.y > 0.1f && BallManager.IsAvailable)
            {
                BallManager.Instance.FireBalls(_aimDirection);
                DisableAiming();
            }
        }
    }
}

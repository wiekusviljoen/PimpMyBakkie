using UnityEngine;
using UnityEngine.InputSystem;

namespace PimpMyBakkie.Configurator
{
    public sealed class VehicleRotator : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] float rotationSpeed = .18f, zoomSpeed = .012f, minDistance = 6f, maxDistance = 16f;
        Camera cam;
        float distance = 10f;

        void Awake()
        {
            if (target == null) target = transform;
            cam = Camera.main;
        }

        void Update()
        {
            HandleTouchInput();
            HandleMouseInput();
            UpdateCamera();
        }

        void HandleTouchInput()
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen == null) return;
            if (touchscreen.touches.Count >= 1 && touchscreen.touches[0].press.isPressed)
                target.Rotate(Vector3.up, -touchscreen.touches[0].delta.ReadValue().x * rotationSpeed, Space.World);

            if (touchscreen.touches.Count >= 2 && touchscreen.touches[0].press.isPressed && touchscreen.touches[1].press.isPressed)
            {
                var a = touchscreen.touches[0].position.ReadValue();
                var b = touchscreen.touches[1].position.ReadValue();
                var previousA = a - touchscreen.touches[0].delta.ReadValue();
                var previousB = b - touchscreen.touches[1].delta.ReadValue();
                distance -= (Vector2.Distance(a, b) - Vector2.Distance(previousA, previousB)) * zoomSpeed;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
            }
        }

        void HandleMouseInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;
            if (mouse.leftButton.isPressed)
                target.Rotate(Vector3.up, -mouse.delta.ReadValue().x * rotationSpeed, Space.World);
            distance -= mouse.scroll.ReadValue().y * .01f;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        void UpdateCamera()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return;
            var focus = target.position + Vector3.up * 1.25f;
            cam.transform.position = focus + Quaternion.Euler(12f, target.eulerAngles.y + 155f, 0f) * Vector3.forward * distance;
            cam.transform.LookAt(focus);
        }
    }
}
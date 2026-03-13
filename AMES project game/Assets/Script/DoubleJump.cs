using UnityEngine;
using System.Reflection;

namespace AmesGame
{
    public class DoubleJump : MonoBehaviour
    {
        public float DoubleJumpHeight = 1.2f;

        private bool _hasDoubleJumped;

        private PlayerController _controller;

        private FieldInfo verticalVelocityField;

        void Start()
        {
            _controller = GetComponent<PlayerController>();

            verticalVelocityField =
                typeof(PlayerController).GetField("_verticalVelocity",
                BindingFlags.NonPublic | BindingFlags.Instance);
        }

        void Update()
        {
            if (_controller.Grounded)
            {
                _hasDoubleJumped = false;
                return;
            }

            if (!_hasDoubleJumped && Input.GetKeyDown(KeyCode.Space))
            {
                float gravity = _controller.Gravity;

                float velocity = Mathf.Sqrt(DoubleJumpHeight * -2f * gravity);

                verticalVelocityField.SetValue(_controller, velocity);

                _hasDoubleJumped = true;
            }
        }
    }
}
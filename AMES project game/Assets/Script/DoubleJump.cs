using UnityEngine;
using System.Reflection;

namespace AmesGame
{
    public class DoubleJump : MonoBehaviour
    {
        public float DoubleJumpHeight = 1.2f;

        private bool _hasDoubleJumped;

        private PlayerController _controller;
        void Start()
        {
            _controller = GetComponent<PlayerController>();
        }
        

        void Update()
        {
            if (_controller == null)
                return;

            // reset when grounded
            if (_controller.Grounded)
            {
                _hasDoubleJumped = false;
                return;
            }

            // Support both the old Input.GetKeyDown and the Input "Jump" button so double-jump works
            bool jumpPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Jump");

            // allow a single extra jump while airborne
            if (!_hasDoubleJumped && jumpPressed)
            {
                float gravity = _controller.Gravity;
                float velocity = Mathf.Sqrt(DoubleJumpHeight * -2f * gravity);

                // directly set the player's vertical velocity to perform the double-jump
                _controller._verticalVelocity = velocity;

                _hasDoubleJumped = true;
            }
        }
    }
}
using UnityEngine;

namespace Game.Input
{
    public sealed class VirtualInput : MonoBehaviour
    {
        [Header("Output")]
        public InputMonitor InputMonitor;

        public void VirtualMoveInput(Vector2 virtualMoveDirection)
        {
            InputMonitor.MoveInput(virtualMoveDirection);
        }

        public void VirtualLookInput(Vector2 virtualLookDirection)
        {
            InputMonitor.LookInput(virtualLookDirection);
        }

        public void VirtualJumpInput(bool virtualJumpState)
        {
            InputMonitor.JumpInput(virtualJumpState);
        }

        public void VirtualSprintInput(bool virtualSprintState)
        {
            InputMonitor.SprintInput(virtualSprintState);
        }
    }
}
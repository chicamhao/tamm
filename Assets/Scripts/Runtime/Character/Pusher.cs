using UnityEngine;

namespace Game.Character
{
	public sealed class Pusher : MonoBehaviour
	{
		public LayerMask PushLayers;
		public bool CanPush;
		[Range(0.5f, 5f)] public float Strength = 1.1f;

		private void OnControllerColliderHit(ControllerColliderHit hit)
		{
			if (CanPush) PushRigidBodies(hit);
		}

        // https://docs.unity3d.com/ScriptReference/CharacterController.OnControllerColliderHit.html
        private void PushRigidBodies(ControllerColliderHit hit)
		{
			Rigidbody body = hit.collider.attachedRigidbody;
			if (body == null || body.isKinematic) return;

			var bodyLayerMask = 1 << body.gameObject.layer;
			if ((bodyLayerMask & PushLayers.value) == 0) return;

			// We dont want to push objects below us
			if (hit.moveDirection.y < -0.3f) return;

			Vector3 pushDir = new(hit.moveDirection.x, 0.0f, hit.moveDirection.z);

			body.AddForce(pushDir * Strength, ForceMode.Impulse);
		}
	}
}
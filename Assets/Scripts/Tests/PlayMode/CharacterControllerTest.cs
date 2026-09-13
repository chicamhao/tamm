using Game.Character;
using Game.Input;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace Game.Character.Tests
{
	// Play-mode tests: Controller is a render-loop MonoBehaviour with no seams
	// (all fields private, camera/movement code frozen), so behavior is verified
	// by driving the real components in a running scene.
	//
	// Scene layout per test (self-built, no prefab/asset coupling):
	//   MainCamera-tagged GO                     -> Controller.Awake's camera probe
	//   Player: CharacterController + Rigidbody
	//           + PlayerInput + InputMonitor + Controller  (the real wiring)
	//   CamTarget: plain GO                      -> CinemachineCameraTarget
	public sealed class CharacterControllerTest
	{
		private sealed class Player
		{
			public GameObject Body;
			public GameObject CinemachineTarget;
			public InputMonitor Input;
			public Controller Main;
		}

		private static Player BuildPlayer()
		{
			GameObject camera = new("MainCamera");
			camera.tag = "MainCamera";

			Player p = new();
			p.Body = new("Player");
			p.CinemachineTarget = new("CamTarget");
			p.Body.AddComponent<CharacterController>();
			p.Body.AddComponent<Rigidbody>();
			p.Body.AddComponent<PlayerInput>();
			p.Input = p.Body.AddComponent<InputMonitor>();
			p.Main = p.Body.AddComponent<Controller>();
			p.Main.CinemachineCameraTarget = p.CinemachineTarget;
			p.Main.Grounded = true; // physics overrides this each frame; jump test adds real ground

			return p;
		}

		// Make Look accumulate fast enough to reach the clamp within test-seconds.
		// RotationSpeed is a test fixture, not a production value.
		private static Player BuildFastPlayer()
		{
			Player p = BuildPlayer();
			p.Main.RotationSpeed = 60.0f;
			return p;
		}

		// --- Movement (character) ---

		[UnityTest] public IEnumerator MoveInput_AdvancesCharacter_OnTheGroundPlane()
		{
			Player p = BuildPlayer();
			Vector3 start = p.Body.transform.position;
			p.Input.Move = new Vector2(0.0f, 1.0f); // local forward

			yield return new WaitForSeconds(1.5f);

			// Horizontal speed ramps to MoveSpeed (4 m/s); the character also sinks in
			// the empty test scene, so assert on the ground plane only.
			Vector3 delta = p.Body.transform.position - start;
			float horizontal = new Vector2(delta.x, delta.z).magnitude;
			Assert.That(horizontal, Is.GreaterThan(1.0f), "character did not advance in the move direction");
			Assert.That(delta.z, Is.GreaterThan(0.0f), "character did not move along +Z (local forward)");
		}

		// --- Jump (character) ---

		[UnityTest] public IEnumerator Jump_WhenGrounded_RaisesTheCharacter()
		{
			Player p = BuildPlayer();
			p.Input.Move = Vector2.zero; // keep standing

			// Real ground so the grounded check stays true and the jump can trigger.
			GameObject ground = new("Ground");
			Rigidbody groundBody = ground.AddComponent<Rigidbody>();
			groundBody.isKinematic = true;
			ground.AddComponent<BoxCollider>();
			ground.transform.position = new Vector3(0.0f, -0.64f, 0.0f); // just below grounded-sphere

			yield return new WaitForSeconds(0.2f); // let the grounded check settle
			Assert.That(p.Main.Grounded, Is.True, "character is not grounded above the test ground");

			float startY = p.Body.transform.position.y;
			p.Input.Jump = true;
			yield return new WaitForSeconds(0.05f); // frame(s) where the jump fires
			p.Input.Jump = false;
			yield return new WaitForSeconds(0.35f); // rise phase

			Assert.That(p.Body.transform.position.y, Is.GreaterThan(startY + 0.25f), "character did not rise after jumping");
		}

		// --- Camera pitch (camera) ---

		[UnityTest] public IEnumerator CameraPitch_ClampsToTop_WhileLookingUp()
		{
			Player p = BuildFastPlayer();
			p.Input.Look = new Vector2(0.0f, 1.0f); // constant look up

			yield return new WaitForSeconds(2.5f); // 60 deg/s long past the 90 deg clamp

			float pitch = p.CinemachineTarget.transform.localRotation.eulerAngles.x;
			Assert.That(pitch, Is.EqualTo(p.Main.TopClamp).Within(2.0f), "pitch exceeded the top clamp");
		}

		[UnityTest] public IEnumerator CameraPitch_ClampsToBottom_WhileLookingDown()
		{
			Player p = BuildFastPlayer();
			p.Input.Look = new Vector2(0.0f, -1.0f); // constant look down

			yield return new WaitForSeconds(2.5f);

			float pitch = p.CinemachineTarget.transform.localRotation.eulerAngles.x;
			float wrapped = pitch > 180.0f ? pitch - 360.0f : pitch; // eulerAngles wraps to [-180, 180]
			Assert.That(wrapped, Is.EqualTo(p.Main.BottomClamp).Within(2.0f), "pitch exceeded the bottom clamp");
		}

		// --- Camera yaw (camera) ---

		[UnityTest] public IEnumerator LookHorizontal_RotatesTheCharacterBody()
		{
			Player p = BuildFastPlayer();
			float yawBefore = 0.0f;
			{ float y = p.Body.transform.rotation.eulerAngles.y; yawBefore = y > 180.0f ? y - 360.0f : y; }

			// Zero Move so the yaw read is not the only thing moving; rot is applied to the body.
			p.Input.Move = Vector2.zero;
			p.Input.Look = new Vector2(1.0f, 0.0f); // constant look right

			yield return new WaitForSeconds(1.5f);

			float yawAfter = 0.0f;
			{ float y = p.Body.transform.rotation.eulerAngles.y; yawAfter = y > 180.0f ? y - 360.0f : y; }
			Assert.That(Mathf.Abs(Mathf.DeltaAngle(yawBefore, yawAfter)), Is.GreaterThan(5.0f), "body did not yaw with horizontal look");
		}
	}
}
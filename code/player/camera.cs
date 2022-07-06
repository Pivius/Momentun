using Sandbox;
using TrickHop.Utility;

namespace TrickHop.Player
{
	public class MomentumCamera : FirstPersonCamera
	{
		protected Vector2 PreviousDelta;

		public override void Activated()
		{
			base.Activated();
			ZNear = 1;
		}
		private void ScaleSensitivity( ref Angles viewAng, Vector2 prevDelta, Vector2 mouseDelta )
		{
			MouseInput.MouseMove( ref viewAng, ref prevDelta, mouseDelta );
			PreviousDelta = prevDelta;
		}

		public override void BuildInput( InputBuilder input )
		{
			ScaleSensitivity( ref input.ViewAngles,
					PreviousDelta,
					new Vector2( input.AnalogLook.yaw * -100, input.AnalogLook.pitch * 100 ) );
			input.InputDirection = input.AnalogMove;
		}
	}
}

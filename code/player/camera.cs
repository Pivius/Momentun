using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sandbox;

namespace Momentum
{
    public class MomentumCamera : FirstPersonCamera
    {
		protected Vector2 PreviousDelta;

		public override void Activated()
		{
			base.Activated();
			ZNear = 1;
		}
		private void ScaleSensitivity(ref Angles view_angles, Vector2 previous_delta, Vector2 mouse_delta)
		{
			MouseInput.MouseMove(ref view_angles, ref previous_delta, mouse_delta);
			PreviousDelta = previous_delta;
		}

        public override void BuildInput(InputBuilder input)
		{
			ScaleSensitivity(ref input.ViewAngles, PreviousDelta, new Vector2(input.AnalogLook.yaw*-100, input.AnalogLook.pitch*100));
			input.InputDirection = input.AnalogMove;
		}
    }
}

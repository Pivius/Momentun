using System.Threading;
using System.Numerics;
using Sandbox;

namespace Momentum
{
	public partial class Accelerate : AirAccelerate
	{
		// Source Movement Accelerate

		public override float GetVelDiff(Vector3 velocity, float length, Vector3 wish_dir)
		{
			return length - velocity.Dot(wish_dir);
		}

		public override void Move(ref Vector3 velocity, Vector3 strafe_vel, float max_speed, float accelerate)
		{
			velocity = GetFinalVelocity(velocity, strafe_vel, max_speed, accelerate);
		}
	}
}

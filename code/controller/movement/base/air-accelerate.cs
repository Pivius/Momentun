using System.Threading;
using System.Numerics;
using Sandbox;

namespace Momentum
{
	public partial class AirAccelerate : BaseNetworkable
	{
		// # Source Movement Air Accel

		public virtual float CapWishSpeed(float wish_speed, float max_speed)
		{
			return MathX.Clamp(wish_speed, 0, max_speed);
		}

		public virtual float GetVelDiff(Vector3 velocity, float length, Vector3 strafe_dir)
		{
			return (length/10.0f) - velocity.Dot(strafe_dir);
		}

		public virtual Vector3 GetAccelSpeed(Vector3 strafe_dir, float length, float vel_diff, float accel)
		{
			return (strafe_dir * MathX.Clamp(length * accel * Time.Delta, 0, vel_diff));
		}

		public virtual Vector3 GetFinalVelocity(Vector3 velocity, Vector3 strafe_vel, float max_speed, float accel)
		{
			Vector3 strafe_dir = strafe_vel.Normal;
			float strafe_vel_length = CapWishSpeed(strafe_vel.Length, max_speed);
			float vel_diff = GetVelDiff(velocity, strafe_vel_length, strafe_dir);
			Vector3 accel_speed = GetAccelSpeed(strafe_dir, strafe_vel_length, vel_diff, accel);

			return velocity + accel_speed;
		}

		public virtual void Move(ref Vector3 velocity, Vector3 strafe_vel, float max_speed, float accelerate)
		{
			velocity = GetFinalVelocity(velocity, strafe_vel, max_speed, accelerate);
		}
		public virtual void Move(ref Vector3 velocity, Vector3 strafe_vel, float max_speed, float side_strafe_max_speed, float accelerate, float strafe_accelerate, float air_stop, float air_control)
		{
		}
	}
}

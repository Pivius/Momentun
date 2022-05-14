using Sandbox;

namespace Momentum
{
	public class QuakeAirAccelerate : AirAccelerate
	{
		// # Vanilla Quake 3 Air Accel

		public override float GetVelDiff(Vector3 velocity, float length, Vector3 strafe_dir)
		{
			return (length) - velocity.Dot(strafe_dir);
		}

		public virtual float AdjustAccel(float dot_vel, float accel, float strafe_accel, float air_stop_accel)
		{
			if (dot_vel < 0.0f || (Input.Forward != 0.0f && Input.Left == 0))
				accel = air_stop_accel;

			if (Input.Left != 0 && Input.Forward == 0)
				accel = strafe_accel;

			return accel;
		}

		public virtual Vector3 GetFinalVelocity(Vector3 velocity, Vector3 strafe_vel, float strafe_vel_length, float accel, float strafe_accel, float air_stop_accel)
		{
			Vector3 strafe_dir = strafe_vel.Normal;
			float vel_diff = GetVelDiff(velocity, strafe_vel_length, strafe_dir);
			Vector3 accel_speed = GetAccelSpeed(strafe_dir, strafe_vel_length, vel_diff, AdjustAccel(velocity.Dot(strafe_dir), accel, strafe_accel, air_stop_accel));

			return velocity + accel_speed;
		}

		public virtual Vector3 AirControl(Vector3 velocity, Vector3 strafe_dir, float air_control)
		{
			if (Input.Forward >= 1 && Input.Left == 0)
			{
				float vel_length;
				float dot_vel;
				float z_speed = velocity.z;
				float air_control_speed = 32;
				
				velocity = velocity.WithZ(0);
				vel_length = velocity.Length;
				velocity = velocity.Normal;
				dot_vel = velocity.Dot(strafe_dir);
				air_control_speed *= (air_control * dot_vel * dot_vel * Time.Delta);

				if (dot_vel > 0)
					velocity = ((velocity * vel_length) + (strafe_dir * air_control_speed)).Normal;

				velocity *= vel_length;
				velocity = velocity.WithZ(z_speed);
			}

			return velocity;
		}

		public override void Move(ref Vector3 velocity, Vector3 strafe_vel, float max_speed, float side_strafe_max_speed, float accelerate, float strafe_accelerate, float air_stop, float air_control)
		{
			float strafe_vel_length = CapWishSpeed(strafe_vel.Length, max_speed);

			if (Input.Left != 0 && Input.Forward == 0)
				strafe_vel_length = CapWishSpeed(strafe_vel.Length, side_strafe_max_speed);

			velocity = GetFinalVelocity(velocity, strafe_vel, strafe_vel_length, accelerate, strafe_accelerate, air_stop);

			if (air_control > 0)
				velocity = AirControl(velocity, strafe_vel.Normal, air_control);
		}
	}
}

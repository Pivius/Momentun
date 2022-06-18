using Sandbox;
using System;

namespace Momentum
{
	public static class Strafe
	{
		/// <summary>
		/// Returns the max cos strafe angle for air control
		/// </summary>
		public static float GetControlMaxCos( float deltaSpeed, float airControl, float opt )
		{
			float maxControl = deltaSpeed >= airControl ? 0 : MathF.Acos( deltaSpeed / airControl );

			return maxControl < opt ? opt : maxControl;
		}

		/// <summary>
		/// Returns the optimal strafe angle for air control
		/// </summary>
		public static float GetControlOpt( float airControl, float wishSpeed, float speed )
		{
			float num = wishSpeed - airControl;

			return MathF.Acos( num / speed );
		}

		/// <summary>
		/// Returns the minimal strafe angle
		/// </summary>
		public static float GetStrafeMin( float wishSpeed, float speed, float prevSpeedSqr, float speedSqr )
		{
			float num_squared = wishSpeed * wishSpeed - prevSpeedSqr + speedSqr;
			float num = MathF.Sqrt( num_squared );

			return num >= speed ? 0 : MathF.Acos( num / speed );
		}

		/// <summary>
		/// Returns the optimal strafe angle
		/// </summary>
		public static float GetStrafeOpt( float wishSpeed, float speed, float accelerate )
		{
			float num = wishSpeed - accelerate;

			return num >= speed ? 0 : MathF.Acos( num / speed );
		}

		/// <summary>
		/// Returns the max cos strafe angle
		/// </summary>
		public static float GetStrafeMaxCos( float opt, float deltaSpeed, float accelerate )
		{
			float maxCos = deltaSpeed >= accelerate ? 0 : MathF.Acos( deltaSpeed / accelerate );

			return maxCos < opt ? opt : maxCos;
		}

		/// <summary>
		/// Returns the max strafe angle
		/// </summary>
		public static float GetStrafeMax( float maxCos, float deltaSpeedSqr, float accelerateSqr, float accelerate, float speed )
		{
			float num = deltaSpeedSqr - accelerateSqr;
			float den = 2 * accelerate * speed;
			float max;

			if ( num >= den )
			{
				return 0;
			}
			else if ( -num >= den )
			{
				return MathF.PI;
			}

			max = MathF.Acos( num / den );

			return max < maxCos ? maxCos : max;
		}




		/// <summary>
		/// Returns the direction of strafe angle aligned with view in degrees
		/// </summary>
		public static float AlignWithDir( float angle, float dir, float scale, bool negate = false )
		{
			dir = MathX.RadianToDegree( dir );

			if ( negate )
				scale *= -1f;

			if ( MathX.RadianToDegree( angle ) < dir )
			{
				scale *= -1;
			}

			return Rotation.FromYaw( MathX.RadianToDegree( angle ) - dir ).Angle() * scale;
		}
	}
}

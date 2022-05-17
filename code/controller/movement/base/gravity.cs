using Sandbox;

namespace Momentum
{
	public class Gravity
	{
		// Source Gravity

		public static Vector3 AddGravity( float gravity, Vector3 velocity )
		{
			return velocity - new Vector3( 0, 0, gravity * Time.Delta );
		}
	}
}

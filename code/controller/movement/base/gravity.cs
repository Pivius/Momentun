using Sandbox;

namespace TrickHop.Movement
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

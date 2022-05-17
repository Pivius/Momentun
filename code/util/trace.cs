using Sandbox;

namespace Momentum
{
	public struct TraceUtil
	{
		private static void DebugHull( float duration, Vector3 start, Vector3 mins, Vector3 maxs, Color color )
		{
			//if (Host.IsClient)
			//DebugOverlay.Box(start, mins, maxs, color, duration);
		}

		public static TraceResult Hull(
			Vector3 start,
			Vector3 end,
			Vector3 mins,
			Vector3 maxs,
			Entity pawn,
			float liftFeet = 0.0f )
		{
			if ( liftFeet > 0 )
			{
				start += Vector3.Up * liftFeet;
				maxs.z -= liftFeet;
			}

			var tr = Trace.Ray( start, end )
						.Size( mins, maxs )
						.HitLayer( CollisionLayer.All, false )
						.HitLayer( CollisionLayer.Solid, true )
						.HitLayer( CollisionLayer.GRATE, true )
						.HitLayer( CollisionLayer.PLAYER_CLIP, true )
						.Ignore( pawn )
						.Run();

			return tr;
		}

		public static TraceResult PlayerBBox( Vector3 start,
										Vector3 end,
										BaseController controller,
										float liftFeet = 0.0f )
		{
			Vector3 maxs = controller.GetPlayerMaxs();
			Vector3 mins = controller.GetPlayerMins();

			if ( liftFeet > 0 )
			{
				start += Vector3.Up * liftFeet;
				maxs.z -= liftFeet;
			}

			var tr = Trace.Ray( start, end )
						.Size( mins, maxs )
						.HitLayer( CollisionLayer.All, false )
						.HitLayer( CollisionLayer.Solid, true )
						.HitLayer( CollisionLayer.GRATE, true )
						.HitLayer( CollisionLayer.PLAYER_CLIP, true )
						.Ignore( controller.Pawn )
						.Run();

			return tr;
		}

		public static Trace NewHull( Vector3 start,
								Vector3 end,
								Vector3 mins,
								Vector3 maxs,
								float liftFeet = 0.0f )
		{
			if ( liftFeet > 0 )
			{
				start += Vector3.Up * liftFeet;
				maxs.z -= liftFeet;
			}

			var tr = Trace.Ray( start, end ).Size( mins, maxs );

			return tr;
		}

		public static TraceResult PlayerLine( Vector3 start, Vector3 end, Entity pawn )
		{
			var tr = Trace.Ray( start, end )
						.HitLayer( CollisionLayer.All, false )
						.HitLayer( CollisionLayer.Solid, true )
						.HitLayer( CollisionLayer.GRATE, true )
						.HitLayer( CollisionLayer.PLAYER_CLIP, true )
						.Ignore( pawn )
						.Run();

			return tr;
		}

		public static Trace NewLine( Vector3 start, Vector3 end )
		{
			var tr = Trace.Ray( start, end )
						.HitLayer( CollisionLayer.All, false )
						.HitLayer( CollisionLayer.Solid, true )
						.HitLayer( CollisionLayer.GRATE, true )
						.HitLayer( CollisionLayer.PLAYER_CLIP, true );

			return tr;
		}
	}
}

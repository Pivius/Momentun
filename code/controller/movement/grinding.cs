using Sandbox;
using System;

namespace Momentum
{
	public partial class Grind : PredictedComponent
	{
		[Net, Predicted]
		public bool IsGrinding { get; set; }
		[Net, Predicted]
		public TimeSince GrindTime { get; set; }
		[Net, Predicted]
		public Vector3 LastNormal { get; set; }
		[Net, Predicted]
		public Vector3 LastGrindPos { get; set; }
		[Net, Predicted]
		public float Speed { get; set; }

		public TraceResult TraceLine( Vector3 start, Vector3 end )
		{
			return Sandbox.Trace.Ray( start, end )
				.HitLayer( CollisionLayer.All, false )
				.HitLayer( CollisionLayer.Solid, true )
				.HitLayer( CollisionLayer.GRATE, true )
				.HitLayer( CollisionLayer.SKY, false )
				.HitLayer( CollisionLayer.PLAYER_CLIP, true )
				.Ignore( Player )
				.Run();
		}

		public float GetDotVel( Vector3 normal )
		{
			return Controller.Velocity.Normal.WithZ( 0 )
				.SubtractDirection( normal.WithZ( 0 ), 2f )
				.Clamp( -1, 1 )
				.Dot( Controller.Velocity.Normal.WithZ( 0 ) );
		}

		public TraceResult? Trace()
		{
			Vector3 maxs = Controller.GetPlayerMaxs().WithZ( 0 );
			Vector3 mins = Controller.GetPlayerMins().WithZ( 0 );
			Vector3 start = Controller.Position;
			float distance = 4f;
			Vector3 end = start.WithZ( start.z + MathF.Min( Velocity.z, 0 ) - distance );
			TraceResult traceHull = Sandbox.Trace.Ray( start, end )
						.Size( mins, maxs.WithZ( 1 ) )
						.HitLayer( CollisionLayer.All, false )
						.HitLayer( CollisionLayer.Solid, true )
						.HitLayer( CollisionLayer.GRATE, true )
						.HitLayer( CollisionLayer.SKY, false )
						.HitLayer( CollisionLayer.PLAYER_CLIP, true )
						.Ignore( Player )
						.Run();

			if ( traceHull.Hit )
			{
				Vector3 linePosition = Controller.Position.WithZ( start.z - distance );
				TraceResult traceLine = TraceLine( linePosition + maxs / 2, linePosition + mins / 2 );

				if ( traceLine.StartedSolid || !traceLine.Hit )
				{
					traceLine = TraceLine( linePosition + maxs.WithX( mins.x ) / 2, linePosition + mins.WithX( maxs.x ) / 2 );

					if ( traceLine.StartedSolid || !traceLine.Hit )
					{
						traceLine = TraceLine( linePosition + mins / 2, linePosition + maxs / 2 );

						if ( traceLine.StartedSolid || !traceLine.Hit )
						{
							traceLine = TraceLine( linePosition + mins.WithX( maxs.x ) / 2, linePosition + maxs.WithX( mins.x ) / 2 );
						}
					}
				}

				if ( traceLine.Hit && !traceLine.StartedSolid )
				{
					return traceLine;
				}
			}

			return null;
		}

		public override void Simulate()
		{
			if ( Input.Down( InputButton.Run ) && Controller.Velocity.WithZ( 0 ).Length > 1 )
			{
				TraceResult? trace = Trace();

				if ( trace != null )
				{
					TraceResult railTrace = (TraceResult)trace;
					Vector3 velocity = Controller.Velocity;
					Vector3 position = Controller.Position;
					Vector3 moveDir = Vector3.Cross( Vector3.Up, railTrace.Normal );
					Vector3 distanceFromRail = position.WithZ( 0 ) - railTrace.EndPosition.WithZ( 0 );
					float fixPosStep = distanceFromRail.Length * 2f;

					if ( !IsGrinding )
					{
						Speed = velocity.WithZ( 0 ).Length;
						IsGrinding = true;
					}

					if ( velocity.Normal.WithZ( 0 ).Dot( moveDir ) < 0 )
						moveDir *= -1;

					Controller.Velocity = moveDir * Speed - (distanceFromRail.Normal * fixPosStep);
					Controller.Position = position.WithZ( railTrace.EndPosition.z + 4f );
					Controller.GroundEntity = null;
					Controller.CategorizePosition( false );

					if ( railTrace.Normal != LastNormal )
					{
						LastNormal = railTrace.Normal;
						LastGrindPos = railTrace.EndPosition;
					}
				}
				else
				{
					IsGrinding = false;
					LastNormal = Vector3.Zero;
					LastGrindPos = Vector3.Zero;
				}
			}
		}
	}
}

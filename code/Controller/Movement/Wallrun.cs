using Sandbox;
using System;
using TrickHop.Utility;

namespace TrickHop.Movement
{
	public partial class Wallrun : PredictedComponent
	{
		[Net]
		public float Delay { get; set; } = 0.5f;
		[Net]
		public float StickTime { get; set; } = 2f;
		[Net]
		public float SideSpeed { get; set; } = 150f;
		[Net]
		public float UpSpeed { get; set; } = 700f;
		[Net]
		public float MaxAngle { get; set; } = 60f;
		[Net]
		public float TraceDistance { get; set; } = 50f;
		[Net, Predicted]
		public TimeUntil WalljumpTime { get; set; }
		[Net, Predicted]
		public TimeSince TimeSinceStick { get; set; }
		[Net, Predicted]
		public bool IsSticking { get; set; } = false;
		[Net, Predicted]
		public float WallRunSpeed { get; set; }
		[Net, Predicted]
		public Vector3 WallDir { get; set; }
		[Net, Predicted]
		public Vector3 WallRunDir { get; set; }
		[Net, Predicted]
		public Vector3 WallPosition { get; set; }
		[Net, Predicted]
		public Vector3 LastWall { get; set; }


		public static float GetAngle( Vector3 trace_direction, Vector3 wall_normal )
		{
			return Vector3.GetAngle( trace_direction, wall_normal.WithZ( 0 ) );
		}

		public TraceResult Trace( Vector3 direction )
		{
			var maxs = Controller.GetPlayerMaxs();
			var mins = Controller.GetPlayerMins();
			var position = Controller.Position.WithZ( Controller.Position.z + 2 );
			Vector3 perimeter_pos = position - (Vector3.One * direction * maxs.x);
			TraceResult trace;

			perimeter_pos.x = MathX.Clamp( perimeter_pos.x, position.x + mins.x, position.x + maxs.x );
			perimeter_pos.y = MathX.Clamp( perimeter_pos.y, position.y + mins.y, position.y + maxs.y );
			perimeter_pos.z = position.z;
			trace = Sandbox.Trace.Ray( position, perimeter_pos )
						.Size( mins, maxs )
						.HitLayer( CollisionLayer.All, false )
						.HitLayer( CollisionLayer.Solid, true )
						.HitLayer( CollisionLayer.GRATE, true )
						.HitLayer( CollisionLayer.SKY, false )
						.HitLayer( CollisionLayer.PLAYER_CLIP, true )
						.Ignore( Player )
						.Run();
			return trace;
		}

		public void StartWallRun( Vector3 velocity, Vector3 traceNormal, float velLength2D )
		{
			Vector3 moveDir = Vector3.Cross( Vector3.Up, traceNormal );

			if ( velocity.Normal.Dot( moveDir ) < 0 )
				moveDir *= -1;

			IsSticking = true;
			TimeSinceStick = 0;
			WallDir = traceNormal;
			WallRunDir = moveDir;
			WallRunSpeed = velLength2D;
		}

		public void StopWallrun( Rotation eyeRotation )
		{
			Vector3 eyeNormal = eyeRotation.Forward.Normal;

			eyeNormal.z *= 1.2f;
			Controller.Velocity += eyeNormal * new Vector3( SideSpeed, SideSpeed, UpSpeed );
			IsSticking = false;
			WalljumpTime = Delay;
		}

		public override void Simulate()
		{
			Rotation eyeRotation = Controller.EyeRotation;
			Vector3 right = eyeRotation.Right;
			Vector3 traceDirection = new( -right.y, right.x, 0 );
			TraceResult trace = Trace( traceDirection );
			Vector3 traceNormal = trace.Normal;
			float angle = Vector3.GetAngle( traceDirection, traceNormal );
			InputButton button = InputButton.SecondaryAttack;
			bool shouldStick = TimeSinceStick <= StickTime;
			bool traceHit = trace.Hit;
			Vector3 velocity = Controller.Velocity;
			float velLength2D = velocity.WithZ( 0 ).LengthSquared;
			float slideSquared = MathF.Pow( 100f, 2 );

			if (
				WalljumpTime <= 0 &&
				Input.Down( button ) &&
				Controller.GroundEntity == null &&
				!IsSticking &&
				angle <= MaxAngle &&
				traceHit &&
				velLength2D > slideSquared &&
				!shouldStick )
			{
				StartWallRun( velocity, traceNormal, velLength2D );
			}
			else if (
				(Input.Released( button ) ||
				!shouldStick ||
				angle > MaxAngle ||
				!traceHit) &&
				IsSticking )
			{
				StopWallrun( eyeRotation );
			}

			if ( IsSticking && Input.Down( button ) )
			{
				Vector3 velDir = WallRunDir - (WallDir * 0.5f);

				velDir = velDir.Clamp( -1, 1 );
				WallRunSpeed = MathX.Clamp(
					InterpFunctions.InQuart( WallRunSpeed, -slideSquared, TimeSinceStick, StickTime ),
					slideSquared,
					WallRunSpeed );
				Controller.Velocity = velDir * MathF.Sqrt( WallRunSpeed );
			}

		}
	}
}

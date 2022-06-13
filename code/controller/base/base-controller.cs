using Sandbox;
using System;

namespace Momentum
{
	public abstract partial class BaseController : WalkController
	{
		public Vector3 LadderNormal;
		public bool IsTouchingLadder = false;
		public float ViewOffset { get; set; } = 64.0f;
		public Vector3 OBBMins { get; set; } = new Vector3( -16, -16, 0 );
		public Vector3 OBBMaxs { get; set; } = new Vector3( 16, 16, 72 );
		public Vector3 OldVelocity { get; set; }
		[Net, Predicted]
		public bool IsSurfing { get; set; }

		public BaseController()
		{
			AirAccelerate = new AirAccelerate();
			Accelerate = new Accelerate();
			Gravity = new Gravity();
			Friction = new Friction();
			Unstuck = new Unstuck( this );
		}

		public MomentumPlayer Player => Pawn as MomentumPlayer;

		public override TraceResult TraceBBox( Vector3 start,
										Vector3 end,
										float liftFeet = 0.0f )
		{
			return TraceBBox( start, end, OBBMins, OBBMaxs, liftFeet );
		}

		public override BBox GetHull()
		{
			return new BBox( OBBMins, OBBMaxs );
		}

		public virtual Vector3 GetPlayerMins( bool isDucked )
		{
			return isDucked ? (Player.Properties.DuckMins * Pawn.Scale) :
							(Player.Properties.StandMins * Pawn.Scale);
		}

		public virtual Vector3 GetPlayerMaxs( bool isDucked )
		{
			return isDucked ? (Player.Properties.DuckMaxs * Pawn.Scale) :
							(Player.Properties.StandMaxs * Pawn.Scale);
		}

		public virtual Vector3 GetPlayerMins()
		{
			return Player.Duck.IsDucked ? (Player.Properties.DuckMins * Pawn.Scale) :
										(Player.Properties.StandMins * Pawn.Scale);
		}

		public virtual Vector3 GetPlayerMaxs()
		{
			return Player.Duck.IsDucked ? (Player.Properties.DuckMaxs * Pawn.Scale) :
										(Player.Properties.StandMaxs * Pawn.Scale);
		}

		public virtual float GetPlayerViewOffset( bool isDucked )
		{
			return isDucked ? (Player.Properties.DuckViewOffset * Pawn.Scale) :
							(Player.Properties.StandViewOffset * Pawn.Scale);
		}

		public virtual float GetViewOffset()
		{
			return ViewOffset * Pawn.Scale;
		}

		public override void SetBBox( Vector3 mins, Vector3 maxs )
		{
			OBBMins = mins;
			OBBMaxs = maxs;
		}

		public override void UpdateBBox()
		{
			var mins = GetPlayerMins();
			var maxs = GetPlayerMaxs();

			if ( OBBMins != mins || OBBMaxs != maxs )
			{
				SetBBox( mins, maxs );
			}
		}

		public bool OnGround() => GroundEntity != null;

		public float FallDamage()
		{
			return MathF.Max( Velocity.z - 580.0f, 0 )
				* Player.Properties.FallDamageMultiplier;
		}

		public override void FrameSimulate()
		{
			base.FrameSimulate();

			EyeLocalPosition = Vector3.Up * GetViewOffset() * Pawn.Scale;
			WishVelocity = WishVel( Player.Properties.MaxMove );
		}

		public override void Simulate()
		{
			if ( StartMove() )
				return;

			if ( SetupMove() )
				return;

			EndMove();
		}
	}
}

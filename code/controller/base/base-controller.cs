using System.Diagnostics;
using System.Runtime.InteropServices;
using System;
using Sandbox;
using System.Threading.Tasks;

namespace Momentum
{
	public abstract partial class BaseController : WalkController
	{
		public Vector3 LadderNormal;
		public bool IsTouchingLadder = false;
		public float ViewOffset{get; set;} = 64.0f;
		public Vector3 OBBMins{get; set;} = new Vector3(-16, -16, 0);
		public Vector3 OBBMaxs{get; set;} = new Vector3(16, 16, 72);
		public Vector3 OldVelocity{get; set;}
		public TimeAssociatedMap<bool> ShouldClip { get; set; }
		public TimeSince ClipTime { get; set; }

		public BaseController()
		{
			AirAccelerate = new AirAccelerate();
			Duck = new Duck(this);
			Accelerate = new Accelerate();
			Gravity = new Gravity();
			Friction = new Friction();
			Water = new Water();
			Unstuck = new Unstuck(this);
			ShouldClip = new TimeAssociatedMap<bool>( 1f, GetShouldClip );
		}

		public virtual MomentumPlayer GetPlayer() => (MomentumPlayer)Pawn;
		public Property MoveProp => GetPlayer().MovementProps;
		public Property ViewProp => GetPlayer().ViewProps;

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
			return isDucked ? ((Vector3)ViewProp["DuckMins"] * Pawn.Scale) : 
							((Vector3)ViewProp["StandMins"] * Pawn.Scale);
		}

		public virtual Vector3 GetPlayerMaxs( bool isDucked )
		{
			return isDucked ? ((Vector3)ViewProp["DuckMaxs"] * Pawn.Scale) : 
							((Vector3)ViewProp["StandMaxs"] * Pawn.Scale);
		}

		public virtual Vector3 GetPlayerMins()
		{
			return Duck.ShouldDuck.Value ? ((Vector3)ViewProp["DuckMins"] * Pawn.Scale) : 
										((Vector3)ViewProp["StandMins"] * Pawn.Scale);
		}

		public virtual Vector3 GetPlayerMaxs()
		{
			return Duck.ShouldDuck.Value ? ((Vector3)ViewProp["DuckMaxs"] * Pawn.Scale) : 
										((Vector3)ViewProp["StandMaxs"] * Pawn.Scale);
		}

		public virtual float GetPlayerViewOffset( bool isDucked )
		{
			return isDucked ? ((float)ViewProp["DuckViewOffset"] * Pawn.Scale) : 
							((float)ViewProp["StandViewOffset"] * Pawn.Scale);
		}

		public virtual float GetViewOffset()
		{
			return ViewOffset * Pawn.Scale;
		}

		public override void SetBBox(Vector3 mins, Vector3 maxs)
		{
			OBBMins = mins;
			OBBMaxs = maxs;
		}

		public override void UpdateBBox()
		{
			var mins = GetPlayerMins();
			var maxs = GetPlayerMaxs();

			if (OBBMins != mins || OBBMaxs != maxs)
			{
				SetBBox(mins, maxs);
			}
		}

		public bool GetShouldClip()
		{
			if ( GetPlayer().IsServer )
				return ClipTime <= (float)MoveProp["ClipTime"];
			else
				return ShouldClip.LastValue && (ClipTime <= (float)MoveProp["ClipTime"]);
		}

		public bool OnGround() => GroundEntity != null;

		public float FallDamage()
		{
			return MathF.Max( Velocity.z - 580.0f, 0 )
				* (float)MoveProp["FallDamageMultiplier"];
		}

		public override void FrameSimulate()
		{
			base.FrameSimulate();

			EyeLocalPosition = Vector3.Up * GetViewOffset() * Pawn.Scale;
			WishVelocity = WishVel( (float)MoveProp["MaxMove"]);
		}

		public override void Simulate()
		{
			if (StartMove()) 
				return;

			if (SetupMove()) 
				return;
			
			EndMove();
		}
	}
}

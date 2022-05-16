using System.Diagnostics;
using System.Runtime.InteropServices;
using System;
using Sandbox;

namespace Momentum
{
	public abstract partial class BaseController : WalkController
	{
		// # Fields
		public AirAccelerate AirAccelerate;
		public new Accelerate Accelerate;
		public new Gravity Gravity;
		public Friction Friction;
		public Water Water;
		public new Duck Duck;

		public float GetWalkSpeed()
		{
			float walk_speed = (GetPlayer().KeyDown(InputButton.Walk) && (bool)MoveProp["CanWalk"]) ? (float)MoveProp["WalkSpeed"] : ((GetPlayer().KeyDown(InputButton.Run) && (bool)MoveProp["CanRun"]) ? (float)MoveProp["RunSpeed"] : (float)MoveProp["DefaultSpeed"]);
			//return walk_speed;
			return Duck.IsDucked ? walk_speed * ((float)MoveProp["DuckedWalkSpeed"]/(float)MoveProp["DefaultSpeed"]) : walk_speed;
		}

		public Vector3 WishVel(float strafe_speed)
		{	
			Vector3 forward = Input.Rotation.Forward;
			float forward_speed = Input.Forward * strafe_speed;
			float side_speed = Input.Left * strafe_speed;
			Vector3 forward_wish = new Vector3(forward.x, forward.y, 0).Normal * forward_speed;
			Vector3 side_wish = new Vector3(-forward.y, forward.x, 0).Normal * side_speed;

			return forward_wish + side_wish;
		}

		public Vector3 ClipVelocity(Vector3 velocity, Vector3 normal)
		{	
			return velocity - (normal * velocity.Dot(normal));
		}

		/// <summary>
		/// Consistent speed boosts when landing on a slope
		/// </summary>
		public virtual void AddSlopeSpeed()
		{
			TraceResult trace = TraceUtil.PlayerBBox(Position, Position.WithZ(Position.z - 2), this);
			Vector3 normal = trace.Normal;

			if (normal.z < 1 && Velocity.z <= 0 && OnGround() && (STATE)MoveProp["MoveState"] == STATE.INAIR)
			{
				Velocity -= (normal * Velocity.Dot(normal));

				if (Velocity.Dot(normal) < 0)
					Velocity = ClipVelocity(Velocity, normal);
			}
		}

		public override void CategorizePosition(bool bStayOnGround)
		{
			SurfaceFriction = 1.0f;

			var point = Position - Vector3.Up * 2;
			var vBumpOrigin = Position;

			//
			//  Shooting up really fast.  Definitely not on ground trimed until ladder shit
			//
			float MaxNonJumpVelocity = 140.0f;
			bool bMovingUpRapidly = Velocity.z > MaxNonJumpVelocity;
			bool bMovingUp = Velocity.z > 0;

			bool bMoveToEndPos = false;

			if ( GroundEntity != null ) // and not underwater
			{
				bMoveToEndPos = true;
				point.z -= StepSize;
			}
			else if ( bStayOnGround )
			{
				bMoveToEndPos = true;
				point.z -= StepSize;
			}

			if ( bMovingUpRapidly || Swimming ) // or ladder and moving up
			{
				ClearGroundEntity();
				return;
			}

			var pm = TraceBBox( vBumpOrigin, point, 4.0f );

			if ( pm.Entity == null || Vector3.GetAngle( Vector3.Up, pm.Normal ) > (float)MoveProp["StandableAngle"] )
			{
				ClearGroundEntity();
				bMoveToEndPos = false;

				if ( Velocity.z > 0 )
					SurfaceFriction = 0.25f;
			}
			else
			{
				base.UpdateGroundEntity( pm );
			}

			if ( bMoveToEndPos && !pm.StartedSolid && pm.Fraction > 0.0f && pm.Fraction < 1.0f )
			{
				Position = pm.EndPosition;
			}
		}
		public override void ClearGroundEntity()
		{
			base.ClearGroundEntity();
		}
		public override void StepMove()
		{
			MoveHelper mover = new MoveHelper(Position, Velocity);
			mover.Trace = mover.Trace.Size(OBBMins, OBBMaxs).Ignore(Pawn);
			mover.MaxStandableAngle = (float)MoveProp["StandableAngle"];

			mover.TryMoveWithStep(Time.Delta, (float)MoveProp["StepSize"]);

			Position = mover.Position;
			Velocity = mover.Velocity;
		}

		/// <summary>
		/// Does sliding when on a ramp or surf
		/// </summary>
		public virtual void TryPlayerMove()
		{
			MoveHelper mover = new MoveHelper(Position, Velocity);
			mover.Trace = mover.Trace.Size(OBBMins, OBBMaxs).Ignore(Pawn);
			mover.MaxStandableAngle = (float)MoveProp["StandableAngle"];

			mover.TryMove(Time.Delta);

			Position = mover.Position;
			Velocity = mover.Velocity;
		}

		public override void AirMove()
		{
			var velocity = Velocity;
			AirAccelerate.Move(ref velocity, WishVelocity, (float)MoveProp["MaxSpeed"], (float)MoveProp["AirAccelerate"]);
			Velocity = velocity;
			Velocity += BaseVelocity;
			TryPlayerMove();
			Velocity -= BaseVelocity;
		}

		public override void StayOnGround()
		{
			var start = Position + Vector3.Up * 2;
			var end = Position + Vector3.Down * (float)MoveProp["StepSize"];

			// See how far up we can go without getting stuck
			var trace = TraceBBox(Position, start);
			start = trace.EndPosition;

			// Now trace down from a known safe position
			trace = TraceBBox(start, end);

			if (trace.Fraction <= 0) return;
			if (trace.Fraction >= 1) return;
			if (trace.StartedSolid) return;
			if (Vector3.GetAngle(Vector3.Up, trace.Normal) > (float)MoveProp["StandableAngle"]) return;

			// This is incredibly hacky. The real problem is that trace returning that strange value we can't network over.
			// float flDelta = fabs(mv->GetAbsOrigin().z - trace.m_vEndPos.z);
			// if (flDelta > 0.5f * DIST_EPSILON)

			Position = trace.EndPosition;
		}

		public override void WalkMove()
		{
			var wishdir = WishVelocity.Normal;
			var wishspeed = WishVelocity.Length;

			WishVelocity = WishVelocity.WithZ(0);
			WishVelocity = WishVelocity.Normal * wishspeed;
			Velocity = Velocity.WithZ(0);
			var velocity = Velocity;
			Accelerate.Move(ref velocity, WishVelocity, GetWalkSpeed(), (float)MoveProp["Accelerate"]);
			Velocity = velocity;
			Velocity = Velocity.WithZ(0);

			// Add in any base velocity to the current velocity.
			Velocity += BaseVelocity;

			try
			{
				if (Velocity.Length < 1.0f)
				{
					Velocity = Vector3.Zero;
					return;
				}

				// first try just moving to the destination	
				var dest = (Position + Velocity * Time.Delta).WithZ(Position.z);

				var pm = TraceUtil.PlayerBBox(Position, dest, this);

				if (pm.Fraction == 1)
				{
					Position = pm.EndPosition;
					StayOnGround();
					return;
				}

				StepMove();
			}
			finally
			{
				// Now pull the base velocity back out.  Base velocity is set if you are on a moving object, like a conveyor (or maybe another monster?)
				Velocity -= BaseVelocity;
			}

			StayOnGround();
		}
	
		public override void CheckJumpButton()
			{

			if (Water.JumpTime > 0.0f)
			{
				Water.JumpTime -= Time.Delta;

				if (Water.JumpTime < 0.0f)
					Water.JumpTime = 0;

				return;
			}
			
			if (Water.WaterLevel >= WATERLEVEL.Waist)
			{
				ClearGroundEntity();
				Velocity = Velocity.WithZ(100);
				return;
			}
			
			if (!OnGround())
				return;

			ClearGroundEntity();
			Velocity = Gravity.AddGravity((float)MoveProp["Gravity"] * 0.5f, Velocity.WithZ((float)MoveProp["JumpPower"]));
			AddEvent("jump");

			//Duck.JumpTime = Duck.JUMP_TIME;
			//Duck.InDuckJump = true;
		}

		public override void CheckLadder()
		{
			if (IsTouchingLadder && GetPlayer().KeyPressed(InputButton.Jump))
			{
				Velocity = LadderNormal * 100.0f;
				IsTouchingLadder = false;
				return;
			}

			const float ladderDistance = 1.0f;
			var start = Position;
			Vector3 end = start + (IsTouchingLadder ? (LadderNormal * -1.0f) : WishVelocity.Normal) * ladderDistance;

			var pm = Trace.Ray(start, end)
						.Size(OBBMins, OBBMaxs)
						.HitLayer(CollisionLayer.All, false)
						.HitLayer(CollisionLayer.LADDER, true)
						.Ignore(Pawn)
						.Run();

			IsTouchingLadder = false;

			if (pm.Hit)
			{
				IsTouchingLadder = true;
				LadderNormal = pm.Normal;
			}
		}

		/// <summary>
		/// Runs first in the simulate method
		/// </summary>
		/// <returns>
		/// return true to not run anything past this event
		/// </returns>
		public virtual bool StartMove()
		{
			//if (Host.IsServer)
			//Duck.TryDuck();
			//Log.Info( GetViewOffset() );
			EyeLocalPosition = Vector3.Up * GetViewOffset() * Pawn.Scale;
			EyeRotation = Input.Rotation;
			WishVelocity = WishVel((float)MoveProp["MaxMove"]);
			UpdateBBox();
			
			if (Unstuck.TestAndFix())
				return true;
			
			// RunLadderMode
			CheckLadder();

			return false;
		}

		/// <summary>
		/// Runs in the middle of the simulate method
		/// </summary>
		/// <returns>
		/// return true to not run anything past this event
		/// </returns>
		public virtual bool SetupMove()
		{
			var player = GetPlayer();
			Duck.ReduceTimers();
			Swimming = Water.CheckWater(Position, OBBMins, OBBMaxs, GetViewOffset(), Pawn);

			//
			// Start Gravity
			//
			if (!Swimming && !IsTouchingLadder)
			{
				Velocity = Gravity.AddGravity((float)MoveProp["Gravity"] * 0.5f, Velocity);
				Velocity += new Vector3(0, 0, BaseVelocity.z) * Time.Delta;
				BaseVelocity = BaseVelocity.WithZ(0);
			}

			if (Water.JumpTime > 0.0f)
			{
				Velocity = Water.WaterJump(Velocity);
				TryPlayerMove();
				return true;
			}

			if (Water.WaterLevel >= WATERLEVEL.Waist)
			{
				if (Water.WaterLevel == WATERLEVEL.Waist)
					Velocity = Water.CheckWaterJump(Velocity, Position, this);

				if (Velocity.z < 0.0f && Water.JumpTime > 0.0f)
					Water.JumpTime = 0.0f;

				if ((bool)MoveProp["AutoJump"] ? player.KeyDown(InputButton.Jump) : player.KeyPressed(InputButton.Jump))
					CheckJumpButton();
	
				Water.Move(this);
				CategorizePosition(OnGround());

				if (OnGround())
					Velocity = Velocity.WithZ(0);

				MoveProp["MoveState"] = STATE.WATER;
			}
			else
			{
		
				if ((bool)MoveProp["AutoJump"] ? player.KeyDown(InputButton.Jump) : player.KeyPressed(InputButton.Jump))
					CheckJumpButton();

				if (OnGround())
				{
					Velocity = Velocity.WithZ(0);
					var velocity = Velocity;
					Friction.Move(ref velocity, (float)MoveProp["Friction"], (float)MoveProp["StopSpeed"]);
					Velocity = velocity;
				}

				Duck.UpdateDuckJumpEyeOffset();
				Duck.Move();

				if (!IsTouchingLadder)
					WishVelocity = WishVelocity.WithZ(0);

				bool bStayOnGround = false;
				
				if (IsTouchingLadder)
				{
					LadderMove();
					MoveProp["MoveState"] = STATE.LADDER;
				}
				else if (OnGround())
				{
					bStayOnGround = true;
					WalkMove();
					MoveProp["MoveState"] = STATE.GROUND;
				}
				else
				{
					AirMove();
					MoveProp["MoveState"] = STATE.INAIR;
				}

				// FinishGravity
				if (!IsTouchingLadder)
					Velocity = Gravity.AddGravity((float)MoveProp["Gravity"] * 0.5f, Velocity);

				if (OnGround())
				{
					AddSlopeSpeed();
					Velocity = Velocity.WithZ(0);
				}

				CategorizePosition(bStayOnGround);
			}
			return false;
		}

		/// <summary>
		/// Is the last to run in the simulate method
		/// </summary>
		public virtual void EndMove()
		{
			OldVelocity = Velocity;
		}
	}
}

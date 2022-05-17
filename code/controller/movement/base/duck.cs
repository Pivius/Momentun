using Sandbox;
using System;


namespace Momentum
{
	public partial class Duck : BaseNetworkable
	{
		protected BaseController Controller;

		public bool IsDucked { get; set; }
		public bool IsDucking { get; set; }
		public bool InDuckJump { get; set; }
		public float DuckTimer { get; set; }
		public float DuckJumpTimer { get; set; }
		public float JumpTime { get; set; }
		public const float DuckingTime = 1000.0f;
		public const float JumpingTime = 600.0f;
		public const float TimeToUnDuck = 0.2f;
		public const float TimeToDuck = 0.4f;
		public const float TimeToUnduckInv = 1000.0f - 200.0f;
		public TimeAssociatedMap<bool> ShouldDuck { get; set; }

		public Duck( BaseController controller )
		{
			Controller = controller;
			ShouldDuck = new TimeAssociatedMap<bool>( 1f, GetShouldDuck );
		}

		public bool GetShouldDuck()
		{
			if ( Controller.GetPlayer().IsServer )
			{
				return IsDucked;
			}
			else
			{
				return ShouldDuck.LastValue && (IsDucked);
			}

		}
		private static float SimpleSpline( float value )
		{
			float valueSquared = value * value;

			return 3 * valueSquared - 2 * valueSquared * value;
		}

		public virtual Vector3 GetUnDuckOrigin( bool negate )
		{
			Vector3 newPosition = Controller.Position;

			if ( Controller.OnGround() )
				newPosition += Controller.GetPlayerMins( true ) - Controller.GetPlayerMins( false );
			else
			{
				Vector3 hull_normal = Controller.GetPlayerMaxs( false ) - Controller.GetPlayerMins( false );
				Vector3 hull_duck = Controller.GetPlayerMaxs( true ) - Controller.GetPlayerMins( true );
				Vector3 view_delta = (hull_normal - hull_duck);

				if ( negate )
					view_delta *= -1;

				newPosition += view_delta;
			}

			return newPosition;
		}

		public void FixPlayerCrouchStuck( bool upward )
		{
			int i;
			Vector3 newPosition;
			TraceResult trace;
			int direction = upward ? 1 : 0;

			trace = TraceUtil.PlayerBBox( Controller.Position,
								Controller.Position,
								Controller );

			if ( trace.StartedSolid )
			{
				newPosition = Controller.Position;

				for ( i = 0; i < 36; i++ )
				{
					Vector3 pos = Controller.Position;

					pos = pos.WithZ( pos.z + direction );
					Controller.Position = pos;
					trace = TraceUtil.PlayerBBox( Controller.Position,
								  Controller.Position,
								  Controller );

					if ( !trace.StartedSolid )
						return;
				}

				Controller.Position = newPosition;
			}
		}

		public bool CanUnDuck()
		{
			Vector3 newPosition = GetUnDuckOrigin( true );
			TraceResult trace;
			bool savedDuck = ShouldDuck.Value;

			IsDucked = false;
			ShouldDuck.Value = false;
			trace = TraceUtil.PlayerBBox( Controller.Position, newPosition, Controller );
			IsDucked = savedDuck;
			ShouldDuck.Value = savedDuck;

			if ( trace.StartedSolid || trace.Fraction != 1.0f )
				return false;

			return true;
		}

		public void FinishUnDuck()
		{
			Vector3 newPosition = GetUnDuckOrigin( true );

			IsDucked = false;
			ShouldDuck.Value = false;
			IsDucking = false;
			Controller.GetPlayer().RemoveFlag( PlayerFlags.DUCKING );
			InDuckJump = false;
			Controller.ViewOffset = Controller.GetPlayerViewOffset( false );
			Controller.Position = newPosition;
			DuckTimer = 0f;
			Controller.CategorizePosition( Controller.OnGround() );
		}

		public void SetDuckedEyeOffset( float frac )
		{
			Vector3 duckMins = Controller.GetPlayerMins( true );
			Vector3 standMins = Controller.GetPlayerMins( false );
			float more = duckMins.z - standMins.z;
			float duckView = Controller.GetPlayerViewOffset( true );
			float standView = Controller.GetPlayerViewOffset( false );
			float viewOffset = ((duckView - more) * frac) + (standView * (1 - frac));

			Controller.ViewOffset = viewOffset;
		}

		public void UpdateDuckJumpEyeOffset()
		{
			if ( DuckJumpTimer != 0.0f )
			{
				float duckMilliSec = MathF.Max( 0.0f, DuckingTime - DuckJumpTimer );
				float duckSec = duckMilliSec / DuckingTime;

				if ( duckSec > TimeToUnDuck )
				{
					DuckJumpTimer = 0.0f;
					SetDuckedEyeOffset( 0.0f );
				}
				else
				{
					float duckFrac = SimpleSpline( 1.0f - (duckSec / TimeToUnDuck) );
					SetDuckedEyeOffset( duckFrac );
				}
			}
		}

		public void FinishUnDuckJump( TraceResult trace )
		{
			Vector3 hullNormal = Controller.GetPlayerMaxs( false ) - Controller.GetPlayerMins( false );
			Vector3 hullDuck = Controller.GetPlayerMaxs( true ) - Controller.GetPlayerMins( true );
			Vector3 hullDelta = (hullNormal - hullDuck);
			Vector3 newPosition = Controller.Position;

			float deltaZ = hullDelta.z;
			hullDelta.z *= trace.Fraction;
			deltaZ -= hullDelta.z;

			Controller.GetPlayer().RemoveFlag( PlayerFlags.DUCKING );
			IsDucked = false;
			ShouldDuck.Value = false;
			IsDucking = false;
			InDuckJump = false;
			DuckTimer = 0.0f;
			DuckJumpTimer = 0.0f;
			JumpTime = 0.0f;

			float viewOffset = Controller.GetPlayerViewOffset( false );
			viewOffset -= deltaZ;
			Controller.ViewOffset = viewOffset;

			newPosition -= hullDelta;
			Controller.Position = newPosition;
			FixPlayerCrouchStuck( true );
			Controller.CategorizePosition( Controller.OnGround() );
		}

		public void FinishDuck()
		{
			if ( ShouldDuck.Value )
				return;

			Controller.GetPlayer().AddFlag( PlayerFlags.DUCKING );
			IsDucked = true;
			ShouldDuck.Value = true;
			IsDucking = false;

			Controller.ViewOffset = Controller.GetPlayerViewOffset( true );
			Controller.Position = GetUnDuckOrigin( false );
			FixPlayerCrouchStuck( true );
			Controller.CategorizePosition( Controller.OnGround() );
		}

		public void StartUnDuckJump()
		{
			Controller.GetPlayer().AddFlag( PlayerFlags.DUCKING );
			IsDucked = true;
			ShouldDuck.Value = true;
			IsDucking = false;

			Controller.ViewOffset = Controller.GetPlayerViewOffset( true );

			Vector3 newPosition = Controller.Position;
			Vector3 hullNormal = Controller.GetPlayerMaxs( false ) - Controller.GetPlayerMins( false );
			Vector3 hullDuck = Controller.GetPlayerMaxs( true ) - Controller.GetPlayerMins( true );
			Vector3 hullDelta = (hullNormal - hullDuck);

			newPosition += hullDelta;
			Controller.Position = newPosition;

			FixPlayerCrouchStuck( true );
			Controller.CategorizePosition( Controller.OnGround() );
		}

		public bool CanUnDuckJump( ref TraceResult trace )
		{
			Vector3 vecEnd = Controller.Position;
			vecEnd.z -= 36.0f;
			trace = TraceUtil.PlayerBBox( Controller.Position, vecEnd, Controller );

			if ( trace.Fraction < 1.0f )
			{
				vecEnd.z = Controller.Position.z + (-36.0f * trace.Fraction);

				TraceResult traceUp;
				traceUp = TraceUtil.Hull( vecEnd,
							 vecEnd,
							 Controller.GetPlayerMins( false ),
							 Controller.GetPlayerMaxs( false ),
							 Controller.Pawn );

				if ( !traceUp.StartedSolid )
					return true;
			}
			else if ( trace.Fraction == 1.0f )
				return true;

			return false;
		}

		public void Move()
		{
			bool inAir = !Controller.OnGround();
			bool inDuck = (ShouldDuck.Value);
			bool duckJump = JumpTime > 0.0f;
			bool duckJumpTime = DuckJumpTimer > 0.0f;
			bool duckButton = Input.Down( InputButton.Duck );

			if ( !inAir && inDuck && InDuckJump )
				InDuckJump = false;

			if ( duckButton || IsDucking || inDuck || duckJump )
			{
				if ( duckButton || duckJump )
				{
					if ( Input.Pressed( InputButton.Duck ) && !inDuck && !duckJumpTime )
					{
						DuckTimer = DuckingTime;
						IsDucking = true;
					}

					if ( IsDucking && !duckJump && !duckJumpTime )
					{
						float duckMilliSec = MathF.Max( 0.0f, DuckingTime - DuckTimer );
						float duckSec = duckMilliSec * 0.001f;

						if ( (duckSec > TimeToDuck) || inDuck || inAir )
						{
							FinishDuck();
						}
						else
						{
							float duckFrac = SimpleSpline( duckSec / TimeToDuck );
							SetDuckedEyeOffset( duckFrac );
						}
					}

					if ( duckJump && (IsDucking || inDuck) )
					{
						if ( !inDuck )
						{
							StartUnDuckJump();
						}
						else
						{
							if ( !duckButton )
							{
								TraceResult trace = TraceUtil.PlayerBBox( Controller.Position,
												 Controller.Position,
												 Controller );
								if ( CanUnDuckJump( ref trace ) )
								{
									FinishUnDuckJump( trace );
									DuckJumpTimer = (TimeToUnDuck
										* 1000f
										* (1.0f - trace.Fraction)) + TimeToUnduckInv;
								}
							}
						}
					}
				}
				else
				{
					if ( InDuckJump )
					{
						if ( duckButton )
						{
							InDuckJump = false;
						}
						else
						{
							TraceResult trace = TraceUtil.PlayerBBox( Controller.Position,
												Controller.Position,
												Controller );
							if ( CanUnDuckJump( ref trace ) )
							{
								FinishUnDuckJump( trace );

								if ( trace.Fraction < 1.0f )
								{
									DuckJumpTimer = (TimeToUnDuck
										* 1000f
										* (1.0f - trace.Fraction)) + TimeToUnduckInv;
								}
							}
						}
					}

					if ( duckJumpTime )
						return;

					if ( (bool)Controller.MoveProp["AllowAutoMovement"] || inAir || IsDucking )
					{
						if ( Input.Released( InputButton.Duck ) )
						{
							DuckTimer = DuckingTime;
						}
						else if ( IsDucking && !GetShouldDuck() )
						{
							float unDuckMilliSec = 1000.0f * TimeToUnDuck;
							float duckMilliSec = 1000.0f * TimeToDuck;
							float elapsedMilliSec = DuckingTime - DuckTimer;

							float duckFrac = elapsedMilliSec / duckMilliSec;
							float remainingDuckTime = duckFrac * unDuckMilliSec;

							DuckTimer = DuckingTime - unDuckMilliSec + remainingDuckTime;
						}
					}

					if ( CanUnDuck() )
					{
						if ( IsDucking || GetShouldDuck() )
						{
							float duckMilliSec = MathF.Max( 0.0f, DuckingTime - DuckTimer );
							float duckSec = duckMilliSec * 0.001f;

							if ( duckSec > TimeToUnDuck || (inAir && !duckJump) )
							{
								FinishUnDuck();
							}
							else
							{
								float duckFrac = SimpleSpline( 1.0f - (duckSec / TimeToUnDuck) );
								SetDuckedEyeOffset( duckFrac );
								IsDucking = true;
							}
						}
					}
					else
					{
						if ( DuckTimer != DuckingTime )
						{
							SetDuckedEyeOffset( 1.0f );
							DuckTimer = DuckingTime;
							IsDucked = true;
							ShouldDuck.Value = true;
							IsDucking = false;
							Controller.GetPlayer().AddFlag( PlayerFlags.DUCKING );
						}
					}
				}
			}

			if ( IsDucking || GetShouldDuck() )
			{
				Controller.SetTag( "ducked" );
			}
			//Log.Info( Controller.ViewOffset - Controller.GetPlayerViewOffset( false ) );
			if ( DuckJumpTimer == 0.0f && MathF.Abs( Controller.ViewOffset - Controller.GetPlayerViewOffset( false ) ) > 0.1 )
			{
				//SetDuckedEyeOffset( 0.0f );
			}
		}

		public void ReduceTimers()
		{
			float frameMilliSec = 1000.0f * Time.Delta;

			if ( DuckTimer > 0 )
			{
				DuckTimer -= frameMilliSec;

				if ( DuckTimer < 0 )
				{
					DuckTimer = 0;
				}
			}

			if ( DuckJumpTimer > 0 )
			{
				DuckJumpTimer -= frameMilliSec;

				if ( DuckJumpTimer < 0 )
				{
					DuckJumpTimer = 0;
				}
			}

			if ( JumpTime > 0 )
			{
				JumpTime -= frameMilliSec;

				if ( JumpTime < 0 )
				{
					JumpTime = 0;
				}
			}
		}
	}
}

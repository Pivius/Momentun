using Sandbox;
using System;
using TrickHop.Utility;

namespace TrickHop.Movement
{
	public partial class Duck : PredictedComponent
	{
		[Net, Predicted]
		public bool IsDucked { get; set; }
		[Net, Predicted]
		public bool IsDucking { get; set; }
		[Net, Predicted]
		public bool InDuckJump { get; set; }
		[Net, Predicted]
		public TimeUntil DuckTimer { get; set; }
		[Net, Predicted]
		public TimeUntil DuckJumpTimer { get; set; }
		[Net, Predicted]
		public TimeUntil JumpTime { get; set; }
		private float _duckingTime = 1f;
		private float _timeToUnDuck = 0.2f;
		public float DuckingTime
		{
			get => _duckingTime;

			set
			{
				_duckingTime = value;
				TimeToUnDuckInv = value - TimeToUnDuck;
			}
		}

		public float TimeToUnDuck
		{
			get => _timeToUnDuck;

			set
			{
				_timeToUnDuck = value;
				TimeToUnDuckInv = DuckingTime - value;
			}
		}

		public float JumpingTime = 0.6f;
		public float TimeToDuck = 0.4f;
		public float TimeToUnDuckInv;

		public Duck()
		{
			TimeToUnDuckInv = DuckingTime - TimeToUnDuck;
		}
		private static float SimpleSpline( float value )
		{
			float valueSquared = value * value;

			return 3 * valueSquared - 2 * valueSquared * value;
		}

		private void InvertDuckTime( bool isDucked )
		{
			float timeToUnDuck = TimeToUnDuck;
			float timeToDuck = TimeToDuck;
			float elapsedDuckTime = DuckingTime - DuckTimer;
			float duckFrac = elapsedDuckTime / (isDucked ? timeToUnDuck : timeToDuck);
			float remainingDuckTime = duckFrac * (isDucked ? timeToDuck : timeToUnDuck);

			DuckTimer = DuckingTime - (isDucked ? timeToDuck : timeToUnDuck) + remainingDuckTime;
		}

		public Vector3 GetUnDuckOrigin( bool negate )
		{
			Vector3 newPosition = Controller.Position;

			if ( Controller.GroundEntity != null )
				newPosition += Controller.GetPlayerMins( true ) - Controller.GetPlayerMins( false );
			else
			{
				Vector3 hull_normal = Controller.GetPlayerMaxs( false ) - Controller.GetPlayerMins( false );
				Vector3 hull_duck = Controller.GetPlayerMaxs( true ) - Controller.GetPlayerMins( true );
				Vector3 view_delta = hull_normal - hull_duck;

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

			trace = Controller.TraceBBox( Controller.Position, Controller.Position );

			if ( trace.StartedSolid )
			{
				newPosition = Controller.Position;

				for ( i = 0; i < 36; i++ )
				{
					Vector3 pos = Controller.Position;

					pos = pos.WithZ( pos.z + direction );
					Controller.Position = pos;
					trace = Controller.TraceBBox( Controller.Position, Controller.Position );

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
			bool savedDuck = IsDucked;

			IsDucked = false;
			trace = Controller.TraceBBox( Controller.Position, newPosition );
			IsDucked = savedDuck;

			if ( trace.StartedSolid || trace.Fraction != 1.0f )
				return false;

			return true;
		}

		public void FinishUnDuck()
		{
			Vector3 newPosition = GetUnDuckOrigin( true );

			IsDucked = false;
			IsDucking = false;
			InDuckJump = false;
			DuckTimer = 0f;

			Controller.ViewOffset = Controller.GetPlayerViewOffset( false );
			Controller.Position = newPosition;
			Controller.UpdateBBox();
			Controller.CategorizePosition( Controller.GroundEntity != null );
		}

		public void SetDuckedEyeOffset( float frac )
		{
			Vector3 duckMins = Controller.GetPlayerMins( true );
			Vector3 standMins = Controller.GetPlayerMins( false );
			float deltaView = duckMins.z - standMins.z;
			float duckView = Controller.GetPlayerViewOffset( true );
			float standView = Controller.GetPlayerViewOffset( false );
			float viewOffset = ((duckView - deltaView) * frac) + (standView * (1 - frac));

			Controller.ViewOffset = viewOffset;
		}

		public void UpdateDuckJumpEyeOffset()
		{
			if ( DuckJumpTimer > 0.0f )
			{
				float duckTime = MathF.Max( 0.0f, DuckingTime - DuckJumpTimer );

				if ( duckTime > TimeToUnDuck )
				{
					DuckJumpTimer = 0.0f;
					SetDuckedEyeOffset( 0.0f );
				}
				else
				{
					float duckFrac = SimpleSpline( 1.0f - (duckTime / TimeToUnDuck) );

					SetDuckedEyeOffset( duckFrac );
				}
			}
		}

		public void FinishUnDuckJump( TraceResult trace )
		{
			Vector3 hullNormal = Controller.GetPlayerMaxs( false ) - Controller.GetPlayerMins( false );
			Vector3 hullDuck = Controller.GetPlayerMaxs( true ) - Controller.GetPlayerMins( true );
			Vector3 hullDelta = hullNormal - hullDuck;
			Vector3 newPosition = Controller.Position;
			float viewOffset = Controller.GetPlayerViewOffset( false );
			float deltaZ = hullDelta.z;

			hullDelta.z *= trace.Fraction;
			deltaZ -= hullDelta.z;

			IsDucked = false;
			IsDucking = false;
			InDuckJump = false;
			DuckTimer = 0.0f;
			DuckJumpTimer = 0.0f;
			JumpTime = 0.0f;

			viewOffset -= deltaZ;
			Controller.ViewOffset = viewOffset;
			newPosition -= hullDelta;
			Controller.Position = newPosition;

			Controller.UpdateBBox();
			FixPlayerCrouchStuck( true );
			Controller.CategorizePosition( Controller.GroundEntity != null );
		}

		public void FinishDuck()
		{
			IsDucked = true;
			IsDucking = false;

			Controller.ViewOffset = Controller.GetPlayerViewOffset( true );
			Controller.Position = GetUnDuckOrigin( false );

			Controller.UpdateBBox();
			FixPlayerCrouchStuck( true );
			Controller.CategorizePosition( Controller.GroundEntity != null );
		}

		public void StartUnDuckJump()
		{
			IsDucked = true;
			IsDucking = false;
			Controller.ViewOffset = Controller.GetPlayerViewOffset( true );

			Vector3 newPosition = Controller.Position;
			Vector3 hullNormal = Controller.GetPlayerMaxs( false ) - Controller.GetPlayerMins( false );
			Vector3 hullDuck = Controller.GetPlayerMaxs( true ) - Controller.GetPlayerMins( true );
			Vector3 hullDelta = hullNormal - hullDuck;

			newPosition += hullDelta;
			Controller.Position = newPosition;

			Controller.UpdateBBox();
			FixPlayerCrouchStuck( true );
			Controller.CategorizePosition( Controller.GroundEntity != null );
		}

		public bool CanUnDuckJump( ref TraceResult trace )
		{
			Vector3 vecEnd = Controller.Position;
			vecEnd.z -= 36.0f;
			trace = Controller.TraceBBox( Controller.Position, vecEnd );

			if ( trace.Fraction < 1.0f )
			{
				vecEnd.z = Controller.Position.z + (-36.0f * trace.Fraction);

				TraceResult traceUp;
				traceUp = Controller.TraceBBox( vecEnd, vecEnd, Controller.GetPlayerMins( false ), Controller.GetPlayerMaxs( false ) );

				if ( !traceUp.StartedSolid )
					return true;
			}
			else if ( trace.Fraction == 1.0f )
				return true;

			return false;
		}

		public override void Simulate()
		{
			bool inAir = Controller.GroundEntity == null;
			bool inDuck = IsDucked;
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
					else if ( IsDucking && inDuck && Input.Pressed( InputButton.Duck ) )
					{
						InvertDuckTime( inDuck );
					}

					if ( IsDucking && !duckJump && !duckJumpTime )
					{
						float duckTime = MathF.Max( 0.0f, DuckingTime - DuckTimer );

						if ( (duckTime > TimeToDuck) || (inDuck && !IsDucking) || inAir )
						{
							FinishDuck();
						}
						else
						{
							float duckFrac = SimpleSpline( duckTime / TimeToDuck );

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
								TraceResult trace = Controller.TraceBBox( Controller.Position, Controller.Position );

								if ( CanUnDuckJump( ref trace ) )
								{
									FinishUnDuckJump( trace );
									DuckJumpTimer = (TimeToUnDuck * (1.0f - trace.Fraction)) + TimeToUnDuckInv;
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
							TraceResult trace = Controller.TraceBBox( Controller.Position, Controller.Position );

							if ( CanUnDuckJump( ref trace ) )
							{
								FinishUnDuckJump( trace );

								if ( trace.Fraction < 1.0f )
								{
									DuckJumpTimer = (TimeToUnDuck * (1.0f - trace.Fraction)) + TimeToUnDuckInv;
								}
							}
						}
					}

					if ( duckJumpTime )
						return;

					if ( true || inAir || IsDucking )
					{
						if ( Input.Released( InputButton.Duck ) && !IsDucking && IsDucked )
						{
							DuckTimer = DuckingTime;
						}
						else if ( IsDucking && !IsDucked && Input.Released( InputButton.Duck ) )
						{
							InvertDuckTime( inDuck );
						}
					}

					if ( CanUnDuck() )
					{
						if ( IsDucking || IsDucked )
						{
							float duckTime = MathF.Max( 0.0f, DuckingTime - DuckTimer );

							if ( duckTime > TimeToUnDuck || (inAir && !duckJump) )
							{
								FinishUnDuck();
							}
							else
							{
								float duckFrac = SimpleSpline( 1.0f - (duckTime / TimeToUnDuck) );

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
							IsDucking = false;
						}
					}
				}
			}

			if ( IsDucking || IsDucked )
			{
				Controller.SetTag( "ducked" );
			}
		}
	}
}

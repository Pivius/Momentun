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
		public float DuckTime { get; set; }
		public float DuckJumpTime { get; set; }
		public float JumpTime { get; set; }
		public float DUCK_TIME { get; set; } = 1000.0f;
		public float JUMP_TIME { get; set; } = 600.0f;
		public float TIME_TO_UNDUCK { get; set; } = 0.2f;
		public float TIME_TO_DUCK { get; set; } = 0.4f;
		public float TIME_TO_UNDUCK_INV { get; set; } = 1000.0f - 200.0f;

		public Duck( BaseController controller )
		{
			Controller = controller;
		}

		private static float SimpleSpline( float value )
		{
			float value_squared = value * value;

			return (3 * value_squared - 2 * value_squared * value);
		}

		public virtual Vector3 GetUnDuckOrigin( bool negate )
		{
			Vector3 new_origin = Controller.Position;

			if ( Controller.OnGround() )
				new_origin += (Controller.GetPlayerMins( true ) - Controller.GetPlayerMins( false ));
			else
			{
				Vector3 hull_normal = Controller.GetPlayerMaxs( false ) - Controller.GetPlayerMins( false );
				Vector3 hull_duck = Controller.GetPlayerMaxs( true ) - Controller.GetPlayerMins( true );
				Vector3 view_delta = (hull_normal - hull_duck);

				if ( negate )
					view_delta *= -1;

				new_origin += view_delta;
			}

			return new_origin;
		}

		public void FixPlayerCrouchStuck(bool upward)
		{
			int i;
			Vector3 cached_pos;
			TraceResult trace;
			int direction = upward ? 1 : 0;

			trace = TraceUtil.PlayerBBox(Controller.Position, Controller.Position, Controller);

			if ( !trace.StartedSolid )
				return;

			cached_pos = Controller.Position;

			for ( i = 0; i < 36; i++ )
			{
				Vector3 org = Controller.Position;

				org = org.WithZ( org.z + direction );
				Controller.Position = org;
				trace = TraceUtil.PlayerBBox( Controller.Position, Controller.Position, Controller );

				if ( !trace.StartedSolid )
					return;
			}

			Controller.Position = cached_pos;
		}

		public bool CanUnDuck()
		{
			Vector3 new_origin = GetUnDuckOrigin( true );
			TraceResult trace;
			bool saved_duck = IsDucked;

			IsDucked = false;
			trace = TraceUtil.PlayerBBox( Controller.Position, new_origin, Controller );
			IsDucked = saved_duck;

			if ( trace.StartedSolid || trace.Fraction != 1.0f )
				return false;

			return true;
		}

		public void FinishUnDuck()
		{
			Vector3 new_origin = GetUnDuckOrigin( true );

			IsDucked = false;
			IsDucking = false;
			Controller.GetPlayer().RemoveFlag( PlayerFlags.DUCKING );
			InDuckJump = false;
			Controller.ViewOffset = Controller.GetPlayerViewOffset( false );
			Controller.Position = new_origin;
			DuckTime = 0f;
			Controller.CategorizePosition( Controller.OnGround() );
		}

		public void SetDuckedEyeOffset( float frac )
		{
			Vector3 duck_mins = Controller.GetPlayerMins( true );
			Vector3 stand_mins = Controller.GetPlayerMins( false );
			float more = duck_mins.z - stand_mins.z;
			float duck_view = Controller.GetPlayerViewOffset( true );
			float stand_view = Controller.GetPlayerViewOffset( false );
			float view_offset = ((duck_view - more) * frac) + (stand_view * (1 - frac));

			Controller.ViewOffset = view_offset;
		}

		public void UpdateDuckJumpEyeOffset()
		{
			if (DuckJumpTime != 0.0f)
			{
				float duck_ms = MathF.Max( 0.0f, DUCK_TIME - DuckJumpTime );
				float duck_s = duck_ms / DUCK_TIME;

				if (duck_s > TIME_TO_UNDUCK)
				{
					DuckJumpTime = 0.0f;
					SetDuckedEyeOffset( 0.0f );
				}
				else
				{
					float duck_frac = SimpleSpline( 1.0f - (duck_s / TIME_TO_UNDUCK) );
					SetDuckedEyeOffset( duck_frac );
				}
			}
		}

		public void FinishUnDuckJump(ref TraceResult trace)
		{
			Vector3 hull_normal = Controller.GetPlayerMaxs( false ) - Controller.GetPlayerMins( false );
			Vector3 hull_duck = Controller.GetPlayerMaxs( true ) - Controller.GetPlayerMins( true );
			Vector3 hull_delta = (hull_normal - hull_duck); //* -1;
			Vector3 new_origin = Controller.Position;

			float delta_z = hull_delta.z;
			delta_z *= trace.Fraction;
			delta_z -= hull_delta.z;

			Controller.GetPlayer().RemoveFlag( PlayerFlags.DUCKING );
			IsDucked = false;
			IsDucking = false;
			InDuckJump = false;
			DuckTime = 0.0f;
			DuckJumpTime = 0.0f;
			JumpTime = 0.0f;

			float view_offset = Controller.GetPlayerViewOffset( false );
			view_offset -= delta_z;
			Controller.ViewOffset = view_offset;

			new_origin = new_origin.WithZ( new_origin.z - delta_z );
			Controller.Position = new_origin;
			Controller.CategorizePosition( Controller.OnGround() );
		}

		public void FinishDuck()
		{
			if ( Controller.GetPlayer().GetFlag( PlayerFlags.DUCKING ) )
				return;

			Controller.GetPlayer().AddFlag( PlayerFlags.DUCKING );
			IsDucked = true;
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
			IsDucking = false;

			Controller.ViewOffset = Controller.GetPlayerViewOffset( true );

			Vector3 new_origin = Controller.Position;
			Vector3 hull_normal = Controller.GetPlayerMaxs( false ) - Controller.GetPlayerMins( false );
			Vector3 hull_duck = Controller.GetPlayerMaxs( true ) - Controller.GetPlayerMins( true );
			Vector3 view_delta = (hull_normal - hull_duck);

			new_origin += view_delta;
			Controller.Position = new_origin;

			FixPlayerCrouchStuck( true );
			Controller.CategorizePosition( Controller.OnGround() );
		}
		
		public bool CanUnDuckJump(ref TraceResult trace)
		{
			Vector3 vec_end = Controller.Position;
			vec_end = vec_end.WithZ( vec_end.z - 36.0f );
			trace = TraceUtil.PlayerBBox( Controller.Position, vec_end, Controller );

			if (trace.Fraction < 1.0f)
			{
				vec_end = vec_end.WithZ( Controller.Position.z + (-36.0f * trace.Fraction) );

				TraceResult trace_up;
				bool WasDucked = IsDucked;
				IsDucked = false;
				trace_up = TraceUtil.PlayerBBox( vec_end, vec_end, Controller );
				IsDucked = WasDucked;

				if ( !trace_up.StartedSolid )
					return true;
			}

			return false;
		}

		public void Move()
		{
			bool bInAir = !Controller.OnGround();
			bool bInDuck = Controller.GetPlayer().GetFlag( PlayerFlags.DUCKING );
			bool bDuckJump = JumpTime > 0.0f;
			bool bDuckJumpTime = DuckJumpTime > 0.0f;
			bool DuckButton = Input.Down( InputButton.Duck );

			if ( DuckButton || IsDucking || bInDuck || bDuckJump)
			{
				if (DuckButton || bDuckJump)
				{
					if ( Input.Pressed( InputButton.Duck ) && !bInDuck && !bDuckJumpTime)
					{
						DuckTime = DUCK_TIME;
						IsDucking = true;
					}

					if (IsDucking && !bDuckJump && !bDuckJumpTime)
					{
						float duck_ms = MathF.Max( 0.0f, DUCK_TIME - DuckTime );
						float duck_s = duck_ms * 0.001f;

						if ((duck_s > TIME_TO_DUCK) || bInDuck || bInAir)
						{
							FinishDuck();
						}
						else
						{
							float duck_frac = SimpleSpline( duck_s / TIME_TO_DUCK );
							SetDuckedEyeOffset( duck_frac );
						}
					}

					if (bDuckJump)
					{
						if (!bInDuck )
						{
							if (DuckButton)
								StartUnDuckJump();
						}
						else
						{
							if (!DuckButton)
							{
								TraceResult trace = TraceUtil.PlayerBBox( Controller.Position, Controller.Position, Controller );

								if ( CanUnDuckJump( ref trace ) )
								{
									FinishUnDuckJump( ref trace );
									DuckJumpTime = ((TIME_TO_UNDUCK * 1000f) * (1.0f - trace.Fraction)) + TIME_TO_UNDUCK_INV;
								}
							}
						}
					}
				}
				else
				{
					if (InDuckJump)
					{
						if (!DuckButton)
						{
							TraceResult trace = TraceUtil.PlayerBBox( Controller.Position, Controller.Position, Controller );

							if (CanUnDuckJump(ref trace))
							{
								FinishUnDuckJump( ref trace );

								if (trace.Fraction < 1.0f)
								{
									DuckJumpTime = ((TIME_TO_UNDUCK * 1000f) * (1.0f - trace.Fraction)) + TIME_TO_UNDUCK_INV;
								}
							}
						}
						else
						{
							InDuckJump = false;
						}
					}

					if ( bDuckJumpTime )
						return;

					if ( (bool)Controller.MoveProp["AllowAutoMovement"] || bInAir || IsDucking)
					{
						if ( Input.Released( InputButton.Duck ) )
						{
							DuckTime = DUCK_TIME;
						}
						else if (IsDucking && !IsDucked)
						{
							float unduck_ms = 1000.0f * TIME_TO_UNDUCK;
							float duck_ms = 1000.0f * TIME_TO_DUCK;
							float elapsed_ms = DUCK_TIME - DuckTime;

							float frac_ducked = elapsed_ms / duck_ms;
							float remaining_unduck_ms = frac_ducked * unduck_ms;

							DuckTime = DUCK_TIME - unduck_ms + remaining_unduck_ms;
						}
					}

					if (CanUnDuck())
					{
						if (IsDucking || IsDucked)
						{
							float duck_ms = MathF.Max( 0.0f, DUCK_TIME - DuckTime );
							float duck_s = duck_ms * 0.001f;

							if (duck_s > TIME_TO_UNDUCK || (bInAir && !bDuckJump))
							{
								FinishUnDuck();
							}
							else
							{
								float duck_frac = SimpleSpline( 1.0f - (duck_s / TIME_TO_UNDUCK) );
								SetDuckedEyeOffset( duck_frac );
								IsDucking = true;
							}
						}
					}
					else
					{
						if (DuckTime != DUCK_TIME)
						{
							SetDuckedEyeOffset( 1.0f );
							DuckTime = DUCK_TIME;
							IsDucked = true;
							IsDucking = false;
							Controller.GetPlayer().AddFlag( PlayerFlags.DUCKING );
						}
					}
				}
			}

			if (IsDucking || IsDucked)
			{
				Controller.SetTag( "ducked" );
			}
			//Log.Info( Controller.ViewOffset - Controller.GetPlayerViewOffset( false ) );
			if (DuckJumpTime == 0.0f && MathF.Abs( Controller.ViewOffset - Controller.GetPlayerViewOffset(false) ) > 0.1)
			{
				//SetDuckedEyeOffset( 0.0f );
			}
		}

		public void ReduceTimers()
		{
			float frame_msec = 1000.0f * Time.Delta;

			if ( DuckTime > 0 )
			{
				DuckTime -= frame_msec;
				if ( DuckTime < 0 )
				{
					DuckTime = 0;
				}
			}
			if ( DuckJumpTime > 0 )
			{
				DuckJumpTime -= frame_msec;
				if ( DuckJumpTime < 0 )
				{
					DuckJumpTime = 0;
				}
			}
			if ( JumpTime > 0 )
			{
				JumpTime -= frame_msec;
				if ( JumpTime < 0 )
				{
					JumpTime = 0;
				}
			}
		}
	}
}

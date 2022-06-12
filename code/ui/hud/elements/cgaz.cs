using Sandbox;
using Sandbox.UI;
using System;

namespace Momentum
{
	public partial class CGaz : Elements
	{
		private float MoveAngle;
		private float PrevSpeedSqr;
		private float SpeedSqr;
		private float WishSpeed;
		private float Accelerate;
		private float AccelerateSqr;
		private float PrevSpeed;
		private float Speed;
		private float DrawMin;
		private float DrawOpt;
		private float DrawMaxCos;
		private float DrawMax;
		private bool DrawVel;
		private float BarOptWidth = 0;
		private float BarOptPosition = 0;
		private float BarMaxWidth = 0;
		private float BarMaxPosition = 0;
		private float BarMaxCosWidth = 0;
		private float BarMaxCosPosition = 0;
		private bool MovingBackwards = false;
		private Vector3 PrevVelocity;

		private Panel MaxCos;
		private Panel Opt;
		private Panel Max;
		private Panel CenterPin;

		public CGaz()
		{
			SetStyleSheet( "/ui/hud/elements/cgaz.scss" );

			MaxCos = Add.Panel( "rad" );
			Opt = Add.Panel( "opt" );
			Max = Add.Panel( "max" );
			CenterPin = Add.Panel( "center" );
		}

		private void UpdateDraw( Vector3 velocity, float wishspeed, float accelerate )
		{
			//gSquared = GetSlickGravity();
			PrevSpeedSqr = PrevVelocity.WithZ( 0 ).LengthSquared;
			SpeedSqr = velocity.WithZ( 0 ).LengthSquared;
			WishSpeed = wishspeed;
			Accelerate = accelerate * WishSpeed * Time.Delta;
			AccelerateSqr = MathF.Pow( Accelerate, 2 );

			PrevSpeed = MathF.Sqrt( PrevSpeedSqr );
			Speed = MathF.Sqrt( SpeedSqr );

			DrawMin = UpdateDrawMin();
			DrawOpt = UpdateDrawOpt();
			DrawMaxCos = UpdateDrawMaxCos( DrawOpt );
			DrawMax = UpdateDrawMax( DrawMaxCos );

			//DrawVel = MathF.Atan2( velocity.y, velocity.x );
		}

		private float UpdateDrawMin()
		{
			float num_squared = WishSpeed * WishSpeed - PrevSpeedSqr + SpeedSqr; //+ gSquared;
			float num = MathF.Sqrt( num_squared );

			return num >= Speed ? 0 : MathF.Acos( num / Speed );
		}

		private float UpdateDrawOpt()
		{
			float num = WishSpeed - Accelerate;

			return num >= Speed ? 0 : MathF.Acos( num / Speed );
		}

		private float UpdateDrawMaxCos( float drawOpt )
		{
			float num = MathF.Sqrt( PrevSpeedSqr /*- gSquared*/ ) - Speed;
			float drawMaxCos = num >= Accelerate ? 0 : MathF.Acos( num / Accelerate );

			if ( drawMaxCos < drawOpt )
			{
				drawMaxCos = drawOpt;
			}

			return drawMaxCos;
		}

		private float UpdateDrawMax( float drawMaxCos )
		{
			float num = PrevSpeedSqr - SpeedSqr - AccelerateSqr; //- gSquared;
			float den = 2 * Accelerate * Speed;

			if ( num >= den )
			{
				return 0;
			}
			else if ( -num >= den )
			{
				return MathF.PI;
			}

			float drawMax = MathF.Acos( num / den );

			if ( drawMax < drawMaxCos )
			{
				drawMax = drawMaxCos;

				return drawMax;
			}

			return drawMax;
		}

		private (float min, float opt, float max, float maxCos) GetAngles( float yaw, float scale, bool negate = false )
		{
			if ( negate )
				scale *= -1f;

			var minAngle = (Rotation.FromYaw( MathX.RadianToDegree( DrawMin ) + MathX.RadianToDegree( -yaw ) ).Angle()) * scale;
			var optAngle = (Rotation.FromYaw( MathX.RadianToDegree( DrawOpt ) + MathX.RadianToDegree( -yaw ) ).Angle()) * scale;
			var maxAngle = (Rotation.FromYaw( MathX.RadianToDegree( DrawMax ) + MathX.RadianToDegree( -yaw ) ).Angle()) * scale;
			var maxCosAngle = (Rotation.FromYaw( MathX.RadianToDegree( DrawMaxCos ) + MathX.RadianToDegree( -yaw ) ).Angle()) * scale;

			return (minAngle, optAngle, maxAngle, maxCosAngle);
		}

		private void DrawStrafeAngles()
		{
			float scale = 5f;
			float minAngle = 0f;
			float optAngle = 0f;
			float maxAngle = 0f;
			float maxCosAngle = 0f;
			float duration = 0.25f;
			bool rightBtnDown = Input.Down( InputButton.Right );
			bool leftBtnDown = Input.Down( InputButton.Left );
			float barWidth = Style.Width.Value.Value;
			bool isBarRightSide = false;

			if ( (leftBtnDown || rightBtnDown) && !(leftBtnDown && rightBtnDown) )
			{
				var angles = leftBtnDown ? GetAngles( MoveAngle, scale ) : GetAngles( MoveAngle, scale, true );

				if ( MovingBackwards )
				{
					angles = (leftBtnDown && MovingBackwards) ? GetAngles( MoveAngle, scale, true ) : GetAngles( MoveAngle, scale, false );
				}

				DrawVel = true;
				isBarRightSide = (rightBtnDown && !MovingBackwards) || (leftBtnDown && MovingBackwards);
				minAngle = angles.min;
				optAngle = angles.opt;
				maxAngle = angles.max;
				maxCosAngle = angles.maxCos;

				if ( MathX.RadianToDegree( DrawMin ) < MathX.RadianToDegree( MoveAngle ) )
				{
					minAngle = -minAngle;
				}

				if ( MathX.RadianToDegree( DrawOpt ) < MathX.RadianToDegree( MoveAngle ) )
				{
					optAngle = -optAngle;
				}

				if ( MathX.RadianToDegree( DrawMax ) < MathX.RadianToDegree( MoveAngle ) )
				{
					maxAngle = -maxAngle;
				}

				if ( MathX.RadianToDegree( DrawMaxCos ) < MathX.RadianToDegree( MoveAngle ) )
				{
					maxCosAngle = -maxCosAngle;
				}

				BarOptPosition = InterpFunctions.Linear(
					BarOptPosition,
					(barWidth / 2 - (isBarRightSide ? minAngle : optAngle)) - BarOptPosition,
					Time.Delta,
					duration );

				BarOptWidth = InterpFunctions.Linear(
					BarOptWidth,
					(barWidth / 2 - (isBarRightSide ? optAngle : minAngle)) - (barWidth / 2 - (isBarRightSide ? minAngle : optAngle)) - BarOptWidth,
					Time.Delta,
					duration );

				BarMaxPosition = InterpFunctions.Linear(
					BarMaxPosition, (barWidth / 2 - (isBarRightSide ? optAngle : maxCosAngle)) - BarMaxPosition,
					Time.Delta,
					duration );

				BarMaxWidth = InterpFunctions.Linear( BarMaxWidth,
					(barWidth / 2 - (isBarRightSide ? maxCosAngle : optAngle)) - (barWidth / 2 - (isBarRightSide ? optAngle : maxCosAngle)) - BarMaxWidth,
					Time.Delta,
					duration );

				BarMaxCosPosition = InterpFunctions.Linear(
					BarMaxCosPosition,
					(barWidth / 2 - (isBarRightSide ? maxCosAngle : maxAngle)) - BarMaxCosPosition,
					Time.Delta,
					duration );

				BarMaxCosWidth = InterpFunctions.Linear(
					BarMaxCosWidth,
					(barWidth / 2 - (isBarRightSide ? maxAngle : maxCosAngle)) - (barWidth / 2 - (isBarRightSide ? maxCosAngle : maxAngle)) - BarMaxCosWidth,
					Time.Delta,
					duration );
			}
			else
			{
				DrawVel = false;
			}
		}

		public override void Tick()
		{
			MomentumPlayer player = Local.Pawn as MomentumPlayer;

			if ( player == null ) return;

			Vector3 velocity = player.Velocity;
			float barWidth = 500f;
			float maxSpeed = (float)player.MovementProps["MaxSpeed"];
			float accelerate = (float)player.MovementProps["StrafeAcceleration"];
			Vector3 eyeAngles = player.EyeRotation.Right;

			eyeAngles = new Vector3( -eyeAngles.y, eyeAngles.x );

			if (Input.Forward == 0 && Input.Left != 0)
			{
				maxSpeed = (float)player.MovementProps["SideStrafeMaxSpeed"];

				if (((MomentumController)player.Controller).IsSurfing)
				{
					maxSpeed = (float)player.MovementProps["MaxSpeed"]/10f;
					accelerate = (float)player.MovementProps["SurfAccelerate"];
				}
			}

			if ( player.Velocity.WithZ(0).Dot( eyeAngles ) < 0)
			{
				MovingBackwards = true;
			}
			else
			{
				MovingBackwards = false;
			}

			Style.Width = barWidth;

			
			MoveAngle = MathX.DegreeToRadian( velocity.WithZ( 0 ).Angle( player.Controller.WishVelocity.WithZ( 0 ).Normal ) );
			UpdateDraw( velocity, maxSpeed, accelerate );
			DrawStrafeAngles();

			//var length = 100;

			//DebugOverlay.Line( player.Position, player.Position + player.Controller.WishVelocity.Normal * length, Color.Red, 0, false );
			//DebugOverlay.Line( player.Position, player.Position + Rotation.FromYaw( MathX.RadianToDegree( drawMin ) + MathX.RadianToDegree( drawVel ) ).Forward * length, Color.Green, 0, false );
			//DebugOverlay.Line( player.Position, player.Position + Rotation.FromYaw( MathX.RadianToDegree( drawOpt ) + MathX.RadianToDegree( drawVel ) ).Forward * length, Color.Blue, 0, false );
			//DebugOverlay.Line( player.Position, player.Position + Rotation.FromYaw( MathX.RadianToDegree( drawMax ) + MathX.RadianToDegree( drawVel ) ).Forward * length, Color.Yellow, 0, false );
			//DebugOverlay.Line( player.Position, player.Position + Rotation.FromYaw( MathX.RadianToDegree( drawMaxCos ) + MathX.RadianToDegree( drawVel ) ).Forward * length, Color.Cyan, 0, false );
			//DebugOverlay.Line( player.Position, player.Position + Rotation.FromYaw( MathX.RadianToDegree( drawVel ) ).Forward * length, Color.White, 0, false );

			//DebugOverlay.Line( player.Position, player.Position + Rotation.FromYaw( MathX.RadianToDegree( -drawMin ) + MathX.RadianToDegree( drawVel ) ).Forward * length, Color.Green, 0, false );
			//DebugOverlay.Line( player.Position, player.Position + Rotation.FromYaw( MathX.RadianToDegree( -drawOpt ) + MathX.RadianToDegree( drawVel ) ).Forward * length, Color.Blue, 0, false );
			//DebugOverlay.Line( player.Position, player.Position + Rotation.FromYaw( MathX.RadianToDegree( -drawMax ) + MathX.RadianToDegree( drawVel ) ).Forward * length, Color.Yellow, 0, false );
			//DebugOverlay.Line( player.Position, player.Position + Rotation.FromYaw( MathX.RadianToDegree( -drawMaxCos ) + MathX.RadianToDegree( drawVel ) ).Forward * length, Color.Cyan, 0, false );

			if ( !DrawVel )
			{
				var duration = 0.1f;

				BarOptPosition = InterpFunctions.Linear(
					BarOptPosition,
					(barWidth / 2) - BarOptPosition,
					Time.Delta,
					duration * 0.5f );

				BarOptWidth = InterpFunctions.Linear(
					BarOptWidth,
					0 - BarOptWidth,
					Time.Delta,
					duration * 0.5f );

				BarMaxPosition = InterpFunctions.Linear(
					BarMaxPosition,
					(barWidth / 2) - BarMaxPosition,
					Time.Delta,
					duration * 0.5f );
				BarMaxWidth = InterpFunctions.Linear(
					BarMaxWidth,
					0 - BarMaxWidth,
					Time.Delta,
					duration * 0.5f );

				BarMaxCosPosition = InterpFunctions.Linear(
					BarMaxCosPosition,
					(barWidth / 2) - BarMaxCosPosition,
					Time.Delta,
					duration * 0.5f );
				BarMaxCosWidth = InterpFunctions.Linear(
					BarMaxCosWidth,
					0 - BarMaxCosWidth,
					Time.Delta,
					duration * 0.5f );

				Opt.Style.Left = BarOptPosition;
				Opt.Style.Width = BarOptWidth;

				Max.Style.Left = BarMaxPosition;
				Max.Style.Width = BarMaxWidth;

				MaxCos.Style.Left = BarMaxCosPosition;
				MaxCos.Style.Width = BarMaxCosWidth;
			}

			Opt.Style.Left = BarOptPosition;
			Opt.Style.Width = BarOptWidth;

			Max.Style.Left = BarMaxPosition;
			Max.Style.Width = BarMaxWidth;

			MaxCos.Style.Left = BarMaxCosPosition;
			MaxCos.Style.Width = BarMaxCosWidth;

			PrevVelocity = player.Velocity;
		}
	}
}

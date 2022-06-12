using Sandbox;
using Sandbox.UI;
using System;

namespace Momentum
{
	public partial class CGaz : Elements
	{
		private float EyeYaw;
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
		private float BarOptWidth = 0;
		private float BarOptPosition = 0;
		private float BarMaxWidth = 0;
		private float BarMaxPosition = 0;
		private float BarMaxCosWidth = 0;
		private float BarMaxCosPosition = 0;
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

			//drawVel = MathF.Atan2( player.Velocity.y, player.Velocity.x );
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
		private void DrawHalfStrafe()
		{
			bool drawVel = false;
			float scale = 5f;
			float minAngle = 0f;
			float optAngle = 0f;
			float maxAngle = 0f;
			float maxCosAngle = 0f;
			float duration = 0.25f;
			bool rightBtnDown = Input.Down( InputButton.Right );
			bool leftBtnDown = Input.Down( InputButton.Left );
			float barWidth = Style.Width.Value.Value;

			if ( (Input.Down( InputButton.Forward ) || Input.Down( InputButton.Back )) && (leftBtnDown || rightBtnDown) )
			{
				var angles = leftBtnDown ? GetAngles( EyeYaw, scale ) : GetAngles( EyeYaw, scale, true );

				drawVel = true;
				minAngle = angles.min;
				optAngle = angles.opt;
				maxAngle = angles.max;
				maxCosAngle = angles.maxCos;
			}

			if ( MathX.RadianToDegree( DrawMin ) < MathX.RadianToDegree( EyeYaw ) )
			{
				minAngle = -minAngle;
			}

			if ( MathX.RadianToDegree( DrawOpt ) < MathX.RadianToDegree( EyeYaw ) )
			{
				optAngle = -optAngle;
			}

			if ( MathX.RadianToDegree( DrawMax ) < MathX.RadianToDegree( EyeYaw ) )
			{
				maxAngle = -maxAngle;
			}

			if ( MathX.RadianToDegree( DrawMaxCos ) < MathX.RadianToDegree( EyeYaw ) )
			{
				maxCosAngle = -maxCosAngle;
			}

			BarOptPosition = InterpFunctions.Linear(
				BarOptPosition,
				(barWidth / 2 - (rightBtnDown ? minAngle : optAngle)) - BarOptPosition,
				Time.Delta,
				duration );

			BarOptWidth = InterpFunctions.Linear(
				BarOptWidth,
				(barWidth / 2 - (rightBtnDown ? optAngle : minAngle)) - (barWidth / 2 - (rightBtnDown ? minAngle : optAngle)) - BarOptWidth,
				Time.Delta,
				duration );

			BarMaxPosition = InterpFunctions.Linear(
				BarMaxPosition, (barWidth / 2 - (rightBtnDown ? optAngle : maxCosAngle)) - BarMaxPosition,
				Time.Delta,
				duration );

			BarMaxWidth = InterpFunctions.Linear( BarMaxWidth,
				(barWidth / 2 - (rightBtnDown ? maxCosAngle : optAngle)) - (barWidth / 2 - (rightBtnDown ? optAngle : maxCosAngle)) - BarMaxWidth,
				Time.Delta,
				duration );

			BarMaxCosPosition = InterpFunctions.Linear(
				BarMaxCosPosition,
				(barWidth / 2 - (rightBtnDown ? maxCosAngle : maxAngle)) - BarMaxCosPosition,
				Time.Delta,
				duration );

			BarMaxCosWidth = InterpFunctions.Linear(
				BarMaxCosWidth,
				(barWidth / 2 - (rightBtnDown ? maxAngle : maxCosAngle)) - (barWidth / 2 - (rightBtnDown ? maxCosAngle : maxAngle)) - BarMaxCosWidth,
				Time.Delta,
				duration );


			if ( !drawVel )
			{
				BarOptPosition = barWidth / 2;
				BarOptWidth = 0;

				BarMaxPosition = barWidth / 2;
				BarMaxWidth = 0;

				BarMaxCosPosition = barWidth / 2;
				BarMaxCosWidth = 0;

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
		}

		public override void Tick()
		{
			var player = (MomentumPlayer)Local.Pawn;

			if ( player == null ) return;

			Vector3 velocity = player.Velocity;
			float barWidth = 500f;

			EyeYaw = MathX.DegreeToRadian( velocity.WithZ( 0 ).Angle( player.Controller.WishVelocity.WithZ( 0 ).Normal ) );
			UpdateDraw( velocity, (float)player.MovementProps["MaxSpeed"], (float)player.MovementProps["StrafeAcceleration"] );
			Style.Width = barWidth;

			DrawHalfStrafe();

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


			PrevVelocity = player.Velocity;
		}
	}
}

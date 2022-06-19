using Sandbox;
using Sandbox.UI;
using System;

namespace Momentum
{
	public partial class CGaz : Elements
	{
		private float MoveAngle;
		private float ControlMaxCos;
		private float ControlOpt;
		private float StrafeMin;
		private float StrafeOpt;
		private float StrafeMaxCos;
		private float StrafeMax;
		private bool DrawCGaz;
		private float BarOptWidth = 0;
		private float BarOptPosition = 0;
		private float BarMaxWidth = 0;
		private float BarMaxPosition = 0;
		private float BarMaxCosWidth = 0;
		private float BarMaxCosPosition = 0;
		private bool MovingBackwards = false;
		private Vector3 PrevVelocity;

		private readonly Panel MainPanel;
		private readonly Panel MaxCos;
		private readonly Panel Opt;
		private readonly Panel Max;
		private readonly Panel CenterPin;

		public CGaz()
		{
			SetStyleSheet( "/ui/hud/elements/cgaz.scss" );

			MainPanel = Add.Panel( "backpanel" );
			MaxCos = MainPanel.Add.Panel( "rad" );
			Opt = MainPanel.Add.Panel( "opt" );
			Max = MainPanel.Add.Panel( "max" );
			CenterPin = Add.Panel( "center" );
		}

		private void UpdateDraw( MomentumPlayer player, float wishSpeed, float accelerate )
		{
			float prevSpeedSqr = PrevVelocity.WithZ( 0 ).LengthSquared;
			float speedSqr = player.Velocity.WithZ( 0 ).LengthSquared;
			float airControlAccel = 32f * player.Properties.AirControl * Time.Delta;

			accelerate *= wishSpeed * Time.Delta;

			float accelerateSqr = MathF.Pow( accelerate, 2 );
			float prevSpeed = MathF.Sqrt( prevSpeedSqr );
			float speed = MathF.Sqrt( speedSqr );

			float deltaSpeed = prevSpeed - speed;
			float deltaSpeedSqr = prevSpeedSqr - speedSqr;

			ControlOpt = Strafe.GetStrafeOpt( wishSpeed, speed, airControlAccel );
			ControlMaxCos = Strafe.GetStrafeMaxCos( ControlOpt, deltaSpeed, airControlAccel );
			StrafeMin = Strafe.GetStrafeMin( wishSpeed, speed, prevSpeedSqr, speedSqr );
			StrafeOpt = Strafe.GetStrafeOpt( wishSpeed, speed, accelerate );
			StrafeMaxCos = Strafe.GetStrafeMaxCos( StrafeOpt, deltaSpeed, accelerate );
			StrafeMax = Strafe.GetStrafeMax( StrafeMaxCos, deltaSpeedSqr, accelerateSqr, accelerate, speed );
		}

		private static void SetMaxSpeed( ref float maxSpeed, ref float accelerate, Properties props, bool isSurfing )
		{
			if ( Input.Forward == 0 && Input.Left != 0 )
			{
				maxSpeed = props.SideStrafeMaxSpeed;

				if ( isSurfing )
				{
					maxSpeed = props.MaxSpeed / 10f;
					accelerate = props.SurfAccelerate;
				}
			}
		}

		private void DrawStrafeAngles()
		{
			float scale = 25f;
			float duration = 0.25f;
			bool rightBtnDown = Input.Down( InputButton.Right );
			bool leftBtnDown = Input.Down( InputButton.Left );

			if ( Input.Down( InputButton.Forward ) || Input.Down( InputButton.Back ) )
				scale = 5;

			if ( (leftBtnDown || rightBtnDown) && !(leftBtnDown && rightBtnDown) )
			{
				bool isBarRightSide = (rightBtnDown && !MovingBackwards) || (leftBtnDown && MovingBackwards);
				float barWidth = MainPanel.Style.Width.Value.Value;
				float moveAngle = MoveAngle;
				float minAngle = Strafe.AlignWithDir( StrafeMin, moveAngle, scale, isBarRightSide );
				float optAngle = Strafe.AlignWithDir( StrafeOpt, moveAngle, scale, isBarRightSide );
				float maxAngle = Strafe.AlignWithDir( StrafeMax, moveAngle, scale, isBarRightSide );
				float maxCosAngle = Strafe.AlignWithDir( StrafeMaxCos, moveAngle, scale, isBarRightSide );

				DrawCGaz = true;

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
				DrawCGaz = false;
			}
		}

		private void DrawControlAngles()
		{
			float scale = 5f;
			float duration = 0.25f;
			bool forwardBtnDown = Input.Down( InputButton.Forward );
			bool shouldDraw = DrawCGaz;

			if ( forwardBtnDown
				&& !Input.Down( InputButton.Back )
				&& !(Input.Down( InputButton.Right ) || Input.Down( InputButton.Left ))
				&& !MovingBackwards )
			{
				float barWidth = MainPanel.Style.Width.Value.Value;
				float moveAngle = MoveAngle;
				float controlOptAngle = Strafe.AlignWithDir( ControlOpt, moveAngle, scale, false );
				float controlMaxAngle = Strafe.AlignWithDir( ControlMaxCos, moveAngle, scale, false );

				DrawCGaz = true;

				BarMaxPosition = InterpFunctions.Linear(
					BarMaxPosition, (barWidth / 2 - controlOptAngle) - BarMaxPosition,
					Time.Delta,
					duration );

				BarMaxWidth = InterpFunctions.Linear( BarMaxWidth,
					(barWidth / 2) - (barWidth / 2 - controlOptAngle * 2) - BarMaxWidth,
					Time.Delta,
					duration );

				BarMaxCosPosition = InterpFunctions.Linear(
					BarMaxCosPosition,
					(barWidth / 2 - controlMaxAngle) - BarMaxCosPosition,
					Time.Delta,
					duration );

				BarMaxCosWidth = InterpFunctions.Linear(
					BarMaxCosWidth,
					(barWidth / 2) - (barWidth / 2 - controlMaxAngle * 2) - BarMaxCosWidth,
					Time.Delta,
					duration );
			}
			else
			{
				DrawCGaz = shouldDraw;
			}
		}

		public override void Tick()
		{
			if ( Local.Pawn is not MomentumPlayer player ) return;

			Vector3 velocity = player.Velocity;
			float barWidth = 500f;
			float maxSpeed = player.Properties.MaxSpeed;
			float accelerate = player.Properties.StrafeAcceleration;
			Vector3 eyeAngles = player.EyeRotation.Right;
			var controller = player.Controller as MomentumController;

			eyeAngles = new Vector3( -eyeAngles.y, eyeAngles.x );

			if ( player.Velocity.WithZ( 0 ).Dot( eyeAngles ) < 0 )
			{
				MovingBackwards = true;
			}
			else
			{
				MovingBackwards = false;
			}

			MainPanel.Style.Width = barWidth - 4;
			MainPanel.Style.Right = 1;
			Style.Width = barWidth;
			MoveAngle = MathX.DegreeToRadian( velocity.WithZ( 0 ).Angle( controller.WishVelocity.WithZ( 0 ).Normal ) );

			SetMaxSpeed( ref maxSpeed, ref accelerate, player.Properties, controller.IsSurfing );
			UpdateDraw( player, maxSpeed, accelerate );
			DrawStrafeAngles();
			DrawControlAngles();
			//var length = 100;

			//DebugOverlay.Line( player.Position, player.Position + player.Controller.WishVelocity.Normal * length, Color.Red, 0, false );
			//DebugOverlay.Line( player.Position, player.Position + Rotation.FromYaw( MathX.RadianToDegree(UpdateDrawControl( player.Properties.AirControl )) + velocity.EulerAngles.yaw ).Forward * length, Color.Magenta, 0, false );
			//DebugOverlay.Line( player.Position, player.Position + Rotation.FromYaw( MathX.RadianToDegree( DrawMin ) + velocity.EulerAngles.yaw ).Forward * length, Color.Green, 0, false );
			//DebugOverlay.Line( player.Position, player.Position + Rotation.FromYaw( MathX.RadianToDegree( DrawOpt ) + velocity.EulerAngles.yaw ).Forward * length, Color.Blue, 0, false );
			//DebugOverlay.Line( player.Position, player.Position + Rotation.FromYaw( MathX.RadianToDegree( DrawMax ) + velocity.EulerAngles.yaw ).Forward * length, Color.Yellow, 0, false );
			//DebugOverlay.Line( player.Position, player.Position + Rotation.FromYaw( MathX.RadianToDegree( DrawMaxCos ) + velocity.EulerAngles.yaw ).Forward * length, Color.Cyan, 0, false );
			//DebugOverlay.Line( player.Position, player.Position + player.Controller.Velocity.Normal * length, Color.White, 0, false );

			//DebugOverlay.Line( player.Position, player.Position + Rotation.FromYaw( MathX.RadianToDegree( -DrawMin ) + velocity.EulerAngles.yaw ).Forward * length, Color.Green, 0, false );
			//DebugOverlay.Line( player.Position, player.Position + Rotation.FromYaw( MathX.RadianToDegree( -DrawOpt ) + velocity.EulerAngles.yaw ).Forward * length, Color.Blue, 0, false );
			//DebugOverlay.Line( player.Position, player.Position + Rotation.FromYaw( MathX.RadianToDegree( -DrawMax ) + velocity.EulerAngles.yaw ).Forward * length, Color.Yellow, 0, false );
			//DebugOverlay.Line( player.Position, player.Position + Rotation.FromYaw( MathX.RadianToDegree( -DrawMaxCos ) + velocity.EulerAngles.yaw ).Forward * length, Color.Cyan, 0, false );

			if ( !DrawCGaz )
			{
				var duration = 0.1f;

				BarOptPosition = InterpFunctions.Linear(
					BarOptPosition,
					(barWidth / 2) - BarOptPosition,
					Time.Delta,
					duration );
				BarOptWidth = InterpFunctions.Linear(
					BarOptWidth,
					0 - BarOptWidth,
					Time.Delta,
					duration );

				BarMaxPosition = InterpFunctions.Linear(
					BarMaxPosition,
					(barWidth / 2) - BarMaxPosition,
					Time.Delta,
					duration );
				BarMaxWidth = InterpFunctions.Linear(
					BarMaxWidth,
					0 - BarMaxWidth,
					Time.Delta,
					duration );

				BarMaxCosPosition = InterpFunctions.Linear(
					BarMaxCosPosition,
					(barWidth / 2) - BarMaxCosPosition,
					Time.Delta,
					duration );
				BarMaxCosWidth = InterpFunctions.Linear(
					BarMaxCosWidth,
					0 - BarMaxCosWidth,
					Time.Delta,
					duration );

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

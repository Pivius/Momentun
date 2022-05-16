using Sandbox;
using System;
using System.Collections.Generic;

namespace Momentum
{
    public partial class MomentumPlayer : Player
    {
		public Property MovementProps = new(
			new Dictionary<string, object>
			{
				// # Movement Properties

				["MaxSpeed"] = 300.0f,
				["MaxMove"] = 10000.0f,
				["CanWalk"] = false,
				["CanRun"] = false,
				["DefaultSpeed"] = 250.0f,
				["RunSpeed"] = 250.0f,
				["WalkSpeed"] = 250.0f,
				["DuckedWalkSpeed"] = 150.0f,
				["SwimSpeed"] = 250.0f,
				["Gravity"] = 800.0f,
				["JumpPower"] = 270.0f, //268.3281572999747f * 1.2f,
				["StepSize"] = 16.0f,
				["StandableAngle"] = 45.0f,
				["FallDamageMultiplier"] = 0.0563f,
				["ClipTime"] = 0.5f,
				["AutoJump"] = true,
				["AllowAutoMovement"] = true,
				["MoveState"] = (STATE) 0,
				["DoubleJumpZ"] = 100f,
				["RampJump"] = 2,

				// # Accelerate Properties

				["CanAirStrafe"] = true,
				["AirAccelerate"] = 1.0f,
				["CanAccelerate"] = true,
				["Accelerate"] = 15.0f,
				["WaterAccelerate"] = 4.0f,


				// # Friction Properties
				["Friction"] = 8.0f,
				["WaterFriction"] = 1.0f,
				["StopSpeed"] = 100.0f,

				// # Quake Properties

				["SideStrafeMaxSpeed"] = 300.0f,
				["StrafeAcceleration"] = 1.0f,
				["AirStopAcceleration"] = 2.5f,
				["AirControl"] = 150.0f,
			}
		);

		public Property ViewProps = new(
			new Dictionary<string, object>
			{
				["StandViewOffset"] = 64.0f,
				["DuckViewOffset"] = 28.0f,
				["DeadViewOffset"] = 14.0f,

				// # Player Hulls

				["StandMins"] = new Vector3(-16, -16, 0),
				["StandMaxs"] = new Vector3(16, 16, 72),
				["DuckMins"] = new Vector3(-16, -16, 0),
				["DuckMaxs"] = new Vector3(16, 16, 32),
			}
		);
    }
}

using Sandbox;

namespace Momentum
{
	public partial class Properties : BaseNetworkable
	{
		// # Movement Properties

		[Net]
		public float MaxSpeed { get; set; } = 300f;
		[Net]
		public float MaxMove { get; set; } = 10000.0f;
		[Net]
		public bool CanWalk { get; set; } = false;
		[Net]
		public bool CanRun { get; set; } = false;
		[Net]
		public float DefaultSpeed { get; set; } = 250.0f;
		[Net]
		public float RunSpeed { get; set; } = 250.0f;
		[Net]
		public float WalkSpeed { get; set; } = 250.0f;
		[Net]
		public float DuckedWalkSpeed { get; set; } = 150.0f;
		[Net]
		public float SwimSpeed { get; set; } = 250.0f;
		[Net]
		public float Gravity { get; set; } = 600.0f;
		[Net]
		public float JumpPower { get; set; } = 270.0f;
		[Net]
		public float StepSize { get; set; } = 18.0f;
		[Net]
		public float StandableAngle { get; set; } = 44.0f;
		[Net]
		public float FallDamageMultiplier { get; set; } = 0.0563f;
		[Net]
		public float ClipTime { get; set; } = 0.5f;
		[Net]
		public bool AutoJump { get; set; } = true;
		[Net]
		public bool AllowAutoMovement { get; set; } = true;
		[Net]
		public STATE MoveState { get; set; } = STATE.GROUND;
		[Net]
		public float DoubleJumpZ { get; set; } = 250f;
		[Net]
		public int RampJump { get; set; } = 2;
		[Net]
		public bool CanOverBounce { get; set; } = true;

		// # Accelerate Properties

		[Net]
		public bool CanAirStrafe { get; set; } = true;
		[Net]
		public float AirAccelerate { get; set; } = 1.0f;
		[Net]
		public float SurfAccelerate { get; set; } = 500f;
		[Net]
		public bool CanAccelerate { get; set; } = true;
		[Net]
		public float Accelerate { get; set; } = 15.0f;
		[Net]
		public float WaterAccelerate { get; set; } = 4.0f;

		// # Friction Properties

		[Net]
		public float Friction { get; set; } = 8.0f;
		[Net]
		public float WaterFriction { get; set; } = 1.0f;
		[Net]
		public float StopSpeed { get; set; } = 100.0f;

		// # Quake Properties

		[Net]
		public float SideStrafeMaxSpeed { get; set; } = 30.0f;
		[Net]
		public float StrafeAcceleration { get; set; } = 70.0f;
		[Net]
		public float AirStopAcceleration { get; set; } = 2.5f;
		[Net]
		public float AirControl { get; set; } = 150.0f;

		// # Player View

		[Net]
		public float StandViewOffset { get; set; } = 64.0f;
		[Net]
		public float DuckViewOffset { get; set; } = 28.0f;
		[Net]
		public float DeadViewOffset { get; set; } = 14.0f;

		// # Player Hulls

		[Net]
		public Vector3 StandMins { get; set; } = new Vector3( -16, -16, 0 );
		[Net]
		public Vector3 StandMaxs { get; set; } = new Vector3( 16, 16, 72 );
		[Net]
		public Vector3 DuckMins { get; set; } = new Vector3( -16, -16, 0 );
		[Net]
		public Vector3 DuckMaxs { get; set; } = new Vector3( 16, 16, 32 );
	}
}

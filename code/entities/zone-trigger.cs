using Sandbox;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Momentum;

namespace Momentum.Entities
{
	public enum ZONE_MODE : int
	{
		TOUCH = 0,
		INAIR,
		GROUNDED,
		SURFING
	}

	public partial class BaseZoneTrigger : BaseTrigger
    {
		public int Id = 1;
		public string ZoneName;
		internal readonly ZONE_MODE Mode = 0;

		public override void Spawn()
		{
			base.Spawn();

			Transmit = TransmitType.Always;
			EnableTouch = true;
			EnableTouchPersists = true;
			EnableDrawing = false;
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			var particle = Particles.Create( "particles/gameplay/checkpoint/checkpoint.vpcf" );

			for ( int i = 0; i < 4; i++ )
			{
				var corner = Position + Model.PhysicsBounds.Corners.ElementAt( i );
				corner.z += 1;
				particle.SetPosition( i + 1, corner );
			}
		}
	}

	[Display( Name = "Zone Trigger" )]
	[Library( "zone_trigger", Description = "a zone trigger" ), Category( "Triggers" ), Icon( "flag_circle" )]
	public partial class ZoneTrigger : BaseZoneTrigger
	{
		public override void Touch( Entity ent )
		{
			if ( ent is not MomentumPlayer ) 
				return;

			MomentumPlayer player = (MomentumPlayer)ent;

			if ( (Mode is ZONE_MODE.INAIR && player.GroundEntity is null) || 
				( Mode is ZONE_MODE.GROUNDED && player.GroundEntity is not null ) )
			{
				base.EndTouch( ent );
				player.ZoneTouchEnd( this );

				return;
			}
			else if ( Mode is ZONE_MODE.SURFING)
			{
				Vector3 position = player.Position;
				TraceResult trace = TraceUtil.PlayerBBox( position,
											position.WithZ( position.z - 2 ),
											(BaseController)player.Controller );
				var normal = Vector3.GetAngle( Vector3.Up, trace.Normal );

				if ( normal > (float)player.MovementProps["StandableAngle"] || normal == 0)
				{
					base.EndTouch( ent );
					player.ZoneTouchEnd( this );

					return;
				}
			}

			base.Touch( ent );
			player.ZoneTouch( this );
		}

		public override void EndTouch( Entity ent )
		{
			if ( ent is not MomentumPlayer )
				return;

			MomentumPlayer player = (MomentumPlayer)ent;

			base.Touch( ent );
			player.ZoneTouchEnd( this );
		}
	}
}

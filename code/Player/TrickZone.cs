using Sandbox;
using System.Collections.Generic;
using TrickHop.Entities;

namespace TrickHop.Player
{
	public partial class Player
	{
		public List<ZoneTrigger> ZoneOrder = new();
		public List<string> ZoneOrderStatus = new();
		public List<Vector3> ZoneOrderSpeed = new();
		public List<float> ZoneOrderTime = new();
		public List<ZoneTrigger> CurrentTricks = new();

		public void AddZoneToOrder( ZoneTrigger zone, bool wasEntered )
		{
			BetterLog.Info( ZoneOrder.Count );
			if ( ZoneOrder.Count > 0 )
			{
				if ( ZoneOrder[^1].Name != zone.Name )
				{
					ZoneOrder.Add( zone );
					ZoneOrderStatus.Add( "Entered" );
					ZoneOrderSpeed.Add( Velocity );
					ZoneOrderTime.Add( Time.Now );
				}
				else
				{
					ZoneOrderSpeed[^1] = Velocity;
					ZoneOrderStatus[^1] = wasEntered ? "Entered" : "Left";
				}
			}
			else
			{
				ZoneOrder.Add( zone );
				ZoneOrderStatus.Add( "Entered" );
				ZoneOrderSpeed.Add( Velocity );
				ZoneOrderTime.Add( Time.Now );
			}
		}

		public void ClearOrder()
		{
			ZoneOrder = new();
			ZoneOrderSpeed = new();
			ZoneOrderTime = new();
			CurrentTricks = new();
		}

		public void ZoneTouch( ZoneTrigger zone ) => AddZoneToOrder( zone, true );

		public void ZoneTouchEnd( ZoneTrigger zone ) => AddZoneToOrder( zone, false );
	}
}

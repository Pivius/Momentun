namespace TrickHop.Player
{
	public enum STATE : int
	{
		GROUND = 0,
		INAIR,
		LADDER,
		WATER
	}

	public enum WATERLEVEL : int
	{
		NotInWater = 0,
		Feet,
		Waist,
		Eyes
	}

	public enum PlayerFlags : int
	{
		NONE = 0,
		DUCKING,
	}
}

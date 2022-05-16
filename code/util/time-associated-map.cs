using System;
using System.Collections.Generic;
using Sandbox;

namespace Momentum
{
	public class TimeAssociatedMap<T> : BaseNetworkable
	{
		public Dictionary<float,T> Map {get; set;} = new Dictionary<float,T>();
		public float LastTime {get; private set;}
		public T LastValue {get; private set;}
		public float Cooldown {get; set;}
		public Func<T> LookupMethod {get; set;}

		public TimeAssociatedMap(float cooldown, Func<T> lookup)
		{
			Cooldown = cooldown;
			LookupMethod = lookup;
		}

		public T Value
		{
			get
			{
				Cleanup();

				if ( !HasChecked() )
					Update();

				return Map[Time.Now];
			}
		}

		public void Update() => Set( (T)LookupMethod() );

		public bool HasChecked() => Map.ContainsKey( Time.Now );

		public void Set(T value)
		{
			Map[Time.Now] = value;
			LastTime = Time.Now;
			LastValue = value;
		}

		public void Cleanup()
		{
			foreach ( KeyValuePair<float, T> map in Map)
			{
				if (Time.Now > (map.Key + Cooldown))
					Map.Remove(map.Key);
			}
		}
	}
}

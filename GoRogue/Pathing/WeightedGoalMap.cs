﻿using GoRogue.MapViews;
using System.Collections.Generic;
using System.Diagnostics;

namespace GoRogue.Pathing
{
	/// <summary>
	/// Implementation of the second half of the goal map system described in
	/// http://www.roguebasin.com/index.php?title=The_Incredible_Power_of_Dijkstra_Maps, the ability
	/// to combine multiple goal maps with different weights.
	/// </summary>
	/// <remarks>
	/// This class encapsulates the work of building one overall goal map out of multiple existing
	/// maps. It holds references to one or more maps, each with its own "weight". The higher the
	/// weight value, the more strongly an AI will attempt to pursue this goal. A negative weight
	/// inverts the map, turning its goal into something to avoid.
	/// </remarks>
	public class WeightedGoalMap : IMapView<double?>
	{
		/// <summary>
		/// The list of weighted goal maps. Can be used to add or remove goal maps, or change their weights.
		/// </summary>
		/// <remarks>
		/// When adding a new goal map, its Height and Width should be identical to the Height and
		/// Width of the GoalMapCombiner.
		/// </remarks>
		public readonly Dictionary<IMapView<double?>, double> Weights;

		/// <summary>
		/// Constructor. Takes a single goal map and assigns it a weight of 1.
		/// </summary>
		/// <param name="map">The goal map.</param>
		public WeightedGoalMap(IMapView<double?> map)
		{
			Weights = new Dictionary<IMapView<double?>, double>();
			Weights.Add(map, 1);
			Width = map.Width;
			Height = map.Height;
		}

		/// <summary>
		/// Constructor. Takes a sequence of goal maps and assigns each one a weight of 1.
		/// </summary>
		/// <param name="maps">The goal maps. Each one should be of the same size.</param>
		public WeightedGoalMap(IEnumerable<IMapView<double?>> maps)
		{
			Weights = new Dictionary<IMapView<double?>, double>();
			foreach (var map in maps)
			{
				Weights.Add(map, 1);
				if (Height == 0)
				{
					Width = map.Width;
					Height = map.Height;
				}
				else
				{
					Debug.Assert(Height == map.Height && Width == map.Width);
				}
			}
		}

		/// <summary>
		/// Constructor. Takes an existing goal map dictionary and copies it.
		/// </summary>
		/// <param name="maps">
		/// The goal maps. Each one should be of the same size, and all weights should have a nonzero value.
		/// </param>
		public WeightedGoalMap(IDictionary<IMapView<double?>, double> maps)
		{
			Weights = new Dictionary<IMapView<double?>, double>();
			foreach (var pair in maps)
			{
				Weights.Add(pair.Key, pair.Value);
				if (Height == 0)
				{
					Width = pair.Key.Width;
					Height = pair.Key.Height;
				}
				else
				{
					Debug.Assert(Height == pair.Key.Height && Width == pair.Key.Width);
				}
				Debug.Assert(pair.Value != 0.0);
			}
		}

		/// <summary>
		/// The height of the goal map, and its underlying maps.
		/// </summary>
		public int Height { get; }

		

		/// <summary>
		/// The width of the goal map, and its underlying maps.
		/// </summary>
		public int Width { get; }

		public double? this[int index1D] => this[Coord.ToXValue(index1D, Width), Coord.ToYValue(index1D, Width)];

		/// <summary>
		/// Returns the value of the combined goal maps at any given point.
		/// </summary>
		public double? this[int x, int y]
		{
			get
			{
				var result = 0.0;
				var negResult = 0.0;
				foreach (var pair in Weights)
				{
					var value = pair.Key[x, y];
					if (!value.HasValue)
						return null;
					var weight = pair.Value;
					var weighted = value.Value * weight;
					if (weight > 0.0)
					{
						result = result == 0.0 ? weighted : result * weighted;
					}
					else
					{
						negResult = negResult == 0.0 ? weighted : negResult * weighted;
					}
				}
				return result + negResult;
			}
		}

		/// <summary>
		/// Returns the value of the combined goal maps at any given point.
		/// </summary>
		public double? this[Coord point] => this[point.X, point.Y];

		/// <summary>
		/// Computes the entire aggregate goal map. This may be useful in situations where the goals
		/// are shared between many characters and do not change frequently.
		/// </summary>
		public ArrayMap<double?> Combine()
		{
			var result = new ArrayMap<double?>(Width, Height);
			for (int y = 0; y < Height; ++y)
			{
				for (int x = 0; x < Width; ++x)
				{
					result[x, y] = this[x, y];
				}
			}
			return result;
		}
	}
}
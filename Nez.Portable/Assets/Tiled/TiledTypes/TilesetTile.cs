using System.Collections.Generic;

namespace Nez.Tiled
{
	public class TmxTilesetTile
	{
		public TmxTileset Tileset;

		public int Id;
		public TmxTerrain[] TerrainEdges;
		public double Probability;
		public string Type;

		public Dictionary<string, string> Properties;
		public TmxImage Image;
		public TmxList<TmxObjectGroup> ObjectGroups;
		public List<TmxAnimationFrame> AnimationFrames;

		// HACK: why do animated tiles need to add the firstGid?
		public int currentAnimationFrameGid => AnimationFrames[_animationCurrentFrame].Gid + Tileset.FirstGid;
		float _animationElapsedTime;
		int _animationCurrentFrame;

		/// <summary>
		/// returns the value of an "nez:isDestructable" property if present in the properties dictionary
		/// </summary>
		/// <value><c>true</c> if is destructable; otherwise, <c>false</c>.</value>
		public bool IsDestructable;

		/// <summary>
		/// returns the value of a "nez:isSlope" property if present in the properties dictionary
		/// </summary>
		/// <value>The is slope.</value>
		public bool IsSlope;

		/// <summary>
		/// returns the value of a "nez:isOneWayPlatform" property if present in the properties dictionary
		/// </summary>
		public bool IsOneWayPlatform;

		/// <summary>
		/// returns the value of a "nez:slopeTopLeft" property if present in the properties dictionary
		/// </summary>
		/// <value>The slope top left.</value>
		public int SlopeTopLeft;

		/// <summary>
		/// returns the value of a "nez:slopeTopRight" property if present in the properties dictionary
		/// </summary>
		/// <value>The slope top right.</value>
		public int SlopeTopRight;

		public void ProcessProperties()
		{
			string value;
			if (Properties.TryGetValue("nez:isDestructable", out value))
				IsDestructable = bool.Parse(value);

			if (Properties.TryGetValue("nez:isSlope", out value))
				IsSlope = bool.Parse(value);

			if (Properties.TryGetValue("nez:isOneWayPlatform", out value))
				IsOneWayPlatform = bool.Parse(value);

			if (Properties.TryGetValue("nez:slopeTopLeft", out value))
				SlopeTopLeft = int.Parse(value);

			if (Properties.TryGetValue("nez:slopeTopRight", out value))
				SlopeTopRight = int.Parse(value);
		}

		public void UpdateAnimatedTiles()
		{
			if (AnimationFrames.Count == 0)
				return;

			_animationElapsedTime += Time.DeltaTime;

			if (_animationElapsedTime > AnimationFrames[_animationCurrentFrame].Duration)
			{
				_animationCurrentFrame = Mathf.IncrementWithWrap(_animationCurrentFrame, AnimationFrames.Count);
				_animationElapsedTime = 0;
			}
		}
	}


	public class TmxAnimationFrame
	{
		public int Gid;
		public float Duration;
	}

}

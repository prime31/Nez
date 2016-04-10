using System;
using Newtonsoft.Json;
using System.Text.RegularExpressions;


namespace Nez.Overlap2D.Runtime
{
	public class MainItemVO
	{
		public int uniqueId = -1;
		public string itemIdentifier = string.Empty;
		public string itemName = string.Empty;
		public string[] tags;
		public string customVars = string.Empty;
		public float x;
		public float y;
		public float scaleX	= 1f;
		public float scaleY	= 1f;
		public float originX = 0;
		public float originY = 0;
		public float rotation;
		public int zIndex = 0;
		public float layerDepth;
		public string layerName
		{
			get { return _layerName; }
			set
			{
				if( value != null && value != string.Empty )
				{
					var regex = new Regex( @"\d+" );
					var match = regex.Match( value );
					if( match.Success )
						renderLayer = int.Parse( match.Value );
				}
				_layerName = value;
			}
		}
		string _layerName;

		// not part of Overlap2D spec
		public int renderLayer = 0;
		public float[] tint = { 1, 1, 1, 1 };

		public string shaderName = string.Empty;

		// this is written only for sColorPrimitives
		public ShapeVO shape;
		public PhysicsBodyDataVO physics;


		/// <summary>
		/// helper to translate zIndex to layerDepth. zIndexMax should be at least equal to the highest zIndex
		/// </summary>
		/// <returns>The depth.</returns>
		/// <param name="zIndexMax">Z index max.</param>
		public float calculateLayerDepth( float zIndexMin, float zIndexMax, CompositeItemVO compositeItem )
		{
			if( compositeItem != null )
				return compositeItem.calculateLayerDepthForChild( zIndexMin, zIndexMax, this );

			return Mathf.map01( (float)zIndex, zIndexMin, zIndexMax );
		}


		public override string ToString()
		{
			return JsonConvert.SerializeObject( this, Formatting.Indented );
		}
	}
}


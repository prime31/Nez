using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct VertexPositionColorNormal : IVertexType
	{
		public Vector3 Position;
		public Color Color;
		public Vector3 Normal;


		static readonly VertexDeclaration _vertexDeclaration = new VertexDeclaration
		(
			new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
			new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
			new VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
		);

		VertexDeclaration IVertexType.VertexDeclaration => _vertexDeclaration;


		public VertexPositionColorNormal(Vector3 position, Color color, Vector3 normal)
		{
			Position = position;
			Color = color;
			Normal = normal;
		}
	}
}
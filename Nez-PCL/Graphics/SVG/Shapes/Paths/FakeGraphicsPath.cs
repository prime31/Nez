using Microsoft.Xna.Framework;


namespace Nez.Svg
{
	public class FakeGraphicsPath
	{
		public int PointCount
		{
			get
			{
				var prop = ReflectionUtils.getPropertyInfo( _graphicsPath, "PointCount" );
				return (int)prop.GetValue( _graphicsPath );
			}
		}

		public System.Array PathPoints
		{
			get
			{
				var prop = ReflectionUtils.getPropertyInfo( _graphicsPath, "PathPoints" );
				return (System.Array)prop.GetValue( _graphicsPath );
			}
		}

		public int[] PathTypes
		{
			get
			{
				var prop = ReflectionUtils.getPropertyInfo( _graphicsPath, "PathTypes" );
				return (int[])prop.GetValue( _graphicsPath );
			}
		}


		object _graphicsPath;


		public FakeGraphicsPath()
		{
			_graphicsPath = System.Activator.CreateInstance( System.Type.GetType( "System.Drawing.Drawing2D.GraphicsPath, System.Drawing" ) );
		}


		public void StartFigure()
		{
			var method = ReflectionUtils.getMethodInfo( _graphicsPath, "StartFigure" );
			method.Invoke( _graphicsPath, new object[0] );
		}


		public void CloseFigure()
		{
			var method = ReflectionUtils.getMethodInfo( _graphicsPath, "CloseFigure" );
			method.Invoke( _graphicsPath, new object[0] );
		}


		public void AddBezier( object first, object second, object third, object fourth )
		{
			var method = ReflectionUtils.getMethodInfo( _graphicsPath, "AddBezier" );
			method.Invoke( _graphicsPath, new object[] { first, second, third, fourth } );
		}


		public void AddLine( object first, object second )
		{
			var method = ReflectionUtils.getMethodInfo( _graphicsPath, "AddLine" );
			method.Invoke( _graphicsPath, new object[] { first, second } );
		}


		public void Flatten( object matrix, float flatness )
		{
			var paramTypes = new System.Type[] { matrix.GetType(), flatness.GetType() };
			var method = ReflectionUtils.getMethodInfo( _graphicsPath, "Flatten", paramTypes );
			method.Invoke( _graphicsPath, new object[] { matrix, flatness } );
		}


		public Vector2[] pathPointsAsVectors()
		{
			var pathPoints = PathPoints;
			if( pathPoints.Length == 0 )
				return new Vector2[0];

			var pts = new Vector2[pathPoints.Length];
			var getX = ReflectionUtils.getPropertyInfo( pathPoints.GetValue( 0 ), "X" );
			var getY = ReflectionUtils.getPropertyInfo( pathPoints.GetValue( 0 ), "Y" );

			for( var i = 0; i < pathPoints.Length; i++ )
			{
				var obj = pathPoints.GetValue( i );
				pts[i] = new Microsoft.Xna.Framework.Vector2( (float)getX.GetValue( obj ), (float)getY.GetValue( obj ) );
			}

			return pts;
		}
	}
}

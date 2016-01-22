using System;
using System.Collections.Generic;
using System.Text;


namespace Nez
{
	public class Matcher
	{
		protected BitSet allSet = new BitSet();
		protected BitSet exclusionSet = new BitSet();
		protected BitSet oneSet = new BitSet();


		public Matcher()
		{}


		public BitSet getAllSet()
		{
			return allSet;
		}


		public BitSet getExclusionSet()
		{
			return exclusionSet;
		}


		public BitSet getOneSet()
		{
			return oneSet;
		}


		public bool isInterested( Entity e )
		{
			return isInterested( e.componentBits );
		}


		public bool isInterested( BitSet componentBits )
		{
			// Check if the entity possesses ALL of the components defined in the aspect.
			if( !allSet.isEmpty() )
			{
				for( int i = allSet.nextSetBit( 0 ); i >= 0; i = allSet.nextSetBit( i + 1 ) )
				{
					if( !componentBits.get( i ) )
					{
						return false;
					}
				}
			}

			// If we are STILL interested,
			// Check if the entity possesses ANY of the exclusion components, if it does then the system is not interested.
			if( !exclusionSet.isEmpty() && exclusionSet.intersects( componentBits ) )
			{
				return false;
			}

			// If we are STILL interested,
			// Check if the entity possesses ANY of the components in the oneSet. If so, the system is interested.
			if( !oneSet.isEmpty() && !oneSet.intersects( componentBits ) )
			{
				return false;
			}

			return true;
		}


		public Matcher all( params Type[] types )
		{
			foreach( var type in types )
				allSet.set( ComponentTypeManager.getIndexFor( type ) );

			return this;
		}


		public Matcher exclude( params Type[] types )
		{
			foreach( var type in types )
				exclusionSet.set( ComponentTypeManager.getIndexFor( type ) );

			return this;
		}


		public Matcher one( params Type[] types )
		{
			foreach( var type in types )
				oneSet.set( ComponentTypeManager.getIndexFor( type ) );

			return this;
		}


		public static Matcher empty()
		{
			return new Matcher();
		}


		public override string ToString()
		{
			var builder = new StringBuilder( 1024 );

			builder.AppendLine( "Matcher:" );
			appendTypes( builder, " -  Requires the components: ", allSet );
			appendTypes( builder, " -  Has none of the components: ", exclusionSet );
			appendTypes( builder, " -  Has at least one of the components: ", oneSet );

			return builder.ToString();
		}


		static void appendTypes( StringBuilder builder, string headerMessage, BitSet typeBits )
		{
			var firstType = true;
			builder.Append( headerMessage );
			foreach( var type in ComponentTypeManager.getTypesFromBits( typeBits ) )
			{
				if( !firstType )
					builder.Append( ", " );
				builder.Append( type.Name );

				firstType = false;
			}

			builder.AppendLine();
		}

	}
}


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
		{
		}

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

		public bool isInterested(Entity e)
		{
			return isInterested(e.componentBits);
		}

		public bool isInterested(BitSet componentBits)
		{
			// Check if the entity possesses ALL of the components defined in the aspect.
			if(!allSet.IsEmpty()) {
				for (int i = allSet.NextSetBit(0); i >= 0; i = allSet.NextSetBit(i+1)) {
					if(!componentBits.Get(i)) {
						return false;
					}
				}
			}

			// If we are STILL interested,
			// Check if the entity possesses ANY of the exclusion components, if it does then the system is not interested.
			if(!exclusionSet.IsEmpty() && exclusionSet.Intersects(componentBits)) {
				return false;
			}

			// If we are STILL interested,
			// Check if the entity possesses ANY of the components in the oneSet. If so, the system is interested.
			if(!oneSet.IsEmpty() && !oneSet.Intersects(componentBits)) {
				return false;
			}

			return true;
		}

		public Matcher all(params Type[] types)
		{
			foreach( var type in types )
			{
				allSet.Set( ComponentTypeManager.getIndexFor( type ) );
			}
			return this;
		}


		public Matcher exclude(params Type[] types)
		{
			foreach( var type in types )
			{
				exclusionSet.Set( ComponentTypeManager.getIndexFor( type ) );
			}
			return this;
		}

		public Matcher one(params Type[] types)
		{
			foreach( var type in types )
			{
				oneSet.Set( ComponentTypeManager.getIndexFor( type ) );
			}
			return this;
		}

		public static Matcher empty()
		{
			return new Matcher();
		}

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder(1024);

			builder.AppendLine("Matcher :");
			AppendTypes(builder, " Requires the components : ", allSet);
			AppendTypes(builder, " Has none of the components : ", exclusionSet);
			AppendTypes(builder, " Has atleast one of the components : ", oneSet);

			return builder.ToString();
		}

		private static void AppendTypes(StringBuilder builder, string headerMessage, BitSet typeBits)
		{
			builder.AppendLine(headerMessage);
			foreach (Type type in ComponentTypeManager.getTypesFromBits(typeBits))
			{
				builder.Append(", ");
				builder.AppendLine(type.Name);
			}
		}

	}
}


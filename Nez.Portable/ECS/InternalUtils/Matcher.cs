using System;
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


		public BitSet GetAllSet()
		{
			return allSet;
		}


		public BitSet GetExclusionSet()
		{
			return exclusionSet;
		}


		public BitSet GetOneSet()
		{
			return oneSet;
		}


		public bool IsInterested(Entity e)
		{
			return IsInterested(e.componentBits);
		}


		public bool IsInterested(BitSet componentBits)
		{
			// Check if the entity possesses ALL of the components defined in the aspect.
			if (!allSet.IsEmpty())
			{
				for (int i = allSet.NextSetBit(0); i >= 0; i = allSet.NextSetBit(i + 1))
				{
					if (!componentBits.Get(i))
					{
						return false;
					}
				}
			}

			// If we are STILL interested,
			// Check if the entity possesses ANY of the exclusion components, if it does then the system is not interested.
			if (!exclusionSet.IsEmpty() && exclusionSet.Intersects(componentBits))
			{
				return false;
			}

			// If we are STILL interested,
			// Check if the entity possesses ANY of the components in the oneSet. If so, the system is interested.
			if (!oneSet.IsEmpty() && !oneSet.Intersects(componentBits))
			{
				return false;
			}

			return true;
		}


		public Matcher All(params Type[] types)
		{
			foreach (var type in types)
				allSet.Set(ComponentTypeManager.GetIndexFor(type));

			return this;
		}


		public Matcher Exclude(params Type[] types)
		{
			foreach (var type in types)
				exclusionSet.Set(ComponentTypeManager.GetIndexFor(type));

			return this;
		}


		public Matcher One(params Type[] types)
		{
			foreach (var type in types)
				oneSet.Set(ComponentTypeManager.GetIndexFor(type));

			return this;
		}


		public static Matcher Empty()
		{
			return new Matcher();
		}


		public override string ToString()
		{
			var builder = new StringBuilder(1024);

			builder.AppendLine("Matcher:");
			AppendTypes(builder, " -  Requires the components: ", allSet);
			AppendTypes(builder, " -  Has none of the components: ", exclusionSet);
			AppendTypes(builder, " -  Has at least one of the components: ", oneSet);

			return builder.ToString();
		}


		static void AppendTypes(StringBuilder builder, string headerMessage, BitSet typeBits)
		{
			var firstType = true;
			builder.Append(headerMessage);
			foreach (var type in ComponentTypeManager.GetTypesFromBits(typeBits))
			{
				if (!firstType)
					builder.Append(", ");
				builder.Append(type.Name);

				firstType = false;
			}

			builder.AppendLine();
		}
	}
}
using System;
using System.Globalization;
using Nez.Persistence;
using NUnit.Framework;


namespace Nez.Persistence.JsonTests
{
	[TestFixture]
	public class NullableTests
	{
		class NullableMembers
		{
			public bool? nullableBool;
			public int? nullableInt;
			public float? nullableFloat;
		}


		[Test]
		public void Nullable_BoolIsNull()
		{
			var hasNull = new NullableMembers();
			var json = Json.ToJson( hasNull );

			System.Diagnostics.Debug.WriteLine( json );
			var obj = Json.FromJson<NullableMembers>( json );
			Assert.AreEqual( hasNull.nullableBool, obj.nullableBool );
        }

		[Test]
		public void Nullable_BoolIsTrue()
		{
			var hasNull = new NullableMembers();
			hasNull.nullableBool = true;
			var json = Json.ToJson( hasNull );

			System.Diagnostics.Debug.WriteLine( json );

			var obj = Json.FromJson<NullableMembers>( json );
            Assert.AreEqual( hasNull.nullableBool, obj.nullableBool );
        }

		[Test]
		public void Nullable_BoolIsFalse()
		{
			var hasNull = new NullableMembers();
			hasNull.nullableBool = false;
			var json = Json.ToJson( hasNull );

			System.Diagnostics.Debug.WriteLine( json );

			var obj = Json.FromJson<NullableMembers>( json );
			Assert.AreEqual( hasNull.nullableBool, obj.nullableBool );
        }

		[Test]
		public void Nullable_IntIsNull()
		{
			var hasNull = new NullableMembers();
			var json = Json.ToJson( hasNull );

			System.Diagnostics.Debug.WriteLine( json );
			var obj = Json.FromJson<NullableMembers>( json );
			Assert.AreEqual( hasNull.nullableInt, obj.nullableInt );
        }

		[Test]
		public void Nullable_IntHasValue()
		{
			var hasNull = new NullableMembers();
			hasNull.nullableInt = 666;
			var json = Json.ToJson( hasNull );

			System.Diagnostics.Debug.WriteLine( json );

			var obj = Json.FromJson<NullableMembers>( json );
			Assert.AreEqual( hasNull.nullableInt, obj.nullableInt );
        }

		[Test]
		public void Nullable_FloatIsNull()
		{
			var hasNull = new NullableMembers();
			hasNull.nullableFloat = null;
			var json = Json.ToJson( hasNull );

			System.Diagnostics.Debug.WriteLine( json );

			var obj = Json.FromJson<NullableMembers>( json );
			Assert.AreEqual( hasNull.nullableFloat, obj.nullableFloat );
        }

		[Test]
		public void Nullable_FloatIsHasValue()
		{
			var hasNull = new NullableMembers();
			hasNull.nullableFloat = 666.666f;
			var json = Json.ToJson( hasNull );

			System.Diagnostics.Debug.WriteLine( json );

			var obj = Json.FromJson<NullableMembers>( json );
			Assert.AreEqual( hasNull.nullableFloat, obj.nullableFloat );
        }
	}
}
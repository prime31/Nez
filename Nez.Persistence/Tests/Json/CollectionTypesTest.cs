using System;
using System.Collections;
using System.Collections.Generic;
using Nez.Persistence;
using NUnit.Framework;

namespace Nez.Persistence.JsonTests
{
	[TestFixture]
	public class CollectionTypesTest
	{
		[Test]
		public void DumpRank1Array()
		{
			var array = new[] { 3, 1, 4 };
			Assert.AreEqual( "[3,1,4]", Json.ToJson( array ) );
		}

		[Test]
		public void DumpRank2Array()
		{
			var array = new[,] { { 1, 2, 3 }, { 4, 5, 6 } };
			Assert.AreEqual( "[[1,2,3],[4,5,6]]", Json.ToJson( array ) );
		}

		[Test]
		public void DumpRank3Array()
		{
			var array = new[, ,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } }, { { 9, 0 }, { 1, 2 } } };
			Assert.AreEqual( "[[[1,2],[3,4]],[[5,6],[7,8]],[[9,0],[1,2]]]", Json.ToJson( array ) );
		}

		[Test]
		public void DumpJaggedArray()
		{
			var array = new[] { new[] { 1, 2, 3 }, new[] { 4, 5, 6 } };
			Assert.AreEqual( "[[1,2,3],[4,5,6]]", Json.ToJson( array ) );
		}


		[Test]
		public void LoadRank1Array()
		{
			var json = "[1,2,3]";
			var array = Json.FromJson<int[]>( json );

			Assert.AreNotEqual( null, array );
			Assert.AreEqual( 3, array.Length );
			Assert.AreEqual( 1, array[0] );
			Assert.AreEqual( 2, array[1] );
			Assert.AreEqual( 3, array[2] );
		}

		[Test]
		public void LoadRank2Array()
		{
			var json = "[[1,2,3],[4,5,6]]";
			var array = Json.FromJson<int[,]>( json );

			Assert.AreNotEqual( null, array );

			Assert.AreEqual( 2, array.Rank );
			Assert.AreEqual( 2, array.GetLength( 0 ) );
			Assert.AreEqual( 3, array.GetLength( 1 ) );

			Assert.AreEqual( 1, array[0, 0] );
			Assert.AreEqual( 2, array[0, 1] );
			Assert.AreEqual( 3, array[0, 2] );

			Assert.AreEqual( 4, array[1, 0] );
			Assert.AreEqual( 5, array[1, 1] );
			Assert.AreEqual( 6, array[1, 2] );
		}

		[Test]
		public void LoadRank2StringArray()
		{
			var json = "[[\"1\",\"2\",\"3\"],[\"4\",\"5\",\"6\"]]";
			var array = Json.FromJson<string[,]>( json );

			Assert.AreNotEqual( null, array );

			Assert.AreEqual( 2, array.Rank );
			Assert.AreEqual( 2, array.GetLength( 0 ) );
			Assert.AreEqual( 3, array.GetLength( 1 ) );

			Assert.AreEqual( "1", array[0, 0] );
			Assert.AreEqual( "2", array[0, 1] );
			Assert.AreEqual( "3", array[0, 2] );

			Assert.AreEqual( "4", array[1, 0] );
			Assert.AreEqual( "5", array[1, 1] );
			Assert.AreEqual( "6", array[1, 2] );
		}

		struct TestStruct
		{
			public int x;
			public int y;
		}

		[Test]
		public void LoadRank2ObjectArray()
		{
			var json = "[[{\"x\":5,\"y\":7}, {\"x\":5,\"y\":7}],[{\"x\":5,\"y\":7}, {\"x\":77,\"y\":7}]]";
			var array = Json.FromJson<TestStruct[,]>( json );

			Assert.AreNotEqual( null, array );

			Assert.AreEqual( 2, array.Rank );
			Assert.AreEqual( 2, array.GetLength( 0 ) );
			Assert.AreEqual( 2, array.GetLength( 1 ) );

			Assert.AreEqual( 77, array[1, 1].x );
		}

		[Test]
		public void LoadRank3Array()
		{
			var json = "[[[1,2],[3,4]],[[5,6],[7,8]],[[9,0],[1,2]]]";
			var array = Json.FromJson<int[,,]>( json );
			Assert.AreNotEqual( null, array );

			Assert.AreEqual( 3, array.Rank );
			Assert.AreEqual( 3, array.GetLength( 0 ) );
			Assert.AreEqual( 2, array.GetLength( 1 ) );
			Assert.AreEqual( 2, array.GetLength( 2 ) );

			Assert.AreEqual( 1, array[0, 0, 0] );
			Assert.AreEqual( 2, array[0, 0, 1] );

			Assert.AreEqual( 3, array[0, 1, 0] );
			Assert.AreEqual( 4, array[0, 1, 1] );

			Assert.AreEqual( 5, array[1, 0, 0] );
			Assert.AreEqual( 6, array[1, 0, 1] );

			Assert.AreEqual( 7, array[1, 1, 0] );
			Assert.AreEqual( 8, array[1, 1, 1] );

			Assert.AreEqual( 9, array[2, 0, 0] );
			Assert.AreEqual( 0, array[2, 0, 1] );

			Assert.AreEqual( 1, array[2, 1, 0] );
			Assert.AreEqual( 2, array[2, 1, 1] );
		}

		[Test]
		public void LoadJaggedArray()
		{
			var json = "[[1,2,3],[4,5,6]]";
			var array = Json.FromJson<int[][]>( json );
			Assert.AreNotEqual( null, array );

			Assert.AreEqual( 2, array.Length );

			Assert.AreEqual( 3, array[0].Length );
			Assert.AreEqual( 1, array[0][0] );
			Assert.AreEqual( 2, array[0][1] );
			Assert.AreEqual( 3, array[0][2] );

			Assert.AreEqual( 3, array[1].Length );
			Assert.AreEqual( 4, array[1][0] );
			Assert.AreEqual( 5, array[1][1] );
			Assert.AreEqual( 6, array[1][2] );
		}


		[Test]
		public void DumpList()
		{
			var list = new List<int>() { { 3 }, { 1 }, { 4 } };
			Assert.AreEqual( "[3,1,4]", Json.ToJson( list ) );
		}

		[Test]
		public void LoadList()
		{
			var list = Json.FromJson<List<int>>( "[3,1,4]" );
			Assert.AreNotEqual( null, list );

			Assert.AreEqual( 3, list.Count );
			Assert.AreEqual( 3, list[0] );
			Assert.AreEqual( 1, list[1] );
			Assert.AreEqual( 4, list[2] );
		}

		class ListOfObjects
		{
			public int x = 5;
			public int y = 10;
		}

		[Test]
		public void DumpListOfObjects()
		{
			var list = new List<ListOfObjects> { { new ListOfObjects() }, { new ListOfObjects() }, { new ListOfObjects() } };
			var json = Json.ToJson( list );

			Assert.AreEqual( "[{\"x\":5,\"y\":10},{\"x\":5,\"y\":10},{\"x\":5,\"y\":10}]", Json.ToJson( list ) );
		}

		[Test]
		public void LoadListOfObjects()
		{
			var list = Json.FromJson<List<ListOfObjects>>( "[{\"x\":5,\"y\":10},{\"x\":15,\"y\":10},{\"x\":25,\"y\":10}]" );
			Assert.AreNotEqual( null, list );

			Assert.AreEqual( 3, list.Count );
			Assert.AreEqual( 5, list[0].x );
			Assert.AreEqual( 15, list[1].x );
			Assert.AreEqual( 25, list[2].x );
		}

		[Test]
		public void DumpDict()
		{
			var dict = new Dictionary<string, float>();
			dict["foo"] = 1337f;
			dict["bar"] = 3.14f;

			Assert.AreEqual( "{\"foo\":1337,\"bar\":3.14}", Json.ToJson( dict ) );
		}


		[Test]
		public void LoadDict()
		{
			var dict = Json.FromJson<Dictionary<string, float>>( "{\"foo\":1337,\"bar\":3.14}" );

			Assert.AreNotEqual( null, dict );
			Assert.AreEqual( 2, dict.Count );
			Assert.AreEqual( 1337f, dict["foo"] );
			Assert.AreEqual( 3.14f, dict["bar"] );
		}


		[Test]
		public void LoadDictIntoGeneric()
		{
			var dict = Json.FromJson( "{\"foo\":1337,\"bar\":3.14}" ) as IDictionary;

			Assert.IsNotNull( dict );
			Assert.AreEqual( 2, dict.Count );
			Assert.AreEqual( 1337f, Convert.ToSingle( dict["foo"] ) );
			Assert.AreEqual( 3.14f, Convert.ToSingle( dict["bar"] ) );
		}


		enum TestEnum
		{
			Thing1,
			Thing2,
			Thing3
		}

		[Test]
		public void DumpEnum()
		{
			const TestEnum testEnum = TestEnum.Thing2;
			Assert.AreEqual( "\"Thing2\"", Json.ToJson( testEnum ) );
		}

		[Test]
		public void LoadEnum()
		{
			var testEnum = Json.FromJson<TestEnum>( "\"Thing2\"" );
			Assert.AreEqual( TestEnum.Thing2, testEnum );

			try
			{
				Json.FromJson<TestEnum>( "\"Thing4\"" );
			}
			catch( ArgumentException e )
			{
				Assert.AreEqual( e.Message, "Requested value 'Thing4' was not found." );
			}
		}

		[Test]
		public void DumpDictWithEnumKeys()
		{
			var dict = new Dictionary<TestEnum, string>
			{
				[TestEnum.Thing1] = "Item 1",
				[TestEnum.Thing2] = "Item 2",
				[TestEnum.Thing3] = "Item 3"
			};
			Assert.AreEqual( "{\"Thing1\":\"Item 1\",\"Thing2\":\"Item 2\",\"Thing3\":\"Item 3\"}", Json.ToJson( dict ) );
		}

		[Test]
		public void LoadDictWithEnumKeys()
		{
			const string json = "{\"Thing1\":\"Item 1\",\"Thing2\":\"Item 2\",\"Thing3\":\"Item 3\"}";
			var dict = Json.FromJson<Dictionary<TestEnum, string>>( json );

			Assert.AreNotEqual( null, dict );
			Assert.AreEqual( 3, dict.Count );
			Assert.AreEqual( "Item 1", dict[TestEnum.Thing1] );
			Assert.AreEqual( "Item 2", dict[TestEnum.Thing2] );
			Assert.AreEqual( "Item 3", dict[TestEnum.Thing3] );
		}

	}
}
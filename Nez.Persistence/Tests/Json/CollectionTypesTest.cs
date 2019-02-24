using System;
using System.Collections.Generic;
using Nez.Persistence;
using NUnit.Framework;

namespace Nez.Persistence.JsonTests
{
	[TestFixture]
	public class CollectionTypesTest
	{
		[Test]
		public void TestDumpRank1Array()
		{
			var array = new[] { 3, 1, 4 };
			Assert.AreEqual( "[3,1,4]", Json.ToJson( array ) );
		}


		[Test]
		public void TestDumpRank2Array()
		{
			var array = new[,] { { 1, 2, 3 }, { 4, 5, 6 } };
			Assert.AreEqual( "[[1,2,3],[4,5,6]]", Json.ToJson( array ) );
		}


		[Test]
		public void TestDumpRank3Array()
		{
			var array = new[, ,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } }, { { 9, 0 }, { 1, 2 } } };
			Assert.AreEqual( "[[[1,2],[3,4]],[[5,6],[7,8]],[[9,0],[1,2]]]", Json.ToJson( array ) );
		}


		[Test]
		public void TestDumpJaggedArray()
		{
			var array = new[] { new[] { 1, 2, 3 }, new[] { 4, 5, 6 } };
			Assert.AreEqual( "[[1,2,3],[4,5,6]]", Json.ToJson( array ) );
		}


		[Test]
		public void TestLoadRank1Array()
		{
			var variant = Json.FromJson( "[1,2,3]" );
			int[] array = VariantConverter.Decode<int[]>( variant );
			Assert.AreNotEqual( null, array );

			Assert.AreEqual( 3, array.Length );
			Assert.AreEqual( 1, array[0] );
			Assert.AreEqual( 2, array[1] );
			Assert.AreEqual( 3, array[2] );
		}


		[Test]
		public void TestLoadRank2Array()
		{
			var variant = Json.FromJson( "[[1,2,3],[4,5,6]]" );
			var array = VariantConverter.Decode<int[,]>( variant );
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
		public void TestLoadRank3Array()
		{
			var variant = Json.FromJson( "[[[1,2],[3,4]],[[5,6],[7,8]],[[9,0],[1,2]]]" );
			int[,,] array = VariantConverter.Decode<int[,,]>( variant );
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
		public void TestLoadJaggedArray()
		{
			var variant = Json.FromJson( "[[1,2,3],[4,5,6]]" );
			int[][] array = VariantConverter.Decode<int[][]>( variant );
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
		public void TestDumpList()
		{
			var list = new List<int>() { { 3 }, { 1 }, { 4 } };
			Assert.AreEqual( "[3,1,4]", Json.ToJson( list ) );
		}


		[Test]
		public void TestLoadList()
		{
			var variant = Json.FromJson( "[3,1,4]" );
			var list = VariantConverter.Decode<List<int>>( variant );

			Assert.AreNotEqual( null, list );

			Assert.AreEqual( 3, list.Count );
			Assert.AreEqual( 3, list[0] );
			Assert.AreEqual( 1, list[1] );
			Assert.AreEqual( 4, list[2] );
		}


		[Test]
		public void TestDumpDict()
		{
			var dict = new Dictionary<string, float>();
			dict["foo"] = 1337f;
			dict["bar"] = 3.14f;

			Assert.AreEqual( "{\"foo\":1337,\"bar\":3.14}", Json.ToJson( dict ) );
		}


		[Test]
		public void TestLoadDict()
		{
			var variant = Json.FromJson( "{\"foo\":1337,\"bar\":3.14}" );

			Dictionary<string, float> dict = VariantConverter.Decode<Dictionary<string, float>>( variant );

			Assert.AreNotEqual( null, dict );
			Assert.AreEqual( 2, dict.Count );
			Assert.AreEqual( 1337f, (float)dict["foo"] );
			Assert.AreEqual( 3.14f, (float)dict["bar"] );
		}


		[Test]
		public void TestLoadDictIntoProxy()
		{
			var variant = Json.FromJson( "{\"foo\":1337,\"bar\":3.14}" );
			var proxy = variant as ProxyObject;

			Assert.IsNotNull( proxy );
			Assert.AreEqual( 2, proxy.Count );
			Assert.AreEqual( 1337f, (float)proxy["foo"] );
			Assert.AreEqual( 3.14f, (float)proxy["bar"] );
		}


		enum TestEnum
		{
			Thing1,
			Thing2,
			Thing3
		}

		[Test]
		public void TestDumpEnum()
		{
			const TestEnum testEnum = TestEnum.Thing2;
			Assert.AreEqual( "\"Thing2\"", Json.ToJson( testEnum ) );
		}


		[Test]
		public void TestLoadEnum()
		{
			var testEnum = VariantConverter.Decode<TestEnum>( Json.FromJson( "\"Thing2\"" ) );
			Assert.AreEqual( TestEnum.Thing2, testEnum );

			try
			{
				VariantConverter.Decode<TestEnum>( Json.FromJson( "\"Thing4\"" ) );
			}
			catch( ArgumentException e )
			{
				Assert.AreEqual( e.Message, "Requested value 'Thing4' was not found." );
			}
		}


		[Test]
		public void TestDumpDictWithEnumKeys()
		{
			var dict = new Dictionary<TestEnum, String>();
			dict[TestEnum.Thing1] = "Item 1";
			dict[TestEnum.Thing2] = "Item 2";
			dict[TestEnum.Thing3] = "Item 3";
			Assert.AreEqual( "{\"Thing1\":\"Item 1\",\"Thing2\":\"Item 2\",\"Thing3\":\"Item 3\"}", Json.ToJson( dict ) );
		}


		[Test]
		public void TestLoadDictWithEnumKeys()
		{
			const string json = "{\"Thing1\":\"Item 1\",\"Thing2\":\"Item 2\",\"Thing3\":\"Item 3\"}";
			var dict = VariantConverter.Decode<Dictionary<TestEnum, String>>( Json.FromJson( json ) );
			Assert.AreNotEqual( null, dict );
			Assert.AreEqual( 3, dict.Count );
			Assert.AreEqual( "Item 1", dict[TestEnum.Thing1] );
			Assert.AreEqual( "Item 2", dict[TestEnum.Thing2] );
			Assert.AreEqual( "Item 3", dict[TestEnum.Thing3] );
		}
	}
}
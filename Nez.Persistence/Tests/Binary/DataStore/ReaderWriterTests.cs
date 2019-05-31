using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Nez.Persistence.Binary.Tests
{
	public class ReaderWriterTests
	{
		void WritePrimitives( IPersistableWriter writer )
		{
			writer.Write( TestValues.aString );
			writer.Write( TestValues.aInt );
			writer.Write( TestValues.aUint );
			writer.Write( TestValues.aFloat );
			writer.Write( TestValues.aDouble );
			writer.Write( TestValues.aBool );
		}

		void ReadPrimitives( IPersistableReader reader )
		{
			Assert.AreEqual( reader.ReadString(), TestValues.aString );
			Assert.AreEqual( reader.ReadInt(), TestValues.aInt );
			Assert.AreEqual( reader.ReadUInt(), TestValues.aUint );
			Assert.AreEqual( reader.ReadFloat(), TestValues.aFloat );
			Assert.AreEqual( reader.ReadDouble(), TestValues.aDouble );
			Assert.AreEqual( reader.ReadBool(), TestValues.aBool );
		}

		void WriteComplex( IPersistableWriter writer )
		{
			writer.Write( TestValues.aNullableBool );
			writer.Write( TestValues.aIntList );
			writer.Write( TestValues.aFloatList );
			writer.Write( TestValues.aStringList );
			writer.Write( TestValues.aIntArray );
			writer.Write( TestValues.aFloatArray );
			writer.Write( TestValues.aStringArray );
		}

		void ReadComplex( IPersistableReader reader )
		{
			Assert.AreEqual( reader.ReadOptionalBool(), TestValues.aNullableBool );
			var intList = new List<int>();
			reader.ReadIntListInto( intList );
			CollectionAssert.AreEqual( intList, TestValues.aIntList );

			var floatList = new List<float>();
			reader.ReadFloatListInto( floatList );
			CollectionAssert.AreEqual( floatList, TestValues.aFloatList );

			var stringList = new List<string>();
			reader.ReadStringListInto( stringList );
			CollectionAssert.AreEqual( stringList, TestValues.aStringList );

			var intArray = reader.ReadIntArray();
			CollectionAssert.AreEqual( intArray, TestValues.aIntArray );

			var floatArray = reader.ReadFloatArray();
			CollectionAssert.AreEqual( floatArray, TestValues.aFloatArray );

			var stringArray = reader.ReadStringArray();
			CollectionAssert.AreEqual( stringArray, TestValues.aStringArray );
		}

		[Test]
		public void BinaryWriterReader_WritesAndReadsPrimitives()
		{
			var memStream = new MemoryStream();
			var writer = new BinaryPersistableWriter( memStream );
			WritePrimitives( writer );
			writer.Flush();

			memStream.Position = 0;
			var reader = new BinaryPersistableReader( memStream );
			ReadPrimitives( reader );
		}

		[Test]
		public void ReusableWriterReader_WritesAndReadsPrimitives()
		{
			var memStream = new MemoryStream();
			var writer = new ReuseableBinaryWriter( memStream );
			WritePrimitives( writer );
			writer.Flush();

			memStream.Position = 0;
			var reader = new ReuseableBinaryReader( memStream );
			ReadPrimitives( reader );
		}

		[Test]
		public void BinaryWriterReader_WritesAndReadsComplex()
		{
			var memStream = new MemoryStream();
			var writer = new BinaryPersistableWriter( memStream );
			WriteComplex( writer );
			writer.Flush();

			memStream.Position = 0;
			var reader = new BinaryPersistableReader( memStream );
			ReadComplex( reader );
		}

		[Test]
		public void ReusableWriterReader_WritesAndReadsComplex()
		{
			var memStream = new MemoryStream();
			var writer = new ReuseableBinaryWriter( memStream );
			WriteComplex( writer );
			writer.Flush();

			memStream.Position = 0;
			var reader = new ReuseableBinaryReader( memStream );
			ReadComplex( reader );
		}

		[Test]
		public void ReusableWriterReader_IsReuseable()
		{
			var writer = new ReuseableBinaryWriter();
			var reader = new ReuseableBinaryReader();

			for( var i = 0; i < 3; i++ )
			{
				var memStream = new MemoryStream();
				writer.SetStream( memStream );
				WritePrimitives( writer );
				writer.Flush();

				memStream.Position = 0;
				reader.SetStream( memStream );
				ReadPrimitives( reader );

				writer.Dispose();
				reader.Dispose();
			}
		}

	}
}
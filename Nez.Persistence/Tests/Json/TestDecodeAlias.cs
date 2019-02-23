using Nez.Persistance;
using NUnit.Framework;


namespace Nez.Persistence.JsonTests
{
	[TestFixture]
	public class TestDecodeAlias
	{
		class AliasData
		{
			[DecodeAliasAttribute( "numberFieldAlias" )]
			public int NumberField;

			[SerializedAttribute]
			[DecodeAliasAttribute( "NumberPropertyAlias" )]
			public int NumberProperty { get; set; }

			[DecodeAliasAttribute( "anotherNumberFieldAliasOne", "anotherNumberFieldAliasTwo" )]
			public int AnotherNumberField;

			[DecodeAliasAttribute( "AnotherNumberPropertyAliasOne" )]
			[DecodeAliasAttribute( "AnotherNumberPropertyAliasTwo" )]
			public int AnotherNumberProperty;
		}


		[Test]
		public void TestLoadAlias()
		{
			const string json = "{ \"numberFieldAlias\" : 1, \"NumberPropertyAlias\" : 2, \"anotherNumberFieldAliasOne\" : 3, \"anotherNumberFieldAliasTwo\" : 4, \"AnotherNumberPropertyAliasOne\" : 5, \"AnotherNumberPropertyAliasTwo\" : 6 }";
			var aliasData = VariantConverter.Make<AliasData>( Json.Decode( json ) );

			Assert.AreEqual( 1, aliasData.NumberField );
			Assert.AreEqual( 2, aliasData.NumberProperty );
			Assert.IsTrue( aliasData.AnotherNumberField == 3 || aliasData.AnotherNumberField == 4 );
			Assert.IsTrue( aliasData.AnotherNumberProperty == 5 || aliasData.AnotherNumberProperty == 6 );
		}
	}
}
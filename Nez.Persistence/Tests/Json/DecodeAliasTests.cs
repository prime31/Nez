using Nez.Persistence;
using NUnit.Framework;


namespace Nez.Persistence.JsonTests
{
	[TestFixture]
	public class DecodeAliasTests
	{
		class AliasData
		{
			[DecodeAlias( "numberFieldAlias" )]
			public int NumberField;

			[JsonInclude]
			[DecodeAlias( "NumberPropertyAlias" )]
			public int NumberProperty { get; set; }

			[DecodeAlias( "anotherNumberFieldAliasOne", "anotherNumberFieldAliasTwo" )]
			public int AnotherNumberField;

			[DecodeAlias( "AnotherNumberPropertyAliasOne" )]
			[DecodeAlias( "AnotherNumberPropertyAliasTwo" )]
			public int YetAnotherNumberField;
		}


		[Test]
		public void LoadAlias()
		{
			const string json = "{ \"numberFieldAlias\" : 1, \"NumberPropertyAlias\" : 2, \"anotherNumberFieldAliasOne\" : 3, \"anotherNumberFieldAliasTwo\" : 4, \"AnotherNumberPropertyAliasOne\" : 5, \"AnotherNumberPropertyAliasTwo\" : 6 }";
			var aliasData = Json.FromJson<AliasData>( json );

			Assert.AreEqual( 1, aliasData.NumberField );
			Assert.AreEqual( 2, aliasData.NumberProperty );
			Assert.IsTrue( aliasData.AnotherNumberField == 3 || aliasData.AnotherNumberField == 4 );
			Assert.IsTrue( aliasData.YetAnotherNumberField == 5 || aliasData.YetAnotherNumberField == 6 );
		}
	}
}
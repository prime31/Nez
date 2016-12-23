using FarseerPhysics.Dynamics;


namespace FarseerPhysics.Common.PhysicsLogic
{
	/// <summary>
	/// Contains filter data that can determine whether an object should be processed or not.
	/// </summary>
	public abstract class FilterData
	{
		/// <summary>
		/// Disable the logic on specific categories.
		/// Category.None by default.
		/// </summary>
		public Category disabledOnCategories = Category.None;

		/// <summary>
		/// Disable the logic on specific groups
		/// </summary>
		public int disabledOnGroup;

		/// <summary>
		/// Enable the logic on specific categories
		/// Category.All by default.
		/// </summary>
		public Category enabledOnCategories = Category.All;

		/// <summary>
		/// Enable the logic on specific groups.
		/// </summary>
		public int enabledOnGroup;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="body"></param>
		/// <returns></returns>
		public virtual bool isActiveOn( Body body )
		{
			if( body == null || !body.enabled || body.isStatic )
				return false;

			if( body.fixtureList == null )
				return false;

			foreach( var fixture in body.fixtureList )
			{
				//Disable
				if( ( fixture.collisionGroup == disabledOnGroup ) && fixture.collisionGroup != 0 && disabledOnGroup != 0 )
					return false;

				if( ( fixture.collisionCategories & disabledOnCategories ) != Category.None )
					return false;

				if( enabledOnGroup != 0 || enabledOnCategories != Category.All )
				{
					//Enable
					if( ( fixture.collisionGroup == enabledOnGroup ) && fixture.collisionGroup != 0 && enabledOnGroup != 0 )
						return true;

					if( ( fixture.collisionCategories & enabledOnCategories ) != Category.None &&
						enabledOnCategories != Category.All )
						return true;
				}
				else
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Adds the category.
		/// </summary>
		/// <param name="category">The category.</param>
		public void addDisabledCategory( Category category )
		{
			disabledOnCategories |= category;
		}

		/// <summary>
		/// Removes the category.
		/// </summary>
		/// <param name="category">The category.</param>
		public void removeDisabledCategory( Category category )
		{
			disabledOnCategories &= ~category;
		}

		/// <summary>
		/// Determines whether this body ignores the the specified controller.
		/// </summary>
		/// <param name="category">The category.</param>
		/// <returns>
		/// 	<c>true</c> if the object has the specified category; otherwise, <c>false</c>.
		/// </returns>
		public bool isInDisabledCategory( Category category )
		{
			return ( disabledOnCategories & category ) == category;
		}

		/// <summary>
		/// Adds the category.
		/// </summary>
		/// <param name="category">The category.</param>
		public void addEnabledCategory( Category category )
		{
			enabledOnCategories |= category;
		}

		/// <summary>
		/// Removes the category.
		/// </summary>
		/// <param name="category">The category.</param>
		public void removeEnabledCategory( Category category )
		{
			enabledOnCategories &= ~category;
		}

		/// <summary>
		/// Determines whether this body ignores the the specified controller.
		/// </summary>
		/// <param name="category">The category.</param>
		/// <returns>
		/// 	<c>true</c> if the object has the specified category; otherwise, <c>false</c>.
		/// </returns>
		public bool isInEnabledInCategory( Category category )
		{
			return ( enabledOnCategories & category ) == category;
		}

	}
}
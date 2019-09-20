namespace Nez.UI
{
	/// <summary>
	/// Provides methods for an element to participate in layout and to provide a minimum, preferred, and maximum size.
	/// </summary>
	interface ILayout
	{
		/// <summary>
		/// If true, this element will be sized to the parent in {@link #validate()}. If the parent is the stage, the element will be sized 
		/// to the stage. This method is for convenience only when the widget's parent does not set the size of its children (such as
		/// the stage).
		/// </summary>
		/// <value><c>true</c> if fill parent; otherwise, <c>false</c>.</value>
		bool FillParent { get; set; }

		/// <summary>
		/// Enables or disables the layout for this element and all child elements, recursively. When false, {@link #validate()} will not
		/// cause a layout to occur. This can be useful when an element will be manipulated externally, such as with actions.
		/// Default is true.
		/// </summary>
		/// <value><c>true</c> if layout enabled; otherwise, <c>false</c>.</value>
		bool LayoutEnabled { get; set; }

		float MinWidth { get; }

		float MinHeight { get; }

		float PreferredWidth { get; }

		float PreferredHeight { get; }

		/// <summary>
		/// Zero indicates no max width
		/// </summary>
		/// <value>The width of the max.</value>
		float MaxWidth { get; }

		/// <summary>
		/// Zero indicates no max height
		/// </summary>
		/// <value>The height of the max.</value>
		float MaxHeight { get; }

		/// <summary>
		/// Computes and caches any information needed for drawing and, if this element has children, positions and sizes each child, 
		/// calls {@link #invalidate()} any each child whose width or height has changed, and calls {@link #validate()} on each child.
		/// This method should almost never be called directly, instead {@link #validate()} should be used
		/// </summary>
		void Layout();

		/// <summary>
		/// Invalidates this element's layout, causing {@link #layout()} to happen the next time {@link #validate()} is called. This
		/// method should be called when state changes in the element that requires a layout but does not change the minimum, preferred,
		/// maximum, or actual size of the element (meaning it does not affect the parent element's layout).
		/// </summary>
		void Invalidate();

		/// <summary>
		/// Invalidates this element and all its parents, calling {@link #invalidate()} on each. This method should be called when state
		/// changes in the element that affects the minimum, preferred, maximum, or actual size of the element (meaning it it potentially
		/// affects the parent element's layout).
		/// </summary>
		void InvalidateHierarchy();

		/// <summary>
		/// Ensures the element has been laid out. Calls {@link #layout()} if {@link #invalidate()} has been called since the last time
		/// {@link #validate()} was called, or if the element otherwise needs to be laid out. This method is usually called in
		/// {@link element#draw(Batch, float)} before drawing is performed.
		/// </summary>
		void Validate();

		/// <summary>
		/// Sizes this element to its preferred width and height, then calls {@link #validate()}.
		/// Generally this method should not be called in an element's constructor because it calls {@link #layout()}, which means a
		/// subclass would have layout() called before the subclass' constructor. Instead, in constructors simply set the element's size
		/// to {@link #getPrefWidth()} and {@link #getPrefHeight()}. This allows the element to have a size at construction time for more
		/// convenient use with groups that do not layout their children.
		/// </summary>
		void Pack();
	}
}
namespace Nez.AI.BehaviorTrees
{
	/// <summary>
	/// interface used just to identify if a Behavior is a conditional. it will always be applied to a Behavior which already has the update method.
	/// </summary>
	public interface IConditional<T>
	{
		TaskStatus Update(T context);
	}
}
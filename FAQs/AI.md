AI
==========
Nez includes several different options for setting up AI ranging from a super simple transition-less finite state machine (FSM) to extendable behavior trees. You can mix and match them as you see fit.


SimpleStateMachine
==========
There


StateMachine
==========
Each



Behavior Tree
==========
There


# Composites
Composites are parent nodes in a behavior tree. They house 1 or more children and execute them in different ways.

- **Sequence<T>:** returns failure as soon as one of its children returns failure. If a child returns success it will sequentially run the next child in the next tick of the tree.
- **Selector<T>:** return success as soon as one of its child tasks return success. If a child task returns failure then it will sequentially run the next child in the next tick.
- **Parallel<T>:** runs each child until a child returns failure. It differs from `Sequence` only in that it runs all children every tick
- **ParallelSelector<T>:** loads
- **RandomSelector<T>:** loads
- **RandomSequence<T>:** loads


# Conditionals
Conditionals are binary success/failure nodes. They are identified by the IConditional interface. They check some condition of your game world and either return success or failure. These are inherently game specific so Nez only provides a single generic Conditional out of the box and a helper Conditional that wraps an Action so you can avoid having to make a separate class for each Conditional.

- **RandomProbability<T>:** return success when the random probability is above the specified success probability
- **ExecuteActionConditional<T>:** wraps a Func and executes it as the Conditional. Useful for prototyping and to avoid creating separate classes for simple Conditionals.


# Decorators
Decorators are wrapper tasks that have a single child. They can modify the behavior of the child task in various ways such as inverting the result, running it until failure, etc.

- **AlwaysFail<T>:** always returns failure regardless of the child result
- **AlwaysSucceed<T>:** always returns success regardless of the child result
- **ConditionalDecorator<T>:** loads
- **Inverter<T>:** loads
- **Repeater<T>:** loads
- **UntilFail<T>:** loads
- **UntilSuccess<T>:** loads


# Actions
Actions are the leaf nodes of the behavior tree. This is where stuff happens such as playing an animation, triggering an event, etc.

- **ExecuteAction<T>:** wraps a Func and executes it as its action. Useful for prototyping and to avoid creating separate classes for simple Actions.
- **WaitAction<T>:** waits a specified amount of time
- **LogAction<T>:** logs a string to the console. Useful for debugging.
- **BehaviorTreeReference<T>:** runs another BehaviorTree<T>



Utility AI
==========
Thre


# Selector
Selects the best Qualifier from the Qualifiers attached to the Selector


# Qualifier
Calculates a score that represents the utility/usefulness of its associated action.


# Scorer
A method for calculating scores that can be reused across Qualifiers.


# Action


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GOAP_Agent : MonoBehaviour
{
	HashSet<GOAP_Action> availableActions;
	Queue<GOAP_Action> currentActions;

	IGOAP dataLink;
	GOAP_Planner planner;

	FSM stateMachine;
	FSM.FSMState idleState;
	FSM.FSMState moveState;
	FSM.FSMState performState;


	void Start()
	{
		availableActions = new HashSet<GOAP_Action>();
		currentActions = new Queue<GOAP_Action>();
		planner = new GOAP_Planner();
		stateMachine = new FSM();

		InitializeDataLink();
		InitializeActions();

		CreateIdleState();
		CreateMoveState();
		CreatePerformState();

		stateMachine.pushState(idleState);
	}

	void Update()
	{
		stateMachine.Update(gameObject);
	}


	void InitializeDataLink()
	{
		foreach (Component component in gameObject.GetComponents(typeof(Component)))
		{
			if (typeof(IGOAP).IsAssignableFrom(component.GetType()))
			{
				dataLink = (IGOAP)component;
				return;
			}
		}
	}

	private void InitializeActions()
	{
		GOAP_Action[] actions = gameObject.GetComponents<GOAP_Action>();
		foreach (GOAP_Action action in actions)
		{
			availableActions.Add(action);
		}
	}

	#region FSM

	private void CreateIdleState()
	{
		idleState = (fsm, gameObj) =>
		{
			// GOAP planning

			// get the world state and the goal we want to plan for
			HashSet<KeyValuePair<string, object>> worldState = dataLink.GetWorldState();
			HashSet<KeyValuePair<string, object>> goal = dataLink.CreateGoalState();

			// Plan
			Queue<GOAP_Action> plan = planner.Plan(gameObject, availableActions, worldState, goal);
			if (plan != null)
			{
				// we have a plan, hooray!
				currentActions = plan;
				dataLink.PlanFound(goal, plan);

				fsm.popState(); // move to PerformAction state
				fsm.pushState(performState);

			}
			else
			{
				// ugh, we couldn't get a plan
				Debug.Log("<color=orange>Failed Plan:</color>");
				dataLink.PlanFailed(goal);
				fsm.popState(); // move back to IdleAction state
				fsm.pushState(idleState);
			}
		};
	}

	private void CreateMoveState()
	{
		moveState = (fsm, gameObj) =>
		{
			// move the game object

			GOAP_Action action = currentActions.Peek();
			if (action.RequiresInRange() && action.target == null)
			{
				Debug.Log("<color=red>Fatal error:</color> Action requires a target but has none. Planning failed. You did not assign the target in your Action.CanBePerformed()");
				fsm.popState(); // move
				fsm.popState(); // perform
				fsm.pushState(idleState);
				return;
			}

			// get the agent to move itself
			if (dataLink.MoveAgent(action))
			{
				fsm.popState();
			}
		};
	}

	private void CreatePerformState()
	{
		performState = (fsm, gameObj) =>
		{
			// perform the action

			if (HasActionPlan == false)
			{
				// no actions to perform
				Debug.Log("<color=red>Done actions</color>");
				fsm.popState();
				fsm.pushState(idleState);
				dataLink.PlanFinished();
				return;
			}

			GOAP_Action action = currentActions.Peek();
			if (action.IsDone())
			{
				// the action is done. Remove it so we can perform the next one
				currentActions.Dequeue();
			}

			if (HasActionPlan)
			{
				// perform the next action
				action = currentActions.Peek();
				bool inRange = action.RequiresInRange() ? action.isInRange : true;

				if (inRange)
				{
					// we are in range, so perform the action
					bool success = action.Perform(gameObj);

					if (!success)
					{
						// action failed, we need to plan again
						fsm.popState();
						fsm.pushState(idleState);
						dataLink.PlanAborted(action);
					}
				}
				else
				{
					// we need to move there first
					// push moveTo state
					fsm.pushState(moveState);
				}

			}
			else
			{
				// no actions left, move to Plan state
				fsm.popState();
				fsm.pushState(idleState);
				dataLink.PlanFinished();
			}

		};
	}

	#endregion

	public void AddAction(GOAP_Action action)
	{
		availableActions.Add(action);
	}

	public GOAP_Action GetAction()
	{
		foreach (GOAP_Action action in availableActions)
		{
			if (action.GetType().Equals(action)) { return action; }
		}
		return null;
	}

	public void RemoveAction(GOAP_Action action)
	{
		availableActions.Remove(action);
	}

	bool HasActionPlan
	{
		get { return currentActions.Count > 0; }
	}


}

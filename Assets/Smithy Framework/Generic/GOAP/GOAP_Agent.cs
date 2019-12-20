using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GOAP_Agent : MonoBehaviour
{
	HashSet<GOAP_Action> availableActions;
	Queue<GOAP_Action> currentActions;

	GOAP_Planner planner;

	FSM stateMachine;
	FSM.FSMState idleState;
	FSM.FSMState moveState;
	FSM.FSMState performState;


	void Start()
	{
		Initialize();
	}

	void Update()
	{
		UpdateStateMachine();
	}


	public void Initialize()
	{
		availableActions = new HashSet<GOAP_Action>();
		currentActions = new Queue<GOAP_Action>();
		planner = new GOAP_Planner();
		stateMachine = new FSM();

		InitializeActions();

		CreateIdleState();
		CreateMoveState();
		CreatePerformState();

		stateMachine.pushState(idleState);
	}

	public void UpdateStateMachine()
	{
		stateMachine.Update(gameObject);
	}

	#region Actions

	void InitializeActions()
	{
		GOAP_Action[] actions = gameObject.GetComponents<GOAP_Action>();
		foreach (GOAP_Action action in actions)
		{
			availableActions.Add(action);
		}
	}

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

	#endregion

	#region FSM

	void CreateIdleState()
	{
		idleState = (fsm, gameObj) =>
		{
			// GOAP planning

			// get the world state and the goal we want to plan for
			HashSet<KeyValuePair<string, object>> worldState = GetWorldState();
			HashSet<KeyValuePair<string, object>> goal = CreateGoalState();

			// Plan
			Queue<GOAP_Action> plan = planner.Plan(gameObject, availableActions, worldState, goal);
			if (plan != null)
			{
				// we have a plan, hooray!
				currentActions = plan;
				PlanFound(goal, plan);

				fsm.popState(); // move to PerformAction state
				fsm.pushState(performState);

			}
			else
			{
				// ugh, we couldn't get a plan
				Debug.Log("<color=orange>Failed Plan:</color>");
				PlanFailed(goal);
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
				Debug.Log("<color=red>Fatal error:</color> Action requires a target but has none. Planning failed. You did not assign the target in your Action.checkProceduralPrecondition()");
				fsm.popState(); // move
				fsm.popState(); // perform
				fsm.pushState(idleState);
				return;
			}

			// get the agent to move itself
			if (MoveAgent(action))
			{
				fsm.popState();
			}

			/*MovableComponent movable = (MovableComponent) gameObj.GetComponent(typeof(MovableComponent));
			if (movable == null) {
				Debug.Log("<color=red>Fatal error:</color> Trying to move an Agent that doesn't have a MovableComponent. Please give it one.");
				fsm.popState(); // move
				fsm.popState(); // perform
				fsm.pushState(idleState);
				return;
			}

			float step = movable.moveSpeed * Time.deltaTime;
			gameObj.transform.position = Vector3.MoveTowards(gameObj.transform.position, action.target.transform.position, step);

			if (gameObj.transform.position.Equals(action.target.transform.position) ) {
				// we are at the target location, we are done
				action.setInRange(true);
				fsm.popState();
			}*/
		};
	}

	void CreatePerformState()
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
				PlanFinished();
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

				if (inRange /* action.CanBePerformed(gameObj) */)
				{
					// we are in range, so perform the action
					bool success = action.Perform(gameObj);

					if (!success)
					{
						// action failed, we need to plan again
						fsm.popState();
						fsm.pushState(idleState);
						PlanAborted(action);
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
				PlanFinished();
			}

		};
	}

	#endregion

	#region Interface

	/// <summary>
	/// The starting state of the Agent and the world. Supply what states are needed for actions to run.
	/// </summary>
	public abstract HashSet<KeyValuePair<string, object>> GetWorldState();

	/// <summary>
	/// Give the planner a new goal so it can figure out the actions needed to fulfill it.
	/// </summary>
	public abstract HashSet<KeyValuePair<string, object>> CreateGoalState();

	/// <summary>
	/// No sequence of actions could be found for the supplied goal. You will need to try another goal.
	/// </summary>
	public abstract void PlanFailed(HashSet<KeyValuePair<string, object>> failedGoal);

	/// <summary>
	/// A plan was found for the supplied goal.  These are the actions the Agent will perform, in order.
	/// </summary>
	public abstract void PlanFound(HashSet<KeyValuePair<string, object>> goal, Queue<GOAP_Action> actions);

	/// <summary>
	/// All actions are complete and the goal was reached. Hooray!
	/// </summary>
	public abstract void PlanFinished();

	/// <summary>
	/// One of the actions caused the plan to abort. That action is returned.
	/// </summary>
	public abstract void PlanAborted(GOAP_Action aborter);


	public abstract bool MoveAgent(GOAP_Action nextAction);

	#endregion
}

using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Any agent that wants to use GOAP must implement
/// this interface. It provides information to the GOAP
/// planner so it can plan what actions to use.
/// 
/// It also provides an interface for the planner to give 
/// feedback to the Agent and report success/failure.
/// </summary>
public interface IGOAP
{
	/// <summary>
	/// The starting state of the Agent and the world.
	/// Supply what states are needed for actions to run.
	/// </summary>
	HashSet<KeyValuePair<string, object>> GetWorldState();

	/// <summary>
	/// Give the planner a new goal so it can figure out 
	/// the actions needed to fulfill it.
	/// </summary>
	HashSet<KeyValuePair<string, object>> CreateGoalState();

	/// <summary>
	/// No sequence of actions could be found for the supplied goal.
	/// You will need to try another goal
	/// </summary>
	void PlanFailed(HashSet<KeyValuePair<string, object>> failedGoal);

	/// <summary>
	/// A plan was found for the supplied goal.
	/// These are the actions the Agent will perform, in order.
	/// </summary>
	void PlanFound(HashSet<KeyValuePair<string, object>> goal, Queue<GOAP_Action> actions);

	/// <summary>
	/// All actions are complete and the goal was reached. Hooray!
	/// </summary>
	void PlanFinished();

	/// <summary>
	/// One of the actions caused the plan to abort.
	/// That action is returned.
	/// </summary>
	void PlanAborted(GOAP_Action aborter);

	/// <summary>
	/// Called during Update. Move the agent towards the target in order
	/// for the next action to be able to perform.
	/// Return true if the Agent is at the target and the next action can perform.
	/// False if it is not there yet.
	/// </summary>
	bool MoveAgent(GOAP_Action nextAction);
}


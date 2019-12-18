using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Baker : GOAP_Agent
{
	public Backpack backpack;
	public float moveSpeed = 1;

	void Start()
	{
		Initialize();
		backpack = GetComponent<Backpack>();
	}

	void Update()
	{
		UpdateStateMachine();
	}


	public override HashSet<KeyValuePair<string, object>> CreateGoalState()
	{
		HashSet<KeyValuePair<string, object>> goal = new HashSet<KeyValuePair<string, object>>();

		goal.Add(new KeyValuePair<string, object>("flour", true));
		return goal;
	}

	public override HashSet<KeyValuePair<string, object>> GetWorldState()
	{
		HashSet<KeyValuePair<string, object>> worldData = new HashSet<KeyValuePair<string, object>>();

		worldData.Add(new KeyValuePair<string, object>("flour", backpack.flour));
		worldData.Add(new KeyValuePair<string, object>("bread", backpack.bread));

		return worldData;
	}

	public override void PlanFailed(HashSet<KeyValuePair<string, object>> failedGoal)
	{

	}

	public override void PlanFound(HashSet<KeyValuePair<string, object>> goal, Queue<GOAP_Action> actions)
	{

	}

	public override void PlanFinished()
	{

	}

	public override void PlanAborted(GOAP_Action aborter)
	{

	}

	public override bool MoveAgent(GOAP_Action nextAction)
	{
		// move towards the NextAction's target
		float step = moveSpeed * Time.deltaTime;
		gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, nextAction.target.transform.position, step);

		if (gameObject.transform.position.Equals(nextAction.target.transform.position))
		{
			// we are at the target location, we are done
			nextAction.isInRange = true;
			return true;
		}
		else
			return false;
	}
}

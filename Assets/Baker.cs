using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Baker : MonoBehaviour, IGOAP
{
	public Backpack backpack;
	public float moveSpeed = 1;

	void Start()
	{
		backpack = GetComponent<Backpack>();
	}


	public HashSet<KeyValuePair<string, object>> CreateGoalState()
	{
		HashSet<KeyValuePair<string, object>> goal = new HashSet<KeyValuePair<string, object>>();

		goal.Add(new KeyValuePair<string, object>("flour", true));
		return goal;
	}

	public HashSet<KeyValuePair<string, object>> GetWorldState()
	{
		HashSet<KeyValuePair<string, object>> worldData = new HashSet<KeyValuePair<string, object>>();

		worldData.Add(new KeyValuePair<string, object>("flour", backpack.flour));
		worldData.Add(new KeyValuePair<string, object>("bread", backpack.bread));

		return worldData;
	}

	public void PlanFailed(HashSet<KeyValuePair<string, object>> failedGoal)
	{

	}

	public void PlanFound(HashSet<KeyValuePair<string, object>> goal, Queue<GOAP_Action> actions)
	{

	}

	public void PlanFinished()
	{

	}

	public void PlanAborted(GOAP_Action aborter)
	{

	}

	public bool MoveAgent(GOAP_Action nextAction)
	{
		float step = moveSpeed * Time.deltaTime;
		transform.position = Vector3.MoveTowards(transform.position, nextAction.target.transform.position, step);

		if (transform.position.Equals(nextAction.target.transform.position))
		{
			// we are at the target location, we are done
			nextAction.isInRange = true;
			return true;
		}
		else
			return false;
	}
}

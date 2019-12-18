using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectWheat : GOAP_Action
{
	public GameObject actionTarget;

	public CollectWheat()
	{
		SetResult("flour", true);
	}

	public override void ResetProperties()
	{

	}

	public override bool RequiresInRange()
	{
		return true;
	}

	public override bool CanBePerformed(GameObject agent)
	{
		Backpack backpack = agent.GetComponent<Backpack>();
		target = actionTarget;
		return (target != null);
	}

	public override bool Perform(GameObject agent)
	{
		Backpack backpack = agent.GetComponent<Backpack>();
		backpack.flour += 1;
		return true;
	}

	public override bool IsDone()
	{
		return false;
	}
}

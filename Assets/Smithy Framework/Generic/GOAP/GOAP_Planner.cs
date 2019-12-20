using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Plans what actions can be completed in order to fulfill a goal state.</summary>
public class GOAP_Planner
{

	/// <summary>
	/// Plan what sequence of actions can fulfill the goal. 
	/// Returns null if a plan could not be found, or a list of the actions that must be performed, in order, to fulfill the goal.
	/// </summary>
	public Queue<GOAP_Action> Plan(GameObject agent, HashSet<GOAP_Action> availableActions, HashSet<KeyValuePair<string, object>> worldState, HashSet<KeyValuePair<string, object>> goal)
	{
		// reset the actions so we can start fresh with them
		foreach (GOAP_Action action in availableActions) { action.ResetAction(); }

		// check what actions can run using their checkProceduralPrecondition
		HashSet<GOAP_Action> usableActions = new HashSet<GOAP_Action>();
		foreach (GOAP_Action action in availableActions)
		{
			if (action.CanBePerformed(agent)) { usableActions.Add(action); }
		}

		// build up the tree and record the leaf nodes that provide a solution to the goal.
		List<Node> leaves = new List<Node>();

		// build graph
		Node start = new Node(null, 0, worldState, null);
		bool success = BuildGraph(start, leaves, usableActions, goal);

		if (success == false)
		{
			// oh no, we didn't get a plan
			Debug.Log("NO PLAN", agent);
			return null;
		}

		// get the cheapest leaf
		Node cheapest = null;
		foreach (Node leaf in leaves)
		{
			if (cheapest == null)
			{
				cheapest = leaf;
			}
			else
			{
				if (leaf.runningCost < cheapest.runningCost) { cheapest = leaf; }
			}
		}

		// get its node and work back through the parents
		List<GOAP_Action> result = new List<GOAP_Action>();
		Node node = cheapest;
		while (node != null)
		{
			if (node.action != null)
			{
				result.Insert(0, node.action); // insert the action in the front
			}
			node = node.parent;
		}
		// we now have this action list in correct order

		Queue<GOAP_Action> queue = new Queue<GOAP_Action>();
		foreach (GOAP_Action a in result)
		{
			queue.Enqueue(a);
		}

		// hooray we have a plan!
		return queue;
	}

	/// <summary>
	/// Returns true if at least one solution was found.
	/// The possible paths are stored in the leaves list. 
	/// Each leaf has a 'runningCost' value where the lowest cost will be the best action sequence.
	/// </summary>
	private bool BuildGraph(Node parent, List<Node> leaves, HashSet<GOAP_Action> usableActions, HashSet<KeyValuePair<string, object>> goal)
	{
		bool foundOne = false;

		// go through each action available at this node and see if we can use it here
		foreach (GOAP_Action action in usableActions)
		{

			// if the parent state has the conditions for this action's preconditions, we can use it here
			if (InState(action.conditions, parent.state))
			{
				// apply the action's effects to the parent state
				HashSet<KeyValuePair<string, object>> currentState = PopulateState(parent.state, action.results);
				Node node = new Node(parent, parent.runningCost + action.cost, currentState, action);

				if (InState(goal, currentState))
				{
					// we found a solution!
					leaves.Add(node);
					foundOne = true;
				}
				else
				{
					// not at a solution yet, so test all the remaining actions and branch out the tree
					HashSet<GOAP_Action> subset = ActionSubset(usableActions, action);
					bool found = BuildGraph(node, leaves, subset, goal);
					if (found) { foundOne = true; }
				}
			}
		}

		return foundOne;
	}

	/// <summary> Create a subset of the actions excluding the removeMe one. Creates a new set. </summary>
	private HashSet<GOAP_Action> ActionSubset(HashSet<GOAP_Action> actions, GOAP_Action removeMe)
	{
		HashSet<GOAP_Action> subset = new HashSet<GOAP_Action>();
		foreach (GOAP_Action action in actions)
		{
			if (action.Equals(removeMe) == false) { subset.Add(action); }
		}
		return subset;
	}

	/// <summary>
	/// Check that all items in 'test' are in 'state'. If just one does not match or is not there
	/// then this returns false.
	/// </summary>
	private bool InState(HashSet<KeyValuePair<string, object>> testConditions, HashSet<KeyValuePair<string, object>> stateConditions)
	{
		bool allMatch = true;
		foreach (KeyValuePair<string, object> test in testConditions)
		{
			bool match = false;
			foreach (KeyValuePair<string, object> state in stateConditions)
			{
				if (state.Equals(test))
				{
					match = true;
					break;
				}
			}
			if (match == false) { allMatch = false; }
		}
		return allMatch;
	}

	/// <summary>
	/// Apply the stateChange to the currentState
	/// </summary>
	private HashSet<KeyValuePair<string, object>> PopulateState(HashSet<KeyValuePair<string, object>> currentState, HashSet<KeyValuePair<string, object>> stateChange)
	{
		HashSet<KeyValuePair<string, object>> state = new HashSet<KeyValuePair<string, object>>();
		// copy the KVPs over as new objects
		foreach (KeyValuePair<string, object> s in currentState)
		{
			state.Add(new KeyValuePair<string, object>(s.Key, s.Value));
		}

		foreach (KeyValuePair<string, object> change in stateChange)
		{
			// if the key exists in the current state, update the Value
			bool exists = false;

			foreach (KeyValuePair<string, object> s in state)
			{
				if (s.Equals(change))
				{
					exists = true;
					break;
				}
			}

			if (exists)
			{
				state.RemoveWhere((KeyValuePair<string, object> kvp) => { return kvp.Key.Equals(change.Key); });
				KeyValuePair<string, object> updated = new KeyValuePair<string, object>(change.Key, change.Value);
				state.Add(updated);
			}
			else // if it does not exist in the current state, add it
			{
				state.Add(new KeyValuePair<string, object>(change.Key, change.Value));
			}
		}
		return state;
	}

	/// <summary>
	/// Used for building up the graph and holding the running costs of actions.
	/// </summary>
	private class Node
	{
		public Node parent;
		public float runningCost;
		public HashSet<KeyValuePair<string, object>> state;
		public GOAP_Action action;

		public Node(Node parent, float runningCost, HashSet<KeyValuePair<string, object>> state, GOAP_Action action)
		{
			this.parent = parent;
			this.runningCost = runningCost;
			this.state = state;
			this.action = action;
		}
	}
}



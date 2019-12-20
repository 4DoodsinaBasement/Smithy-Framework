using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GOAP_Action : MonoBehaviour
{
	private HashSet<KeyValuePair<string, object>> _conditions; public HashSet<KeyValuePair<string, object>> conditions { get { return _conditions; } }
	private HashSet<KeyValuePair<string, object>> _results; public HashSet<KeyValuePair<string, object>> results { get { return _results; } }

	public float cost = 1;

	public bool isInRange = false;
	public GameObject target = null;

	public GOAP_Action()
	{
		_conditions = new HashSet<KeyValuePair<string, object>>();
		_results = new HashSet<KeyValuePair<string, object>>();
	}

	/// <summary>Reset the action's variables.</summary>
	public void ResetAction()
	{
		isInRange = false;
		target = null;
		ResetProperties();
	}

	/// <summary>IIO: Reset the action's variables before planning happens again.</summary>
	public abstract void ResetProperties();

	/// <summary>IIO: Return true if the agent must be in range to perform the action.</summary>
	public abstract bool RequiresInRange();

	/// <summary>IIO: Return true if the action can be performed.</summary>
	public abstract bool CanBePerformed(GameObject agent);

	/// <summary>IIO: Run the action. Return true if the action succeeded.</summary>
	public abstract bool Perform(GameObject agent);

	/// <summary>IIO: Return true if the action is finished.</summary>
	public abstract bool IsDone();

	public void SetCondition(string key, object value)
	{
		_conditions.Add(new KeyValuePair<string, object>(key, value));
	}

	public void RemoveCondition(string key)
	{
		KeyValuePair<string, object> remove = default(KeyValuePair<string, object>);

		foreach (KeyValuePair<string, object> kvp in _conditions)
		{
			if (kvp.Key.Equals(key)) { remove = kvp; }
		}
		if (!default(KeyValuePair<string, object>).Equals(remove)) { _conditions.Remove(remove); }
	}

	public void SetResult(string key, object value)
	{
		_results.Add(new KeyValuePair<string, object>(key, value));
	}

	public void RemoveResult(string key)
	{
		KeyValuePair<string, object> remove = default(KeyValuePair<string, object>);

		foreach (KeyValuePair<string, object> kvp in _results)
		{
			if (kvp.Key.Equals(key)) { remove = kvp; }
		}
		if (!default(KeyValuePair<string, object>).Equals(remove)) { _results.Remove(remove); }
	}
}

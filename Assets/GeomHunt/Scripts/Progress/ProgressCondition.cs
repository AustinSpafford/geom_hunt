using UnityEngine;
using System.Collections;

public struct ProgressConditionChangedMessageArgs
{
	public ProgressCondition Sender;

	public bool ConditionIsTrue;
}

public class ProgressCondition : MonoBehaviour
{
	public enum ConditionComparitor
	{
		Equal,
		NotEqual,
		GreaterThan,
		GreaterThanOrEqual,
		LessThan,
		LessThanOrEqual,
	}

	public enum ConditionConcatenator
	{
		And,
		Or,
	}

	[System.Serializable]
	public struct ConditionClause
	{
		public string ProgressName;
		public ConditionComparitor Comparitor;
		public float ProgressValue;
		public ConditionConcatenator Concatenator;
	}

	public ConditionClause[] ConditionClauses = new ConditionClause[0];

	public bool ConditionIsTrue { get; private set; }

	public bool DebugEnabled = false;

	public void Awake()
	{
		ConditionIsTrue = true;
	}

	public void Start()
	{
		ConditionIsTrue = ComputeProgressConditionEvaluation();
	}
	
	public void OnEnable()
	{
		UpdateConditionEvaluation();

		ProgressStorage.ProgressChanged += OnProgressChanged;
	}
	
	public void OnDisable()
	{
		ProgressStorage.ProgressChanged -= OnProgressChanged;
	}

	private void OnProgressChanged(
		object sender,
		ProgressChangedEventArgs eventArgs)
	{
		bool changeRelatesToProgressCondition = false;

		foreach (ConditionClause clause in ConditionClauses)
		{
			if (clause.ProgressName == eventArgs.ProgressName)
			{
				changeRelatesToProgressCondition = true;

				break;
			}
		}

		if (changeRelatesToProgressCondition)
		{
			if (DebugEnabled)
			{
				Debug.LogFormat(
					"Triggering a progress-condition reevaluation due to progress [{0}] changing.",
					eventArgs.ProgressName);
			}

			UpdateConditionEvaluation();
		}
	}

	private bool ComputeProgressConditionEvaluation()
	{
		bool currentConditionEvaluation = true;
	
		// NOTE: Empty-conditions (no clauses) intentionally evaluate to true.	
		if (ConditionClauses.Length > 0)
		{
			ConditionConcatenator lastConcatenator = ConditionConcatenator.And;

			ProgressStorage progressStorage = ProgressStorage.LocalPlayerProgressStorage;

			foreach (ConditionClause clause in ConditionClauses)
			{
				if (string.IsNullOrEmpty(clause.ProgressName))
				{
					throw new System.InvalidOperationException("Condition-clause rendered invalid by a missing progress-name.");
				}

				float progressValue = 
					progressStorage.GetProgressValueAsFloat(clause.ProgressName);

				bool progressValuesAreEqual = 
					Mathf.Approximately(progressValue, clause.ProgressValue);

				bool clauseEvaluation;

				switch (clause.Comparitor)
				{
					case ConditionComparitor.Equal:
						clauseEvaluation = progressValuesAreEqual;
						break;

					case ConditionComparitor.NotEqual:
						clauseEvaluation = (progressValuesAreEqual == false);
						break;

					case ConditionComparitor.GreaterThan:
						clauseEvaluation = (
							(progressValue > clause.ProgressValue) &&
							(progressValuesAreEqual == false));
						break;

					case ConditionComparitor.GreaterThanOrEqual:
						clauseEvaluation = (
							(progressValue > clause.ProgressValue) ||
							progressValuesAreEqual);
						break;

					case ConditionComparitor.LessThan:
						clauseEvaluation = (
							(progressValue < clause.ProgressValue) &&
							(progressValuesAreEqual == false));
						break;

					case ConditionComparitor.LessThanOrEqual:
						clauseEvaluation = (
							(progressValue < clause.ProgressValue) ||
							progressValuesAreEqual);
						break;

					default:
						throw new System.ComponentModel.InvalidEnumArgumentException(clause.Comparitor.ToString());
				}

				// Concatenate the current clause onto the last clause's results.
				switch (lastConcatenator)
				{
					case ConditionConcatenator.And:
						currentConditionEvaluation = (currentConditionEvaluation && clauseEvaluation);
						break;

					case ConditionConcatenator.Or:
						currentConditionEvaluation = (currentConditionEvaluation || clauseEvaluation);
						break;

					default:
						throw new System.ComponentModel.InvalidEnumArgumentException(lastConcatenator.ToString());
				}

				lastConcatenator = clause.Concatenator;
			}
		}

		return currentConditionEvaluation;
	}

	private void UpdateConditionEvaluation()
	{
		bool conditionWasTrue = ConditionIsTrue;
			
		ConditionIsTrue = ComputeProgressConditionEvaluation();
		
		if (conditionWasTrue != ConditionIsTrue)
		{
			var messageArgs = new ProgressConditionChangedMessageArgs()
			{
				ConditionIsTrue = ConditionIsTrue,
			};

			SendMessage(
				"OnProgressConditionChanged", 
				messageArgs,
				SendMessageOptions.DontRequireReceiver);
		}
	}
}

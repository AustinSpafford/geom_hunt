using UnityEngine;
using System.Collections;

public class ActivateObjectOnProgressCondition : MonoBehaviour
{
	public enum ProgressConditionComparitor
	{
		Equal,
		NotEqual,
		GreaterThan,
		GreaterThanOrEqual,
		LessThan,
		LessThanOrEqual,
	}

	public enum ProgressConditionConcatenator
	{
		And,
		Or,
	}

	[System.Serializable]
	public struct ProgressConditionClause
	{
		public string ProgressName;
		public ProgressConditionComparitor Comparitor;
		public float ProgressValue;
		public ProgressConditionConcatenator Concatenator;
	}

	public GameObject TargetObject = null;

	public ProgressConditionClause[] ProgressCondition = new ProgressConditionClause[0];

	public bool DebugEnabled = false;

	public void Start()
	{
		UpdateTargetObjectActivation();
	}
	
	public void OnEnable()
	{
		UpdateTargetObjectActivation();

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

		foreach (ProgressConditionClause clause in ProgressCondition)
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
					"Progress-change detected that might affect [{0}]. Updating activation.",
					TargetObject.name);
			}

			UpdateTargetObjectActivation();
		}
	}

	private bool ComputeProgressConditionEvaluation()
	{
		bool currentConditionEvaluation = true;
		
		ProgressConditionConcatenator lastConcatenator = ProgressConditionConcatenator.And;

		ProgressStorage progressStorage = ProgressStorage.LocalPlayerProgressStorage;

		foreach (ProgressConditionClause clause in ProgressCondition)
		{
			float progressValue = 
				progressStorage.GetProgressValueAsFloat(clause.ProgressName);

			bool progressValuesAreEqual = 
				Mathf.Approximately(progressValue, clause.ProgressValue);

			bool clauseEvaluation;

			switch (clause.Comparitor)
			{
				case ProgressConditionComparitor.Equal:
					clauseEvaluation = progressValuesAreEqual;
					break;

				case ProgressConditionComparitor.NotEqual:
					clauseEvaluation = (progressValuesAreEqual == false);
					break;

				case ProgressConditionComparitor.GreaterThan:
					clauseEvaluation = (
						(progressValue > clause.ProgressValue) &&
						(progressValuesAreEqual == false));
					break;

				case ProgressConditionComparitor.GreaterThanOrEqual:
					clauseEvaluation = (
						(progressValue > clause.ProgressValue) ||
						progressValuesAreEqual);
					break;

				case ProgressConditionComparitor.LessThan:
					clauseEvaluation = (
						(progressValue < clause.ProgressValue) &&
						(progressValuesAreEqual == false));
					break;

				case ProgressConditionComparitor.LessThanOrEqual:
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
				case ProgressConditionConcatenator.And:
					currentConditionEvaluation = (currentConditionEvaluation && clauseEvaluation);
					break;

				case ProgressConditionConcatenator.Or:
					currentConditionEvaluation = (currentConditionEvaluation || clauseEvaluation);
					break;

				default:
					throw new System.ComponentModel.InvalidEnumArgumentException(lastConcatenator.ToString());
			}

			lastConcatenator = clause.Concatenator;
		}

		return currentConditionEvaluation;
	}

	private void UpdateTargetObjectActivation()
	{
		if ((TargetObject != null) &&
			(ProgressCondition.Length > 0))
		{
			bool progressConditionEvaluation = ComputeProgressConditionEvaluation();

			if (progressConditionEvaluation != TargetObject.activeSelf)
			{
				TargetObject.SetActive(progressConditionEvaluation);
			}
		}
	}
}

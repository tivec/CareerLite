using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace CareerLite
{
	[KSPScenario(ScenarioCreationOptions.AddToExistingCareerGames,GameScenes.FLIGHT,GameScenes.EDITOR,GameScenes.SPACECENTER,GameScenes.TRACKSTATION)] 
	public class CareerLite : ScenarioModule
	{
		private bool lockFunds = false;

		public CareerLite ()
		{
			Debug.Log("[CareerLite [" + this.GetInstanceID ().ToString ("X") + "][" + Time.time.ToString ("0.0000") + "]: Constructor");
		}

		public void FundsChanged(double amount, TransactionReasons reason)
		{
			// TODO: Add GUI so this can be toggled!
			// if (!lockFunds)
			//	return;

			if (reason != TransactionReasons.Cheating)
				Funding.Instance.AddFunds (double.MaxValue-amount, TransactionReasons.Cheating);

		}


		public void Start()
		{
			Debug.Log("[CareerLite [" + this.GetInstanceID ().ToString ("X") + "][" + Time.time.ToString ("0.0000") + "]: Start");
			GameEvents.OnFundsChanged.Add (FundsChanged);

		}

		public override void OnAwake ()
		{

		}

		public override void OnSave (ConfigNode node)
		{
			node.AddValue ("LockedFunds", lockFunds.ToString());
		}

		// TODO: Move this to AstrotechUtilities
		private bool GetBool(ConfigNode node, string key) {
			return bool.Parse (node.GetValue (key));
		}


		public override void OnLoad (ConfigNode node)
		{
			Debug.Log("[CareerLite [" + this.GetInstanceID ().ToString ("X") + "][" + Time.time.ToString ("0.0000") + "]: OnLoad.");
			lockFunds = GetBool (node, "LockedFunds");
		}

		void OnDestroy()
		{
			GameEvents.OnFundsChanged.Remove (FundsChanged);
		}

		void OnGUI() 
		{

		}
	}
}


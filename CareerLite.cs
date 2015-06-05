using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace CareerLite
{
	[KSPScenario(ScenarioCreationOptions.AddToExistingCareerGames | ScenarioCreationOptions.AddToNewCareerGames,GameScenes.FLIGHT,GameScenes.EDITOR,GameScenes.SPACECENTER,GameScenes.TRACKSTATION)] 
	public class CareerLite : ScenarioModule
	{
		private bool lockFunds = false;
		public static double MONEY_LOCK = 99999999999;

		public CareerLite ()
		{
			Debug.Log("[CareerLite [" + this.GetInstanceID ().ToString ("X") + "][" + Time.time.ToString ("0.0000") + "]: Constructor");
		}

		public void FundsChanged(double amount, TransactionReasons reason)
		{
			// TODO: Add GUI so this can be toggled!
			// if (!lockFunds)
			//	return;

			if (reason != TransactionReasons.Cheating) // we don't want an ugly infinite loop here, do we?
			{
				if (Funding.Instance.Funds < MONEY_LOCK) {
					Funding.Instance.AddFunds (MONEY_LOCK - Funding.Instance.Funds, TransactionReasons.Cheating);
				}
			}

		}

		public void UnlockAllScience() {
		}


		public void Start()
		{
			Debug.Log("[CareerLite [" + this.GetInstanceID ().ToString ("X") + "][" + Time.time.ToString ("0.0000") + "]: Start");
			GameEvents.OnFundsChanged.Add (FundsChanged);
			GameEvents.onGUIRnDComplexSpawn.Add (UnlockAllScience);

			// This is a safeguard, just to make sure we have locked money.
			if (Funding.Instance.Funds < MONEY_LOCK) {
				Funding.Instance.AddFunds (MONEY_LOCK - Funding.Instance.Funds, TransactionReasons.Cheating);
			}

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
			GameEvents.onGUIRnDComplexSpawn.Remove (UnlockAllScience);
		}

		void OnGUI() 
		{

		}
	}
}


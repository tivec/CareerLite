using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace CareerLite
{
	[KSPScenario(ScenarioCreationOptions.AddToExistingCareerGames | ScenarioCreationOptions.AddToNewCareerGames,GameScenes.FLIGHT,GameScenes.EDITOR,GameScenes.SPACECENTER,GameScenes.TRACKSTATION)] 
	public class CareerLite : ScenarioModule
	{
		public static double MONEY_LOCK = 99999999999;

		public CareerLite ()
		{
			Debug.Log("[CareerLite [" + this.GetInstanceID ().ToString ("X") + "][" + Time.time.ToString ("0.0000") + "]: Constructor");
		}

		public void FundsChanged(double amount, TransactionReasons reason)
		{
			if (reason != TransactionReasons.Cheating) // we don't want an ugly infinite loop here, do we?
			{
				LockMoney ();
			}

		}

		public void LockMoney()
		{
			// This is a safeguard, just to make sure we have locked money.
			if (Funding.Instance.Funds < MONEY_LOCK) {
				Funding.Instance.AddFunds (MONEY_LOCK - Funding.Instance.Funds, TransactionReasons.Cheating);
			} else if (Funding.Instance.Funds > MONEY_LOCK) {
				Funding.Instance.AddFunds (-Funding.Instance.Funds, TransactionReasons.Cheating);
				Funding.Instance.AddFunds (MONEY_LOCK, TransactionReasons.Cheating);
			}
		}


		public void Start()
		{
			Debug.Log("[CareerLite [" + this.GetInstanceID ().ToString ("X") + "][" + Time.time.ToString ("0.0000") + "]: Start");
			GameEvents.OnFundsChanged.Add (FundsChanged);
			LockMoney ();

		}



		public override void OnAwake ()
		{

		}

		public override void OnSave (ConfigNode node)
		{
		}

		// TODO: Move this to AstrotechUtilities
		private bool GetBool(ConfigNode node, string key) {
			return bool.Parse (node.GetValue (key));
		}


		public override void OnLoad (ConfigNode node)
		{
			Debug.Log("[CareerLite [" + this.GetInstanceID ().ToString ("X") + "][" + Time.time.ToString ("0.0000") + "]: OnLoad.");
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


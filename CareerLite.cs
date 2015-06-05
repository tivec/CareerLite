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
				LockMoney ();
			}

		}

		public void HandleTechTree(RDTechTree techTree) {

			foreach(RDNode node in RDController.Instance.nodes)
			{

				Debug.Log ("[CareerLite] Node " + node.name + " is " + (node.IsResearched ? "researched" : "NOT researched"));


				if (!node.IsResearched) {
					//node.tech.ResearchTech ();
					//node.tech.AutoPurchaseAllParts ();
				}


				if (node.IsResearched && node.tech != null)
				{

					foreach (AvailablePart fPart in node.tech.partsPurchased)
					{
						if (node.tech.partsAssigned.Contains(fPart)) {
							Debug.Log("[CareerLite] Part in both lists: " + fPart.name);
						} else {
							Debug.Log("[CareerLite] Part in purchased : " + fPart.name);
						}
					}
					//node.UpdateGraphics ();
				}
			}
		}


		public void LockMoney()
		{
			// This is a safeguard, just to make sure we have locked money.
			if (Funding.Instance.Funds < MONEY_LOCK) {
				Funding.Instance.AddFunds (MONEY_LOCK - Funding.Instance.Funds, TransactionReasons.Cheating);
			} else if (Funding.Instance.Funds > MONEY_LOCK) {
				Funding.Instance.AddFunds (Funding.Instance.Funds - MONEY_LOCK, TransactionReasons.Cheating);
			}
		}


		public void Start()
		{
			Debug.Log("[CareerLite [" + this.GetInstanceID ().ToString ("X") + "][" + Time.time.ToString ("0.0000") + "]: Start");
			GameEvents.OnFundsChanged.Add (FundsChanged);
			RDTechTree.OnTechTreeSpawn.Add (HandleTechTree);

			LockMoney ();

		}



		public override void OnAwake ()
		{

		}

		public override void OnSave (ConfigNode node)
		{
			//node.AddValue ("LockedFunds", lockFunds.ToString());
		}

		// TODO: Move this to AstrotechUtilities
		private bool GetBool(ConfigNode node, string key) {
			return bool.Parse (node.GetValue (key));
		}


		public override void OnLoad (ConfigNode node)
		{
			Debug.Log("[CareerLite [" + this.GetInstanceID ().ToString ("X") + "][" + Time.time.ToString ("0.0000") + "]: OnLoad.");
			//lockFunds = GetBool (node, "LockedFunds");
		}

		void OnDestroy()
		{
			GameEvents.OnFundsChanged.Remove (FundsChanged);
			RDTechTree.OnTechTreeSpawn.Remove (HandleTechTree);
		}

		void OnGUI() 
		{

		}
	}
}


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
					Funding.Instance.AddFunds (MONEY_LOCK - amount, TransactionReasons.Cheating);
				}
			}

		}

		public void HandleTechTree(RDTechTree techTree) {

			foreach(RDNode node in RDController.Instance.nodes)
			{

				if (!node.IsResearched) {
					node.tech.ResearchTech ();
					node.tech.UnlockTech (true);
				}



				if (node.IsResearched && node.tech != null)
				{
					node.tech.AutoPurchaseAllParts ();
					foreach (AvailablePart fPart in node.tech.partsAssigned)
					{
						if (!node.tech.partsPurchased.Contains(fPart)) {
							Debug.Log("[CareerLite] Purchased part " + fPart.name);
						} else {
							Debug.Log("[CareerLite] Part " + fPart.name + " already purchased");
						}
					}

					//node.UpdateGraphics ();

				}
			}
		}



		public void Start()
		{
			Debug.Log("[CareerLite [" + this.GetInstanceID ().ToString ("X") + "][" + Time.time.ToString ("0.0000") + "]: Start");
			GameEvents.OnFundsChanged.Add (FundsChanged);
			RDTechTree.OnTechTreeSpawn.Add (HandleTechTree);

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


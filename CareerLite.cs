
/*
	Copyright (C) 2015  Henrik "Tivec" Bergvin

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/


using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Astrotech;

namespace CareerLite
{

	[KSPScenario(ScenarioCreationOptions.AddToExistingCareerGames | ScenarioCreationOptions.AddToNewCareerGames,GameScenes.FLIGHT,GameScenes.EDITOR,GameScenes.SPACECENTER,GameScenes.TRACKSTATION)]
	public class CareerLite : ScenarioModule
	{
		public static double MONEY_LOCK = 99999999999;
		private CareerLiteUI CareerLiteGUI = new CareerLiteUI();
		private bool unlockTechnology = false;

		private Dictionary<string, float> facilities = new Dictionary<string, float>();

		public CareerLite ()
		{
			Utilities.Log ("CareerLite", GetInstanceID (), "Constructor");

			// add the facilities, so we can save/load properly - can we do this better?
			facilities.Add ("LaunchPad", 0);
			facilities.Add ("Runway", 0);
			facilities.Add ("VehicleAssemblyBuilding", 0);
			facilities.Add ("TrackingStation", 0);
			facilities.Add ("AstronautComplex", 0);
			facilities.Add ("MissionControl", 0);
			facilities.Add ("ResearchAndDevelopment", 0);
			facilities.Add ("Administration", 0);

		}

		public void FundsChanged (double amount, TransactionReasons reason)
		{
			if (!CareerLiteGUI.GetOption (CareerOptions.LOCKFUNDS)) 
				return;

			if (reason != TransactionReasons.Cheating) // we don't want an ugly infinite loop here, do we?
			{
				LockMoney ();
			}
		}

		public void LockMoney ()
		{
			// This is a safeguard, just to make sure we have locked money.
			if (Funding.Instance.Funds < MONEY_LOCK) {
				Funding.Instance.AddFunds (MONEY_LOCK - Funding.Instance.Funds, TransactionReasons.Cheating);
			} else if (Funding.Instance.Funds > MONEY_LOCK) {
				Funding.Instance.AddFunds (-Funding.Instance.Funds, TransactionReasons.Cheating);
				Funding.Instance.AddFunds (MONEY_LOCK, TransactionReasons.Cheating);
			}
		}

		/* Thanks to Michael Marvin, author of the mod TreeToppler, for showing me that UnlockTech is the proper way of unlocking technologies.
		 * Code for TreeToppler is available under GPLv3, http://forum.kerbalspaceprogram.com/threads/107663, and express permission was given
		 * to use as inspiration: http://forum.kerbalspaceprogram.com/threads/124468-1-0-2-Kerbin-Astrotech-CareerLite?p=1999301&viewfull=1#post1999301
		 */
		public void RnDOpened (RDController controller)
		{
			if (!unlockTechnology) 
				return;


			if (CareerLiteGUI.GetOption(CareerOptions.UNLOCKTECH))
			{
				float level = GameVariables.Instance.GetScienceCostLimit(ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.ResearchAndDevelopment));
				foreach (RDNode node in controller.nodes) {
					if (node.tech != null && node.tech.scienceCost < level)
						node.tech.UnlockTech (true); //this will trigger an event, but we're not actioning this event for now.
				}
			} 
			unlockTechnology = false;
		}

		public void UpgradeAllBuildings()
		{
			ScenarioUpgradeableFacilities.protoUpgradeables.ToList ().ForEach (pu =>
           	{
				foreach (var p in pu.Value.facilityRefs) {
					Utilities.Log("CareerLite", GetInstanceID(), "Current level of " + p.name + " is " + ScenarioUpgradeableFacilities.GetFacilityLevel(p.name));
					if (!facilities.ContainsKey(p.name)) {
						facilities.Add(p.name, ScenarioUpgradeableFacilities.GetFacilityLevel(p.name));
					} else {
						facilities[p.name] = ScenarioUpgradeableFacilities.GetFacilityLevel(p.name);
					}

					p.SetLevel(p.MaxLevel);
				}
			});
		}

		public void DowngradeAllBuildings()
		{
			ScenarioUpgradeableFacilities.protoUpgradeables.ToList ().ForEach (pu =>
			                                                                   {
				foreach (var p in pu.Value.facilityRefs) {
					float newLevel = facilities[p.name];

					// GetFacilityLevel returns a float between 0 and 1, but the actual levels are ints. By multiplying with the level count of the facility, I get the true level.
					Utilities.Log("CareerLite", GetInstanceID(), "Current level of " + p.name + " is " + ScenarioUpgradeableFacilities.GetFacilityLevel(p.name) + ", downgrading to " +newLevel);
					p.SetLevel((int)(newLevel*pu.Value.GetLevelCount())); 
				}
			});
		}

		private IEnumerator Start()
		{

			//Utilities.Log ("CareerLite", GetInstanceID (), "Start");

			// Hook fund changes
			Utilities.Log ("CareerLite", GetInstanceID (), "Hook FundsChanged");
			GameEvents.OnFundsChanged.Add (FundsChanged);

			// Hook technology
			Utilities.Log ("CareerLite", GetInstanceID (), "Hook RnDTreeSpawn");
			RDController.OnRDTreeSpawn.Add (RnDOpened);

			yield return 0;

			ScenarioUpgradeableFacilities.protoUpgradeables.ToList ().ForEach (pu =>
			{
				foreach (var p in pu.Value.facilityRefs) {
					Utilities.Log ("CareerLite", GetInstanceID (), "Current level of " + p.name + " is " + ScenarioUpgradeableFacilities.GetFacilityLevel (p.name) + " count " + pu.Value.GetLevelCount());
					if (!facilities.ContainsKey (p.name)) {
						facilities.Add (p.name, ScenarioUpgradeableFacilities.GetFacilityLevel (p.name));
					} else {
						facilities [p.name] = ScenarioUpgradeableFacilities.GetFacilityLevel (p.name);
					}
				}
			});
		}

		public void Update()
		{
			if (CareerLiteGUI.TogglesChanged) {
				Utilities.Log ("CareerLite", GetInstanceID (), "Toggles have changed!");
				// Lock funds?
				if (CareerLiteGUI.GetOption (CareerOptions.LOCKFUNDS)) {
					LockMoney ();
				} 

				unlockTechnology = true; // do this on the next RnD;

				if (CareerLiteGUI.GetOption(CareerOptions.UNLOCKBUILDINGS)) {
					UpgradeAllBuildings ();
				} else {
					DowngradeAllBuildings ();
				}

				CareerLiteGUI.TogglesChanged = false;
			}
		}

		public override void OnSave (ConfigNode node)
		{
			CareerLiteGUI.SaveSettings (node);

			foreach (string key in facilities.Keys) {
				node.AddValue (key, facilities [key].ToString());
			}
		}

		public override void OnLoad (ConfigNode node)
		{
			CareerLiteGUI.LoadSettings (node);

			List<string> keys = new List<string> (facilities.Keys);
			foreach (string key in keys) {
				float level;
				node.GetConfigValue (out level, key);
				facilities [key] = level;
			}
		}

		void OnDestroy ()
		{
			GameEvents.OnFundsChanged.Remove (FundsChanged);
			RDController.OnRDTreeSpawn.Remove (RnDOpened);

			CareerLiteGUI.Destroy ();
		}

		void OnGUI()
		{
			CareerLiteGUI.DrawGUI ();
		}

	}
}


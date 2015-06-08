
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

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Astrotech;

namespace CareerLite
{

	public enum TechnologyUnlock
	{
		OFF,
		UNLOCK,
		REVERT
	}

	[KSPScenario(ScenarioCreationOptions.AddToExistingCareerGames | ScenarioCreationOptions.AddToNewCareerGames,GameScenes.FLIGHT,GameScenes.EDITOR,GameScenes.SPACECENTER,GameScenes.TRACKSTATION)]
	public class CareerLite : ScenarioModule
	{
		public static double MONEY_LOCK = 99999999999;
		private CareerLiteUI CareerLiteGUI = new CareerLiteUI();
		private TechnologyUnlock unlockTechnology = TechnologyUnlock.OFF;
		private double revertFunds = MONEY_LOCK;
		private Dictionary<string, float> facilities = new Dictionary<string, float>();
		bool RnDOpen = false;


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
			facilities.Add ("SpaceplaneHangar", 0);

			SetupToggles ();

		}

		private void SetupToggles() 
		{
			Utilities.Log ("CareerLiteUI", GetHashCode (), "Setting up toggle buttons");
			
			Rect optionRect = new Rect (0, 0, 195, 20);
			CareerLiteGUI.CreateToggle (CareerOptions.LOCKFUNDS, optionRect, false, "Lock funds", FundsLocked);
			CareerLiteGUI.CreateToggle (CareerOptions.UNLOCKBUILDINGS, optionRect, false, "Unlock buildings", BuildingsUnlocked);
			CareerLiteGUI.CreateToggle (CareerOptions.UNLOCKTECH, optionRect, false, "Unlock technologies", TechnologiesUnlocked);
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

		public void FundsLocked(bool state)
		{
			if (state) {
				// We just locked funds. Save the data now.
				revertFunds = Funding.Instance.Funds;
				LockMoney ();
				ScreenMessages.PostScreenMessage ("CareerLite: Funds locked.");
			} else {
				Funding.Instance.AddFunds (-Funding.Instance.Funds, TransactionReasons.Cheating);
				Funding.Instance.AddFunds (revertFunds, TransactionReasons.Cheating);
				ScreenMessages.PostScreenMessage ("CareerLite: Funds reverted.");
			}
		}

		public void BuildingsUnlocked(bool state) {
			if (state) {
				UpgradeAllBuildings ();
				ScreenMessages.PostScreenMessage ("CareerLite: Buildings upgraded!");
			} else 	{
				DowngradeAllBuildings ();
				ScreenMessages.PostScreenMessage ("CareerLite: Buildings downgraded.");
			}
		}

		public void TechnologiesUnlocked(bool state) {
			if (state) {
				unlockTechnology = TechnologyUnlock.UNLOCK;
				if (RnDOpen) {
					UnlockTechnologies ();
					ScreenMessages.PostScreenMessage ("CareerLite: Technologies unlocked. Close and reopen the Research Department to see changes.");
				} else {
					ScreenMessages.PostScreenMessage ("CareerLite: Visit the R&D to confirm technology changes.");
				}

			} else {
				unlockTechnology = TechnologyUnlock.REVERT;
				if (RnDOpen) {
					LockTechnologies ();
					ScreenMessages.PostScreenMessage ("CareerLite: Technologies locked. Close and reopen the Research Department to see changes.");
				} else {
					ScreenMessages.PostScreenMessage ("CareerLite: Visit the R&D to confirm technology changes.");
				}
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


		public void RnDOpened (RDController controller)
		{
			if (unlockTechnology == TechnologyUnlock.UNLOCK)
			{
				UnlockTechnologies ();
			} else if (unlockTechnology == TechnologyUnlock.REVERT){
				LockTechnologies ();
			}


		}

		public void RnDGUIClosed()
		{
			RnDOpen = false;
		}

		public void RnDGUIOpened()
		{
			RnDOpen = true;
		}

		/* Thanks to Michael Marvin, author of the mod TreeToppler, for showing me that UnlockTech is the proper way of unlocking technologies.
		 * Code for TreeToppler is available under GPLv3, http://forum.kerbalspaceprogram.com/threads/107663, and express permission was given
		 * to use as inspiration: http://forum.kerbalspaceprogram.com/threads/124468-1-0-2-Kerbin-Astrotech-CareerLite?p=1999301&viewfull=1#post1999301
		 * 
		 * The code in LockTechnologies() is directly based on his code.
		 */
		public void UnlockTechnologies()
		{
			float level = GameVariables.Instance.GetScienceCostLimit(ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.ResearchAndDevelopment));
			foreach (RDNode node in RDController.Instance.nodes) {
				if (node.tech != null && node.tech.scienceCost < level)
					node.tech.UnlockTech (true); //this will trigger an event, but we're not actioning this event for now.
			}
		}

		void LockTechnologies ()
		{
			unlockTechnology = TechnologyUnlock.OFF;

			foreach (RDNode node in RDController.Instance.nodes)
			{
				if (node.tech.scienceCost > 0)
				{
					ProtoTechNode protoNode = ResearchAndDevelopment.Instance.GetTechState(node.tech.techID);
					protoNode.state = RDTech.State.Unavailable;
					protoNode.partsPurchased.Clear();
					ResearchAndDevelopment.Instance.SetTechState(node.tech.techID, protoNode);
				}
			}
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
					if (facilities.ContainsKey(p.name)) {
						float newLevel = facilities[p.name];

						// GetFacilityLevel returns a float between 0 and 1, but the actual levels are ints. By multiplying with the level count of the facility, I get the true level.
						Utilities.Log("CareerLite", GetInstanceID(), "Current level of " + p.name + " is " + ScenarioUpgradeableFacilities.GetFacilityLevel(p.name) + ", downgrading to " +newLevel);
						p.SetLevel((int)(newLevel*pu.Value.GetLevelCount())); 
					} else {
						Utilities.Log("CareerLite", GetInstanceID(), "Facility " + p.name + " was missing in our registry!");
					}

				}
			});
		}

		private void Start()
		{

			//Utilities.Log ("CareerLite", GetInstanceID (), "Start");

			// Hook fund changes
			Utilities.Log ("CareerLite", GetInstanceID (), "Hook FundsChanged");
			GameEvents.OnFundsChanged.Add (FundsChanged);

			// Hook technology
			Utilities.Log ("CareerLite", GetInstanceID (), "Hook RnDTreeSpawn");
			RDController.OnRDTreeSpawn.Add (RnDOpened);
			GameEvents.onGUIRnDComplexSpawn.Add (RnDGUIOpened);
			GameEvents.onGUIRnDComplexDespawn.Add (RnDGUIClosed);
		}

		public void Update()
		{
		}

		public override void OnSave (ConfigNode node)
		{
			Utilities.Log ("CareerLite", GetInstanceID (), "Entering OnSave");
			CareerLiteGUI.SaveSettings (node);

			ConfigNode buildingLevels = new ConfigNode ("BUILDING_LEVELS");

			foreach (string key in facilities.Keys) {
				Utilities.Log ("CareerLite", GetInstanceID (), "Facility '" + key + " level " + facilities[key].ToString());
				buildingLevels.AddValue (key, facilities [key].ToString());
			}

			node.AddValue ("RevertFunds", revertFunds);
			node.AddNode (buildingLevels);
		}

		public override void OnLoad (ConfigNode node)
		{
			Utilities.Log ("CareerLite", GetInstanceID (), "Entering OnLoad");
			CareerLiteGUI.LoadSettings (node);

			if (node.HasValue ("RevertFunds"))
				node.GetConfigValue (out revertFunds, "RevertFunds");


			if (node.HasNode("BUILDING_LEVELS"))
			{
				ConfigNode buildingLevels = node.GetNode ("BUILDING_LEVELS");

				List<string> keys = new List<string> (facilities.Keys);
				foreach (string key in keys) {
					float level;
					buildingLevels.GetConfigValue (out level, key);
					Utilities.Log ("CareerLite", GetInstanceID (), "Facility '" + key + " level " + level);
					facilities [key] = level;
				}
			}

			if (CareerLiteGUI.GetOption(CareerOptions.UNLOCKTECH)) {
				unlockTechnology = TechnologyUnlock.UNLOCK;
			}
		}

		void OnDestroy ()
		{
			GameEvents.OnFundsChanged.Remove (FundsChanged);
			RDController.OnRDTreeSpawn.Remove (RnDOpened);
			GameEvents.onGUIRnDComplexSpawn.Remove (RnDGUIOpened);
			GameEvents.onGUIRnDComplexDespawn.Remove (RnDGUIClosed);

			CareerLiteGUI.Destroy ();
		}

		void OnGUI()
		{
			CareerLiteGUI.DrawGUI ();
		}

	}
}


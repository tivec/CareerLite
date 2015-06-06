
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

		/* Thanks to Michael Marvin, author of the mod TreeToppler, for showing me that UnlockTech is the proper way of unlocking technologies.
		 * Code for TreeToppler is available under GPLv3, http://forum.kerbalspaceprogram.com/threads/107663, and express permission was given
		 * to use as inspiration: http://forum.kerbalspaceprogram.com/threads/124468-1-0-2-Kerbin-Astrotech-CareerLite?p=1999301&viewfull=1#post1999301
		 */

		public void RnDOpened(RDController controller)
		{
			foreach (RDNode node in controller.nodes) 
			{
				if (node.tech != null)
					node.tech.UnlockTech (true); //this will trigger an event, but we're not actioning this event for now.
			}
		}

		public void Start()
		{
			Debug.Log("[CareerLite [" + this.GetInstanceID ().ToString ("X") + "][" + Time.time.ToString ("0.0000") + "]: Start");

			// Hook fund changes
			GameEvents.OnFundsChanged.Add (FundsChanged);

			// Hook technology
			RDController.OnRDTreeSpawn.Add (RnDOpened);

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
			RDController.OnRDTreeSpawn.Remove (RnDOpened);
		}

		void OnGUI() 
		{

		}
	}
}


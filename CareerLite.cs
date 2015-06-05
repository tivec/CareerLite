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

		}

		public void FundsChanged(double amount, TransactionReasons reason)
		{
			ScreenMessages.PostScreenMessage ("CareerLite: Funds changed: " + amount);

		}


		public void Start()
		{
			GameEvents.OnFundsChanged.Add (FundsChanged);

		}

		public override void OnAwake ()
		{
		}

		public override void OnSave (ConfigNode node)
		{
		}

		public override void OnLoad (ConfigNode node)
		{

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


using System;
using UnityEngine;
using Astrotech;

namespace CareerLite
{
	[KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
	public class CareerLiteUI : MonoBehaviour
	{
		static readonly CareerLiteUI _instance = new CareerLiteUI();
		public bool useAppLauncher = true;
		private IButton toolbarButton = null;

		public static CareerLiteUI Instance {
			get {
				return _instance;
			}
		}

		public CareerLiteUI ()
		{
			// Check if the toolbar is available
			if (ToolbarManager.ToolbarAvailable) {
				useAppLauncher = false; // force using the toolbar if it exists!

				toolbarButton = ToolbarManager.Instance.add ("CareerLite", "careerlitebutton");
				toolbarButton.TexturePath = "Astrotech/CareerLite/icons/careerlite";
				toolbarButton.ToolTip = "CareerLite options";
				toolbarButton.OnClick += (e) => {
					Utilities.Log ("CareerLiteUI", GetInstanceID (), "Toolbar button was pressed");
				};
			
			}
		}

		public void Awake() 
		{

		}

		public void FixedUpdate()
		{

		}

		public void OnGui()
		{

		}

		void OnDestroy ()
		{
			if (toolbarButton != null)
			{
				toolbarButton.Destroy ();
			}
		}
	}
}


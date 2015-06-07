using System;
using UnityEngine;

namespace CareerLite
{
	[KSPAddon(KSPAddon.Startup.EveryScene, false)]
	public class CareerLiteUI : MonoBehaviour
	{
		static readonly CareerLiteUI _instance = new CareerLiteUI();
		public bool useAppLauncher = true;
		private IButton toolbarButton;

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

				toolbarButton = ToolbarManager.Instance.add ("Career Lite", "careerlitebutton");
				toolbarButton.TexturePath = "Astrotech/CareerLite/icons/careerlite";
				toolbarButton.ToolTip = "Career Lite options";
				toolbarButton.OnClick += (e) => {
					Debug.Log("[CareerLite]: Toolbar button was clicked!");
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
	}
}


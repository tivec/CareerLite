using System;
using System.Collections.Generic;
using UnityEngine;
using Astrotech;

namespace CareerLite
{
	public enum CareerOptions {
		LOCKFUNDS,
		UNLOCKTECH,
		UNLOCKBUILDINGS
	}

	public class CareerLiteUI
	{
		private bool guiActive = false;
		private bool togglesChanged = false; // if the settings have changed, we need to do stuff!

		public bool useAppLauncher = true;
		private IButton toolbarButton = null;

		private int windowID;
		private Rect optionsWindowRect;

		private Dictionary<CareerOptions, MenuToggle> options;

		public bool GuiActive {
			get {
				return guiActive;
			}
			set {
				guiActive = value;
			}
		}

		public bool TogglesChanged {
			get {
				return togglesChanged;
			}
			set {
				togglesChanged = value;
			}
		}

		public Dictionary<CareerOptions, MenuToggle> Options {
			get {
				return options;
			}
		}

		public CareerLiteUI ()
		{
			options = new Dictionary<CareerOptions, MenuToggle> ();
			SetupToggles ();
			// Check if the toolbar is available
			if (ToolbarManager.ToolbarAvailable) {
				useAppLauncher = false; // force using the toolbar if it exists!

				toolbarButton = ToolbarManager.Instance.add ("CareerLite", "careerlitebutton");
				toolbarButton.TexturePath = "Astrotech/CareerLite/icons/careerlite";
				toolbarButton.ToolTip = "CareerLite options";
				toolbarButton.OnClick += (e) => {
					Utilities.Log ("CareerLiteUI", GetHashCode(), "Toolbar button was pressed");
					GuiActive = !GuiActive;
					Utilities.Log("CareerLiteUI", GetHashCode(), "Gui: " + GuiActive.ToString());
				};
			}

			windowID = Guid.NewGuid ().GetHashCode ();
			optionsWindowRect = new Rect (0, 0, 200, 100);
		}

		private void CreateToggle(CareerOptions opt, Rect rect, bool defaultstate, string description, Action cback)
		{
			options.Add (
				opt,
				new MenuToggle (rect, defaultstate, description, cback)
			);
		}

		private void onToggled()
		{
			TogglesChanged = true;
		}

		private void SetupToggles() 
		{
			Utilities.Log ("CareerLiteUI", GetHashCode (), "Setting up toggle buttons");

			Rect optionRect = new Rect (0, 0, 195, 20);
			CreateToggle (CareerOptions.LOCKFUNDS, optionRect, false, "Lock funds", onToggled);
			CreateToggle (CareerOptions.UNLOCKBUILDINGS, optionRect, false, "Unlock buildings", onToggled);
			CreateToggle (CareerOptions.UNLOCKTECH, optionRect, false, "Unlock technologies", onToggled);
		}

		public bool GetOption(CareerOptions opt)
		{
			if (options.ContainsKey(opt))
			{
				Utilities.Log ("CareerLiteUI", GetHashCode (), "Option " + opt.ToString() + " = " + options[opt].state);
				return options[opt].state;
			}

			return false;
		}

		public void LoadSettings(ConfigNode node)
		{
			node.GetConfigValue (out options [CareerOptions.LOCKFUNDS]._state, "LockFunds");
			node.GetConfigValue (out options [CareerOptions.UNLOCKBUILDINGS]._state, "UnlockBuildings");
			node.GetConfigValue (out options [CareerOptions.UNLOCKTECH]._state, "UnlockTech");
		}

		public void SaveSettings(ConfigNode node)
		{
			node.AddValue ("LockFunds", GetOption (CareerOptions.LOCKFUNDS).ToString ());
			node.AddValue ("UnlockBuildings", GetOption (CareerOptions.UNLOCKBUILDINGS).ToString ());
			node.AddValue ("UnlockTech", GetOption (CareerOptions.UNLOCKTECH).ToString ());
		}

		public void DrawGUI()
		{
			if (GuiActive) {
				optionsWindowRect = GUILayout.Window (windowID, optionsWindowRect, Draw, "CareerLite Options", GUI.skin.window);
			}
		}

		public void Draw(int windowID)
		{
			GUILayout.BeginVertical ();
			foreach(KeyValuePair<CareerOptions, MenuToggle> toggle in options) 
			{
				toggle.Value.draw ();
			}
			GUILayout.EndVertical ();
			GUI.DragWindow ();
		}

		public void Destroy ()
		{
			if (toolbarButton != null)
			{
				toolbarButton.Destroy ();
			}
		}
	}

	/* The MenuToggle class was built on the MIT Licensed code of Technicalfool's mod HeatWarning
	 * https://github.com/Technicalfool/HeatWarning/
	 */
	public class MenuToggle
	{
		private Rect size;
		public bool _state;
		private string description;
		private Action callback;

		public MenuToggle(Rect sizeRect, bool defaultState, string desc, Action cback)
		{
			size = new Rect (sizeRect);
			_state = defaultState;
			description = desc;
			callback = cback;
		}

		public bool state {
			get { return _state; }
			set {_state = value; }
		}

		public string getDescription {
			get { return description; }
		}

		public Rect getSize {
			get { return size; }
		}

		// this must be called in OnGUI
		public void draw()
		{
			bool oldState = _state;
			_state = GUILayout.Toggle (_state, description, GUILayout.ExpandWidth (true));
			if (_state != oldState) {
				Utilities.Log ("MenuToggle", GetHashCode (), "State changed to " + _state);
				callback ();
			}
		}
	}


}


/******************************************************************************\
* Copyright (C) 2012-2014 Leap Motion, Inc. All rights reserved.               *
* Leap Motion proprietary and confidential. Not for distribution.              *
* Use subject to the terms of the Leap Motion SDK Agreement available at       *
* https://developer.leapmotion.com/sdk_agreement, or another agreement         *
* between Leap Motion and you, your company or other organization.             *
\******************************************************************************/
using System;
using System.Threading;
using UnityEngine;
using Leap;

class LeapListener : Listener {

	public override void OnInit(Controller controller) {
		Debug.Log("Leap motion initialised");
	}

	public override void OnConnect(Controller controller) {
		if (!LeapManager.connected) {
			Debug.Log("Leap motion connected");
			LeapManager.connected = true;
		}else {
			Debug.Log("Attempted to try to connect when already connected");
		}
	}

	public override void OnDisconnect(Controller controller) {
        //Note: not dispatched when running in a debugger.
		Debug.Log("Leap motion disconnected");
		LeapManager.connected = false;
	}

	public override void OnExit(Controller controller) {
		Debug.Log("Leap motion has exited");
		LeapManager.connected = false;
	}

	public override void OnFrame(Controller controller) {
		// Get the most recent frame and report some basic information
		Frame frame = controller.Frame();

		foreach (Hand hand in frame.Hands) {
			//Debug.Log("Hand id: " + hand.Id
			//			+ ", palm position: " + hand.PalmPosition);
			// Get the hand's normal vector and direction
			Vector normal = hand.PalmNormal;
			Vector direction = hand.Direction;

			// Calculate the hand's pitch, roll, and yaw angles
			Debug.Log("Hand pitch: " + direction.Pitch * 180.0f / (float)System.Math.PI + " degrees, "
						+ "roll: " + normal.Roll * 180.0f / (float)System.Math.PI + " degrees, "
						+ "yaw: " + direction.Yaw * 180.0f / (float)System.Math.PI + " degrees");
		}
	}
}

class LeapManager {

	public static LeapListener listener; 
	public static Controller controller;
	public static bool inited = false;
	public static bool connected = false;
	public static bool enabled = false;

	public static void init() {
		if (inited) dispose();

		// Create a sample listener and controller
		listener = new LeapListener();
		controller = new Controller();

		// Have the sample listener receive events from the controller
		controller.AddListener(listener);

		inited = true;
		enabled = true;

		Debug.Log("inited leap motion listener");
	}

	public static void update() {
		if (Input.GetKeyDown(KeyCode.P)) {
			if (enabled) init(); else dispose();
		}
	}

	public static void dispose() {
		if (inited) {
			// Remove the sample listener when done
			controller.RemoveListener(listener);
			controller.Dispose();
			Debug.Log("leap motion data disposed");
			inited = false;
			enabled = false;
		}
	}
}

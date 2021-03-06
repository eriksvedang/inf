﻿using UnityEngine;
using System.Collections;

public interface Freezable {
	void Freeze(InfZone zone);
	void Unfreeze(InfZone zone);
	void UpdateWhileFrozen(InfZone zone);
}

public class InfActor : MonoBehaviour {

	public InfZone zone;

	InfZone[] zones;
	System.Action<InfZone> onFreeze;
	System.Action<InfZone> onUnfreeze;
	System.Action<InfZone> onUpdateWhileFrozen;
	int randomFrameCountOffset;

	void Awake() {
		zones = GameObject.FindObjectsOfType<InfZone>();
		TransferToClosestZone();
		randomFrameCountOffset = Random.Range(0, 60);
		SetupFreezableDelegates();
	}

	void SetupFreezableDelegates () {
		var updateableComponents = GetComponents<Freezable>();
		foreach(var u in updateableComponents) {
			onFreeze += u.Freeze;
			onUnfreeze += u.Unfreeze;
			onUpdateWhileFrozen += u.UpdateWhileFrozen;
		}
	}

	public void UpdateWhileFrozen () {
		onUpdateWhileFrozen(zone);
	}

	/// <summary>
	/// Alters the activity status of the actor's GameObject. 
	/// If the status is changed from it's previous value the approperiate callback is called (FixAfterFreeze / FixAfterUnfreeze).
	/// </summary>
	public void SetActive (bool newActiveStatus, InfZone zone) {
		if (!isActiveAndEnabled && newActiveStatus) {
			FixAfterUnfreeze (zone);
		} else if (isActiveAndEnabled && !newActiveStatus) {
			FixAfterFreeze (zone);
		}
		gameObject.SetActive (newActiveStatus);
	}

	/// <summary>
	/// Whether an actor is frozen or not depends solely on if its Zone is frozen. It can't be set from the actor itself.
	/// An actor always has exactly one zone that it is contained inside. 
	/// Zones might overlap geographicly though, and many zones can be active at the same time.
	/// </summary>
	public bool frozen {
		get {
			if (zone == null) {
				throw new UnityException (name + "'s zone == null");
			}
			return zone.frozen;
		}
	}

	/// <summary>
	/// The correct way to move an actor from its current Zone to another one.
	/// </summary>
	public void TransferToZone(InfZone newZone) {
		if(zone == newZone) {
			return; // Already in that zone
		}
		if(zone != null) {
			zone.RemoveActor (this);
		}
		newZone.AddActor (this);
		zone = newZone;
		SetActive (!zone.frozen, zone);
	}

	public void TransferToClosestZone() {
		TransferToZone(InfZone.ClosestZone(transform.position, zones));
	}

	void Update() {
		if((randomFrameCountOffset + Time.frameCount) % 60 == 0) {
			TransferToClosestZone();
		}
	}

	public void FixAfterUnfreeze (InfZone zone) {
		onUnfreeze (zone);
	}

	public void FixAfterFreeze (InfZone zone) {
		onFreeze (zone);
	}
}

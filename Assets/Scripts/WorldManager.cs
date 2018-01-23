using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour {
	
	[SerializeField]
	GameObject PhysicsObjPrefab;
	[SerializeField]
	int createObjects = 1;

	Vector3 worldDimension;

	List<PhysicsObjectGraphics> objects;

	public List<PhysicsObjectGraphics> Objects {
		get {
			return objects;
		}
	}

	public Vector3 WorldDimension {
		get {
			return worldDimension;
		}
	}

	// Use this for initialization
	void Start () {
		objects = new List<PhysicsObjectGraphics>();
		worldDimension = transform.localScale;

		// Create random
		for(int i=0;i<createObjects;i++){
			GameObject g = Instantiate( PhysicsObjPrefab );
			g.transform.position = new Vector3( Random.Range( -worldDimension.x/2f, worldDimension.x/2f ), worldDimension.y/2f, 0 );
			//g.transform.position = new Vector3( 0, worldDimension.y/2f, 0 );

			PhysicsObjectGraphics obj = g.GetComponent<PhysicsObjectGraphics>();
			objects.Add( obj );
			obj.World = this;
			obj.PhysicsObj.setPosition( g.transform.position );
		}
	}

	void FixedUpdate () {
		foreach( PhysicsObjectGraphics obj in objects ){
			obj.ApplyPhysics();
		}
	}


}

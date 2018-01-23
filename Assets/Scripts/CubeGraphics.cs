using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeGraphics : ObjectGraphics {

	CubeStaticObject staticObj;

	public CubeStaticObject StaticObj {
		get {
			return staticObj;
		}
	}

	void Awake(){
		staticObj = new CubeStaticObject( this.transform.position, this.transform.localScale.x, this.transform.localScale.y, this.transform.localScale.z );
	}

	// Use this for initialization
	void Start () {
		this.transform.position = staticObj.getStartPosition();
	}

	public override void ApplyPhysics () {

	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractObject {

	protected Vector3 startPosition;	// m

	public AbstractObject( Vector3 startPosition ){
		this.startPosition = startPosition;
	}

	public Vector3 getStartPosition(){
		return this.startPosition;
	}

	public abstract CubeStaticObject getBoundingBox();
}

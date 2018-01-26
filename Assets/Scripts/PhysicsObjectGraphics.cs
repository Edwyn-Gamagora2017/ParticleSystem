using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsObjectGraphics : ObjectGraphics {

	public float mass = 1;

	PhysicsObject physicsObj;
	List<Vector3> forces;
	WorldManager world;

	Vector3 extraForce;

	float densityFactor;

	public PhysicsObject PhysicsObj {
		get {
			return physicsObj;
		}
	}

	public WorldManager World {
		set {
			world = value;
		}
	}

	public float DensityFactor {
		get {
			return densityFactor;
		}
		set {
			setDensityFactor( value );
		}
	}

	public CubeStaticObject generateBoundingBox(){
		return new CubeStaticObject( this.transform.position, this.transform.localScale.x, this.transform.localScale.y, this.transform.localScale.z);
	}

	void Awake(){
		physicsObj = new PhysicsObject( mass, this.transform.localScale.x/2f, this.transform.position, new Vector3( 2, Random.Range( -5f, 0f ), 0 ), generateBoundingBox() );
		//physicsObj = new PhysicsObject( mass, this.transform.position, new Vector3( 4, 0, 0 ), generateBoundingBox() );
		forces = new List<Vector3>();
		extraForce = new Vector3(0,0,0);
	}

	// Use this for initialization
	void Start () {
	}

	public override void ApplyPhysics () {
		forces.Clear();
		forces.Add( PhysicsObject.gravityAcc*mass );
		forces.Add( extraForce );
		extraForce = new Vector3(0,0,0);

		if( world != null ){
			physicsObj.evaluate( forces, Time.deltaTime, world );
		}

		this.transform.position = physicsObj.getPosition();
	}

	public void setExtraForce( Vector3 force ){
		extraForce = force;
	}

	private void setDensityFactor( float value ){
		this.densityFactor = value;
		this.GetComponent<MeshRenderer>().material.color = Color.white*value;
	}
}

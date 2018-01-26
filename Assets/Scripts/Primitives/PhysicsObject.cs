using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsObject : AbstractObject {
	
	Vector3 position;		// m
	Vector3 speed;			// m/s
	Vector3 acceleration;	// m/s2

	Vector3 temp_speed;			// m/s

	float mass;
	float radius;

	CubeStaticObject boundingBox;

	//public static Vector3 gravityAcc = new Vector3(0f,-9.8f,0f);
	public static Vector3 gravityAcc = new Vector3(0f,-5f,0f);

	float reboundFactor = 1.5f;
	List<Vector3> internalForces;

	List<PhysicsObjectGraphics> currentNeighbors;

	float densityThreshold =100f;
	float densityFactor = 0.2f;

	public PhysicsObject( float mass, float radius, Vector3 startPosition, Vector3 startSpeed, CubeStaticObject boundingBox )
	: base( startPosition ){
		this.mass = mass;
		this.radius = radius;

		this.position = startPosition;
		this.speed = startSpeed;
		this.temp_speed = startSpeed;

		this.acceleration = new Vector3(0,0,0);

		this.boundingBox = boundingBox;

		internalForces = new List<Vector3>();
		currentNeighbors = new List<PhysicsObjectGraphics>();
	}

	public PhysicsObject( float mass, float radius, Vector3 startPosition, CubeStaticObject boundingBox )
		: this( mass, radius, startPosition, new Vector3( 0,0,0 ), boundingBox ){}

	public void evaluate( List<Vector3> forces, float deltaTSeconds, WorldManager world ){
		this.speed = temp_speed;

		// calculate Acceleration
		Vector3 newAcceleration = new Vector3(0,0,0);
		foreach( Vector3 force in forces ){
			newAcceleration += force;
		}
		foreach( Vector3 force in internalForces ){
			newAcceleration += force;
		}
		internalForces.Clear();
		newAcceleration /= mass;

		// calculate Speed
		Vector3 newSpeed = this.speed + newAcceleration*deltaTSeconds;

		// TODO viscosity here

		// calculate Position
		Vector3 newPosition = this.position + newSpeed*deltaTSeconds;
		//Vector3 newPosition = this.position + deltaTSeconds*deltaTSeconds*newAcceleration;

		// Internal Repulsion and Attraction
		Vector3 accumulatedTranslation = new Vector3(0,0,0);
		float densityStatus = world.obtainP(this,currentNeighbors,2);
		float densityStatusNear = world.obtainP(this,currentNeighbors,3);
		foreach( PhysicsObjectGraphics neighbor in currentNeighbors ){
			accumulatedTranslation += world.densityRelaxation(
				newPosition,
				neighbor.PhysicsObj.getPosition(),
				densityFactor*(densityStatus-densityThreshold),
				densityFactor*densityStatusNear,
				deltaTSeconds
			);
		}
		//Debug.Log( accumulatedTranslation );
		newPosition += accumulatedTranslation;

		// Calculate Collision
		bool collisionTrigger = false;

		// Collision to world
		Vector3 reboundForceAcc = new Vector3(0,0,0);
		if( newPosition.x < -world.WorldDimension.x/2f || newPosition.x > world.WorldDimension.x/2f ){
			reboundForceAcc += new Vector3( -(2*this.temp_speed.x)/(reboundFactor*deltaTSeconds), 0, 0 );
			newPosition.x = this.position.x;
			collisionTrigger = true;
		}
		if( newPosition.y < -world.WorldDimension.y/2f || newPosition.y > world.WorldDimension.y/2f ){
			reboundForceAcc += new Vector3( -(0.01f*this.temp_speed.x), -(2f*this.temp_speed.y), 0 )/(reboundFactor*deltaTSeconds);
			newPosition.y = this.position.y;
			collisionTrigger = true;
		}
		if( newPosition.z < -world.WorldDimension.z/2f || newPosition.z > world.WorldDimension.z/2f ){
			reboundForceAcc += new Vector3( 0, 0, -(2*this.temp_speed.z)/(reboundFactor*deltaTSeconds) );
			newPosition.z = this.position.z;
			collisionTrigger = true;
		}
		if( collisionTrigger ){
			internalForces.Add( reboundForceAcc*this.mass );
		}

		this.temp_speed = (newPosition-this.position)/deltaTSeconds;
		this.position = newPosition;

		// Viscosity
		/*foreach( PhysicsObjectGraphics neighbor in neighbors ){
			internalForces.Add( viscosity( neighbor, deltaTSeconds, world.NeighborhoodRadius ) );
		}*/

		// Collision to other objects
		/*foreach( PhysicsObjectGraphics neighbor in neighbors ){
			if( (newPosition - neighbor.PhysicsObj.getPosition()).magnitude < this.radius+neighbor.PhysicsObj.Radius ){
				// Collision
				this.internalForces.Add( ((neighbor.PhysicsObj.getSpeed()+this.speed)/(deltaTSeconds*10f))*neighbor.mass );
				collisionTrigger = true;
			}
		}*/

		/*if( !collisionTrigger ){
			// Update info : new one
			this.temp_speed = newSpeed;
			this.position = newPosition;
			//this.boundingBox.Position = newPosition;
		}
		else{
			// Update info : 0
			internalForces.Add( reboundForceAcc*this.mass );
			//this.temp_speed = new Vector3(0,0,0);
		}*/

		this.temp_speed = newSpeed;
		//this.temp_speed = (newPosition-this.position)/deltaTSeconds;
		//this.position = newPosition;
	}

	public void setPosition(Vector3 position){
		this.position = position;
	}
	public Vector3 getPosition(){
		return this.position;
	}
	public Vector3 getSpeed(){
		return this.speed;
	}
	public Vector3 getAcceleration(){
		return this.acceleration;
	}
	public float Mass {
		get {
			return mass;
		}
	}
	public float Radius {
		get {
			return radius;
		}
	}

	public List<PhysicsObjectGraphics> Neighbors {
		get {
			return currentNeighbors;
		}
		set {
			currentNeighbors = value;
		}
	}

	public List<Vector3> InternalForces {
		get {
			return internalForces;
		}
	}

	Vector3 viscosity( PhysicsObjectGraphics neighbor, float deltaSeconds, float neighborhoodRadius ){
		Vector3 diffSpeed = this.getSpeed() - neighbor.PhysicsObj.getSpeed();
		Vector3 diffPosition = this.getPosition() - neighbor.PhysicsObj.getPosition();

		float q = diffPosition.magnitude/neighborhoodRadius;
		if( q < 1 ){
			float u = Vector3.Dot( diffSpeed, diffPosition );
			if( u > 0 ){
				return (1-q)*(u*0.8f+u*u*0.8f)*diffPosition/deltaSeconds;
			}
		}
		return new Vector3();
	}

	Vector3 center( List<PhysicsObjectGraphics> neighbors ){
		if( neighbors.Count > 0 ){
			Vector3 result = neighbors[0].PhysicsObj.getPosition();
			for( int i = 1; i < neighbors.Count; i++ ){
				result+=neighbors[i].PhysicsObj.getPosition();
			}
			return result;
		}
		return this.position;
	}

	/*Vector3 spring( PhysicsObjectGraphics neighbor, float deltaSeconds, float neighborhoodRadius ){
		Vector3 diffPosition = this.getPosition() - neighbor.PhysicsObj.getPosition();

		float q = diffPosition.magnitude/neighborhoodRadius;
		if( q < 1 ){
			
		}
		return new Vector3();
	}*/

	#region implemented abstract members of AbstractObject

	public override CubeStaticObject getBoundingBox ()
	{
		return this.boundingBox;
	}

	#endregion
}

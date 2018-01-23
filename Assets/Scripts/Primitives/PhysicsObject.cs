using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsObject : AbstractObject {
	
	Vector3 position;		// m
	Vector3 speed;			// m/s
	Vector3 acceleration;	// m/s2

	float mass;

	CubeStaticObject boundingBox;

	//public static Vector3 gravityAcc = new Vector3(0f,-9.8f,0f);
	public static Vector3 gravityAcc = new Vector3(0f,-5f,0f);

	Vector3 reboundForceAcc;
	float reboundFactor = 1.1f;

	public PhysicsObject( float mass, Vector3 startPosition, Vector3 startSpeed, CubeStaticObject boundingBox )
	: base( startPosition ){
		this.mass = mass;

		this.position = startPosition;
		this.speed = startSpeed;

		this.acceleration = new Vector3(0,0,0);

		this.boundingBox = boundingBox;

		this.reboundForceAcc = new Vector3(0,0,0);
	}

	public PhysicsObject( float mass, Vector3 startPosition, CubeStaticObject boundingBox )
		: this( mass, startPosition, new Vector3( 0,0,0 ), boundingBox ){}

	public void evaluate( List<Vector3> forces, float deltaTSeconds, WorldManager world ){
		// calculate Acceleration
		Vector3 newAcceleration = new Vector3(0,0,0);
		foreach( Vector3 force in forces ){
			newAcceleration += force;
		}

		newAcceleration += this.reboundForceAcc*this.mass;
		newAcceleration /= mass;
		// calculate Speed
		Vector3 newSpeed = this.speed + newAcceleration*deltaTSeconds;
		// calculate Position
		Vector3 newPosition = this.position + newSpeed*deltaTSeconds;

		// Calculate Collision
		bool collisionTrigger = false;
		/*CubeStaticObject futureBoundingBox = new CubeStaticObject( newPosition, this.boundingBox.Width, this.boundingBox.Height, this.boundingBox.Depth );
		AbstractObject collisionObj = world.BbTree.collision( futureBoundingBox, this );
		collisionTrigger = collisionObj != null;*/

		this.reboundForceAcc = new Vector3(0,0,0);
		// Collision to world
		if( newPosition.x < -world.WorldDimension.x/2f || newPosition.x > world.WorldDimension.x/2f ){
			collisionTrigger = true;

			//this.reboundForceAcc += new Vector3( -this.speed.x/(reboundFactor*deltaTSeconds), 0,0 );
			newSpeed = new Vector3( -newSpeed.x, newSpeed.y, newSpeed.z );
			this.reboundForceAcc += newSpeed/(reboundFactor*deltaTSeconds);
		}
		if( newPosition.y < -world.WorldDimension.y/2f || newPosition.y > world.WorldDimension.y/2f ){
			collisionTrigger = true;

			//this.reboundForceAcc += new Vector3( 0,-this.speed.y/(reboundFactor*deltaTSeconds), 0 );
			newSpeed = new Vector3( newSpeed.x, -newSpeed.y, newSpeed.z );
			this.reboundForceAcc += newSpeed/(reboundFactor*deltaTSeconds);
		}
		if( newPosition.z < -world.WorldDimension.z/2f || newPosition.z > world.WorldDimension.z/2f ){
			collisionTrigger = true;
			this.reboundForceAcc += new Vector3( 0,0,-this.speed.z/(reboundFactor*deltaTSeconds) );
		}

		if( !collisionTrigger ){
			// Update info : new one
			this.speed = newSpeed;
			this.position = newPosition;
			this.boundingBox.Position = newPosition;
		}
		else{
			// Update info : 0
			this.speed = new Vector3(0,0,0);
		}
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

	#region implemented abstract members of AbstractObject

	public override CubeStaticObject getBoundingBox ()
	{
		return this.boundingBox;
	}

	#endregion
}

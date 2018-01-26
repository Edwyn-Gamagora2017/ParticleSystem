﻿using System.Collections;
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

	float reboundFactor = 1.2f;
	List<Vector3> internalForces;
	List<PhysicsObjectGraphics> neighbors;

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
		neighbors = new List<PhysicsObjectGraphics>();
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
		// calculate Position
		Vector3 newPosition = this.position + newSpeed*deltaTSeconds;

		// Calculate Collision
		bool collisionTrigger = false;
		/*CubeStaticObject futureBoundingBox = new CubeStaticObject( newPosition, this.boundingBox.Width, this.boundingBox.Height, this.boundingBox.Depth );
		AbstractObject collisionObj = world.BbTree.collision( futureBoundingBox, this );
		collisionTrigger = collisionObj != null;*/

		// Collision to world
		if( newPosition.x < -world.WorldDimension.x/2f || newPosition.x > world.WorldDimension.x/2f ){
			collisionTrigger = true;

			newSpeed = new Vector3( -newSpeed.x, newSpeed.y, newSpeed.z );
		}
		if( newPosition.y < -world.WorldDimension.y/2f || newPosition.y > world.WorldDimension.y/2f ){
			collisionTrigger = true;

			newSpeed = new Vector3( newSpeed.x, -newSpeed.y, newSpeed.z );
		}
		if( newPosition.z < -world.WorldDimension.z/2f || newPosition.z > world.WorldDimension.z/2f ){
			collisionTrigger = true;

			newSpeed = new Vector3( newSpeed.x, newSpeed.y, -newSpeed.z );
		}

		// Viscosity
		foreach( PhysicsObjectGraphics neighbor in neighbors ){
			internalForces.Add( viscosity( neighbor, deltaTSeconds, world.NeighborhoodRadius ) );
		}

		// Collision to other objects
		/*foreach( PhysicsObjectGraphics neighbor in neighbors ){
			if( (newPosition - neighbor.PhysicsObj.getPosition()).magnitude < this.radius+neighbor.PhysicsObj.Radius ){
				// Collision
				this.internalForces.Add( ((neighbor.PhysicsObj.getSpeed()+this.speed)/(deltaTSeconds*10f))*neighbor.mass );
				collisionTrigger = true;
			}
		}*/

		if( !collisionTrigger ){
			// Update info : new one
			this.temp_speed = newSpeed;
			this.position = newPosition;
			this.boundingBox.Position = newPosition;
		}
		else{
			// Update info : 0
			Vector3 reboundForceAcc = new Vector3(0,0,0);
			reboundForceAcc += newSpeed/(reboundFactor*deltaTSeconds);
			internalForces.Add( reboundForceAcc*this.mass );
			this.temp_speed = new Vector3(0,0,0);
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
			return neighbors;
		}
		set {
			neighbors = value;
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

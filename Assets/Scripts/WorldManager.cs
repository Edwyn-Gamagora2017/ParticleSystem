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
	Dictionary< int, List< PhysicsObjectGraphics > > hashObjects;
	int hashCapacity;
	Vector3 gridDimension;
	float neighborhoodRadius;

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

	public float NeighborhoodRadius {
		get {
			return neighborhoodRadius;
		}
	}

	// Use this for initialization
	void Start () {
		objects = new List<PhysicsObjectGraphics>();
		worldDimension = transform.localScale;
		gridDimension = worldDimension/20;
		neighborhoodRadius = gridDimension.x;

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

		hashCapacity = Mathf.Max( objects.Count/10 , 1 );
		fillHash( hashCapacity, gridDimension );
	}

	void FixedUpdate () {
		foreach( PhysicsObjectGraphics obj in objects ){
			obj.ApplyPhysics();
		}

		fillHash( hashCapacity, gridDimension );

		foreach( PhysicsObjectGraphics obj in objects ){
			List<PhysicsObjectGraphics> neighbors = neighborhood( obj, hashCapacity, gridDimension, neighborhoodRadius );
			obj.PhysicsObj.Neighbors = neighbors;

			obj.DensityFactor = obtainDensity( obj, neighbors, neighborhoodRadius );
		}
	}

	void fillHash( int _hashCapacity, Vector3 _gridCubeDimension ){
		hashObjects = new Dictionary<int, List<PhysicsObjectGraphics>>();
		for( int i = 0; i < _hashCapacity; i++ ){
			hashObjects[i] = new List<PhysicsObjectGraphics>();
		}

		foreach( PhysicsObjectGraphics obj in objects ){
			int key = getParticleHash( obj.PhysicsObj.getPosition(), _hashCapacity, _gridCubeDimension );
			try{
				hashObjects[ key ].Add( obj );
			}
			catch(System.Exception ex){
				Debug.LogWarning( key );
			}
		}
	}

	Vector3 getParticleGridPosition( Vector3 _objPosition, Vector3 _gridCubeDimension ){
		int gridX = Mathf.FloorToInt( _objPosition.x / _gridCubeDimension.x );
		int gridY = Mathf.FloorToInt( _objPosition.y / _gridCubeDimension.y );
		int gridZ = Mathf.FloorToInt( _objPosition.z / _gridCubeDimension.z );

		return new Vector3( gridX, gridY, gridZ );
	}

	int getParticleHash( Vector3 _objPosition, int _hashCapacity, Vector3 _gridCubeDimension ){
		Vector3 gridPosition = getParticleGridPosition( _objPosition, _gridCubeDimension );

		return getParticleGridPositionHash( gridPosition, _hashCapacity );
	}

	int getParticleGridPositionHash( Vector3 _objGridPosition, int _hashCapacity ){
		return Mathf.Abs(Mathf.FloorToInt(_objGridPosition.x*2+_objGridPosition.y*3+_objGridPosition.z*4))%_hashCapacity;
	}

	List<PhysicsObjectGraphics> neighborhood( PhysicsObjectGraphics objPOG, int _hashCapacity, Vector3 _gridCubeDimension, float radius ){
		Vector3 gridPosition = getParticleGridPosition( objPOG.PhysicsObj.getPosition(), _gridCubeDimension );

		List<PhysicsObjectGraphics> result = new List<PhysicsObjectGraphics>();

		// check neighbor grid cells 2D
		// TODO check if the cell is inside the radius
		for( int i = -1; i <= 1; i++ ){
			for( int j = -1; j <= 1; j++ ){
				Vector3 neighborGridPosition = new Vector3( gridPosition.x+i, gridPosition.y+j, gridPosition.z );

				int neighborHash = getParticleGridPositionHash( neighborGridPosition, _hashCapacity );
				foreach( PhysicsObjectGraphics obj in hashObjects[ neighborHash ] ){
					// in radius?
//Debug.Log( hashObjects[ neighborHash ].Count );
					if( obj != objPOG && isNeighbor( objPOG.PhysicsObj.getPosition(), obj.PhysicsObj.getPosition(), radius ) && !result.Contains(obj) ){
						result.Add( obj );
					}
				}
			}
		}

		return result;
	}

	bool isNeighbor( Vector3 aPos, Vector3 bPos, float radius ){
		return (aPos - bPos).magnitude <= radius;
	}

	float obtainDensity( PhysicsObjectGraphics obj, List<PhysicsObjectGraphics> neighbors, float radius ){
		float density = 0;
		foreach( PhysicsObjectGraphics neighbor in neighbors ){
			density += Mathf.Pow( 1f-(( obj.PhysicsObj.getPosition()-neighbor.PhysicsObj.getPosition() ).magnitude/radius), 2f );
		}
		return Mathf.Sqrt( density );

		/*float dist = 0;
		foreach( PhysicsObjectGraphics neighbor in neighbors ){
			dist += ( obj.getPosition()-neighbor.PhysicsObj.getPosition() ).magnitude;
		}

		return dist/(float)neighbors.Count;*/
	}

	public float obtainDensity( PhysicsObjectGraphics obj, List<PhysicsObjectGraphics> neighbors ){
		return obtainDensity( obj, neighbors, neighborhoodRadius );
	}
}

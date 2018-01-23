using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeStaticObject : AbstractObject {
	float width;
	float height;
	float depth;

	public CubeStaticObject( Vector3 startPosition, float width, float height, float depth )
		: base( startPosition ){
		this.width = width;
		this.height = height;
		this.depth = depth;
	}
		
	public float Width {
		get {
			return width;
		}
		set{
			width = value;
		}
	}

	public float Height {
		get {
			return height;
		}
		set{
			height = value;
		}
	}

	public float Depth {
		get {
			return depth;
		}
		set{
			depth = value;
		}
	}

	public Vector3 Position {
		get {
			return startPosition;
		}
		set {
			startPosition = value;
		}
	}

	public Vector3 minValues(){
		return new Vector3( this.startPosition.x-this.width/2f, this.startPosition.y-this.height/2f, this.startPosition.z-this.depth/2f);
	}
	public Vector3 maxValues(){
		return new Vector3( this.startPosition.x+this.width/2f, this.startPosition.y+this.height/2f, this.startPosition.z+this.depth/2f);
	}

	public bool isPointInside( Vector3 point ){
		Vector3 thisMin = this.minValues();
		Vector3 thisMax = this.maxValues();

		return point.x >= thisMin.x && point.x <= thisMax.x && point.y >= thisMin.y && point.y <= thisMax.y && point.z >= thisMin.z && point.z <= thisMax.z;
	}
	/*public Vector3 collisionDistance( Vector3 point ){
		Vector3 thisMin = this.minValues();
		Vector3 thisMax = this.maxValues();
	}*/
	public List<Vector3> vertices(){
		List<Vector3> result = new List<Vector3>();
		Vector3 thisMin = this.minValues();
		Vector3 thisMax = this.maxValues();

		// Front
		result.Add( new Vector3( thisMin.x, thisMin.y, thisMin.z ) );
		result.Add( new Vector3( thisMin.x, thisMax.y, thisMin.z ) );
		result.Add( new Vector3( thisMax.x, thisMax.y, thisMin.z ) );
		result.Add( new Vector3( thisMax.x, thisMin.y, thisMin.z ) );

		// Back
		result.Add( new Vector3( thisMin.x, thisMin.y, thisMax.z ) );
		result.Add( new Vector3( thisMin.x, thisMax.y, thisMax.z ) );
		result.Add( new Vector3( thisMax.x, thisMax.y, thisMax.z ) );
		result.Add( new Vector3( thisMax.x, thisMin.y, thisMax.z ) );

		return result;
	}

	public bool collision( CubeStaticObject other ){
		// For each vertex of the cube
		foreach( Vector3 v in other.vertices() ){
			if( this.isPointInside( v ) ){ return true; }
		}
		foreach( Vector3 v in this.vertices() ){
			if( other.isPointInside( v ) ){ return true; }
		}
		if( this.isPointInside( other.Position ) ){ return true; }
		if( other.isPointInside( this.Position ) ){ return true; }
		return false;
	}

	#region implemented abstract members of AbstractObject

	public override CubeStaticObject getBoundingBox ()
	{
		return this;
	}

	#endregion
}

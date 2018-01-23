using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundingBoxTree : CubeStaticObject{
	public enum interieurOrderAxe {
		axeX,axeY,axeZ
	};
		
	protected interieurOrderAxe orderAxe;   // Axe which is used to order the boxes inside
	AbstractObject inside;          		// A pointer to the object inside the box, if the box has only one component. If there are more than one, interieur will be null and the box will has boxes inside
	List<BoundingBoxTree> boxes;  			// A list of boxes inside the box. If there is only one object inside the box, boxes will be null

	public BoundingBoxTree( List<AbstractObject> interieur, interieurOrderAxe axe )
	: base( new Vector3(0,0,0),1,1,1 ){
		if( interieur.Count > 0 ){
			this.orderAxe = axe;

			if( axe == interieurOrderAxe.axeX ){
				interieur.Sort( compareBoundingBoxesX );
			} else if( axe == interieurOrderAxe.axeY ){
				interieur.Sort( compareBoundingBoxesY );
			} else {
				interieur.Sort( compareBoundingBoxesZ );
			}

			// adjust position and dimensions
			this.adjustValues( interieur );

			// create boxes (more than one object in interieur parameter) or interieur (just one object in interieur parameter)
			if( interieur.Count == 1 )
			{
				// create box to this primitive
				this.inside = interieur[0];
			}
			else
			{
				this.inside = null;
				this.boxes = new List<BoundingBoxTree>();

				int amountBoxes = 2;
				int amountObjPerBox = interieur.Count/amountBoxes;
				// Create the boxes inside : divide the list of objects
				for(int i = 0; i < amountBoxes; i++)
				{
					List<AbstractObject> subList = new List<AbstractObject>();
					// inserting the range of objects from interieur into subList. the last sublist will have the rest of the list
					for( int j = i*amountObjPerBox; j < (i==amountBoxes-1?interieur.Count:(i+1)*amountObjPerBox); j++ ){
						subList.Add( interieur[j] );
					}
					// Generating new boxes : the will be ordered based on the next axe
					if( axe == interieurOrderAxe.axeX ){
						this.boxes.Add( new BoundingBoxTree( subList, interieurOrderAxe.axeY ) );
					} else if( axe == interieurOrderAxe.axeX ){
						this.boxes.Add( new BoundingBoxTree( subList, interieurOrderAxe.axeZ ) );
					} else {
						this.boxes.Add( new BoundingBoxTree( subList, interieurOrderAxe.axeX ) );
					}
				}
			}
		}
	}

	// Verify the collision with the box and its children ( if previousIntersection is farest than the intersection with the box )
	public AbstractObject collision( CubeStaticObject box, AbstractObject obj ){
		// Intersection to external box Cube
		bool collideBox = this.getBoundingBox().collision( box );
		// Calculate internal intersection if the external one if smaller than the previousIntersection
		if( collideBox )
		{
			// If the box has only one child, check its intersection
			if( this.inside != null )
			{
				// This box is the same as inside
				if( obj != this.inside ){
					return this.inside;
				}
			}
			// If the box has boxes as children, check their intersection
			else
			{
				for (int i = 0; i < this.boxes.Count; i++)
				{
					AbstractObject collObj = this.boxes[i].collision( box, obj );
					if( collObj != null ){
						return collObj;
					}
				}
			}
		}
		return null;
	}

	protected AbstractObject getInside(){
		return this.inside;
	}

	// order list based on the axe
	static int compareBoundingBoxesX( AbstractObject a, AbstractObject b ) {
		float res = a.getBoundingBox().minValues().x - b.getBoundingBox().minValues().x;
		return (res>0 ? 1 : (res<0 ? -1 : 0));
	}
	static int compareBoundingBoxesY( AbstractObject a, AbstractObject b ) {
		float res = a.getBoundingBox().minValues().y - b.getBoundingBox().minValues().y;
		return (res>0 ? 1 : (res<0 ? -1 : 0));
	}
	static int compareBoundingBoxesZ( AbstractObject a, AbstractObject b ) {
		float res = a.getBoundingBox().minValues().z - b.getBoundingBox().minValues().z;
		return (res>0 ? 1 : (res<0 ? -1 : 0));
	}

	void adjustValues( List<AbstractObject> objs )
	{
		Vector3 minValues = objs[0].getBoundingBox().minValues();
		Vector3 maxValues = objs[0].getBoundingBox().maxValues();
		for(int i=0;i<objs.Count;i++){
			Vector3 current_minValues = objs[i].getBoundingBox().minValues();
			Vector3 current_maxValues = objs[i].getBoundingBox().maxValues();
			if( minValues.x > current_minValues.x ){ minValues.x = current_minValues.x; }
			if( minValues.y > current_minValues.y ){ minValues.y = current_minValues.y; }
			if( minValues.z > current_minValues.z ){ minValues.z = current_minValues.z; }

			if( maxValues.x < current_maxValues.x ){ maxValues.x = current_maxValues.x; }
			if( maxValues.y < current_maxValues.y ){ maxValues.y = current_maxValues.y; }
			if( maxValues.z < current_maxValues.z ){ maxValues.z = current_maxValues.z; }
		}

		// Dimensions
		this.Width = maxValues.x - minValues.x;
		this.Height = maxValues.y - minValues.y;
		this.Depth = maxValues.z - minValues.z;
		// Center
		this.Position = new Vector3( minValues.x+this.Width/2f, minValues.y+this.Height/2f, minValues.z+this.Depth/2f );
	}

	public string toString( int level )
	{
		string result = "";
		for(int i=0; i<level; i++){
			result += "-";
		}
		if( this.inside != null ){
			result += " obj\n";
		}
		else{
			result += " BOX\n";
			for( int i=0; i<this.boxes.Count; i++ ){
				result += this.boxes[i].toString( level+1 );
			}
		}
		return result;
	}
}

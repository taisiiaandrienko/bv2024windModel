/*
* The SEI Software Open Source License, Version 1.0
*
* Copyright (c) 2004, Solution Engineering, Inc.
* All rights reserved.
*
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions
* are met:
*
* 1. Redistributions of source code must retain the above copyright
*    notice, this list of conditions and the following disclaimer. 
*
* 2. The end-user documentation included with the redistribution,
*    if any, must include the following acknowledgment:
*       "This product includes software developed by the
*        Solution Engineering, Inc. (http://www.seisw.com/)."
*    Alternately, this acknowledgment may appear in the software itself,
*    if and wherever such third-party acknowledgments normally appear.
*
* 3. The name "Solution Engineering" must not be used to endorse or
*    promote products derived from this software without prior
*    written permission. For written permission, please contact
*    admin@seisw.com.
*
* THIS SOFTWARE IS PROVIDED ``AS IS'' AND ANY EXPRESSED OR IMPLIED
* WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
* OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED.  IN NO EVENT SHALL SOLUTION ENGINEERING, INC. OR
* ITS CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
* SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
* LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF
* USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
* OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT
* OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
* SUCH DAMAGE.
* ====================================================================
*/

using System.Drawing;

namespace Macs3.Core.Mathematics.GeneralPolygonClipperLibrary
{
	
	// <summary> <code>Poly</code> is an interface to a complex polygon.  <code>Poly</code> polygons
	// can consist of  multiple "inner" polygons that can be disjoint and can be considered holes.
	// <p>
	// Currently, this interface supports two concepts:
	// <ul>
	// <li>a set of inner polygons</li>
	// <li>a set of points of a polygon</li>
	// </ul>
	// <p>
	// <b>Refactoring.</b> This would be a good place for some refactoring to create
	// a ComplexPoly and an InnerPoly or something so that these two concepts are broken
	// out.  One might also consider changing from an interface to an abstract class,
	// so the methods <code>isContributing()</code> and <code>setContributing()</code>
	// could have an access of package only.  Or, the <code>Clip</code> algorithm could 
	// not store this information in the <code>Poly</code>.
	// <p>
	// <b>Assumptions.</b> The methods that access the polygon as though it were a set of points assume
	// it is accessing the first polygon in the list of inner polygons.  It is also assumed that
	// inner polygons do not have more inner polygons.
	// *
	// </summary>
	// <author>   Dan Bridenbecker, Solution Engineering, Inc.
	// 
	// </author>
	public interface IPolygon
		{
            PointF[] Points
            {
                get;
            }
            
            /// <summary> Return true if the polygon is empty
			/// </summary>
			bool Empty
			{
				get;
				
			}

			/// <summary> Returns the bounding rectangle of this polygon.
			/// </summary>
			RectangleF Bounds
			{
				get;
				
			}
			/// <summary> Returns the number of inner polygons - inner polygons are assumed to return one here.
			/// </summary>
			int NumInnerPoly
			{
				get;
				
			}
			/// <summary> Return the number points of the first inner polygon
			/// </summary>
			int NumPoints
			{
				get;
				
			}
			/// <summary> Return true if this polygon is a hole.  Holes are assumed to be inner polygons of
			/// a more complex polygon.
			/// *
			/// @throws IllegalStateException if called on a complex polygon.
			/// </summary>
			bool Hole
			{
				get;
				
			}
			/// <summary> Set whether or not this polygon is a hole.  Cannot be called on a complex polygon.
			/// *
			/// @throws IllegalStateException if called on a complex polygon.
			/// </summary>
			bool IsHole
			{
				set;
				
			}
			/// <summary> Return the area of the polygon in square units.
			/// </summary>
			double Area
			{
				get;
				
			}
			// ----------------------
			// --- Public Methods ---
			// ----------------------
			/// <summary> Remove all of the points.  Creates an empty polygon.
			/// </summary>
			void  clear();
			/// <summary> Add a point to the first inner polygon.
			/// </summary>
			void  add(double x, double y);
			/// <summary> Add a point to the first inner polygon.
			/// </summary>
			void  add(PointF p);
			/// <summary> Add a rectangle to the first inner polygon.
			/// </summary>
			void  addRectangle(double x, double y, double xsize, double ysize);
			/// <summary> Add an inner polygon to this polygon - assumes that adding polygon does not
			/// have any inner polygons.
			/// </summary>
			void  add(IPolygon p);
			/// <summary> Returns the polygon at this index.
			/// </summary>
			IPolygon getInnerPoly(int polyIndex);
			/// <summary> Return the X value of the point at the index in the first inner polygon
			/// </summary>
			double getX(int index);
			/// <summary> Return the Y value of the point at the index in the first inner polygon
			/// </summary>
			double getY(int index);
			/// <summary> Return true if the given inner polygon is contributing to the set operation.
			/// This method should NOT be used outside the Clip algorithm.
			/// </summary>
			bool isContributing(int polyIndex);
			/// <summary> Set whether or not this inner polygon is constributing to the set operation.
			/// This method should NOT be used outside the Clip algorithm.
			/// </summary>
			void  setContributing(int polyIndex, bool contributes);
			/// <summary> Return a Poly that is the intersection of this polygon with the given polygon.
			/// The returned polygon could be complex.
			/// </summary>
			IPolygon intersection(IPolygon p);
			/// <summary> Return a Poly that is the multiintersection of this polygon with the given polygon.
			/// The returned polygon could be complex.
			/// </summary>
			IPolygon multiintersection(IPolygon p);
			/// <summary> Return a Poly that is the union of this polygon with the given polygon.
			/// The returned polygon could be complex.
			/// </summary>
			IPolygon union(IPolygon p);
			/// <summary> Return a Poly that is the exclusive-or of this polygon with the given polygon.
			/// The returned polygon could be complex.
			/// </summary>
			IPolygon xor(IPolygon p);
			/// <summary> Return a Poly that is the exclusive-or of this polygon with the given polygon.
			/// The returned polygon could be complex.
			/// </summary>
			IPolygon diff(IPolygon p);

            IPolygon duplicate();

            long PointsHashCode { get; }
    }
}
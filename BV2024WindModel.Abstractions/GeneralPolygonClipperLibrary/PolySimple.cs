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

using System.Collections.Generic;
using System;
using System.Drawing;

namespace Macs3.Core.Mathematics.GeneralPolygonClipperLibrary
{
    // <summary> <code>PolySimple</code> is a simple polygon - contains only one inner polygon.
	// <p>
	// <strong>WARNING:</strong> This type of <code>Poly</code> cannot be used for an
	// inner polygon that is a hole.
	// *
	// </summary>
	// <author>   Dan Bridenbecker, Solution Engineering, Inc.
	// 
	// </author>
	public class PolySimple : IPolygon
	{
		private void  InitBlock()
		{
            m_List = new List<PointF>();
		}
		/// <summary> Return true if the polygon is empty
		/// </summary>
		virtual public bool Empty
		{
			get
			{
				return (m_List.Count==0);
			}
			
		}
		/// <summary> Returns the bounding rectangle of this polygon.
		/// </summary>
		virtual public RectangleF Bounds
		{
			get
			{
				double xmin = System.Double.MaxValue;
				double ymin = System.Double.MaxValue;
				double xmax = - System.Double.MaxValue;
				double ymax = - System.Double.MaxValue;
				
				for (int i = 0; i < m_List.Count; i++)
				{
					double x = getX(i);
					double y = getY(i);
					if (x < xmin)
						xmin = x;
					if (x > xmax)
						xmax = x;
					if (y < ymin)
						ymin = y;
					if (y > ymax)
						ymax = y;
				}
				
				return new RectangleF((float)xmin, (float)ymin, (float)(xmax - xmin),(float) (ymax - ymin));
			}
			
		}
		/// <summary> Always returns 1.
		/// </summary>
		virtual public int NumInnerPoly
		{
			get
			{
				return 1;
			}
			
		}
		/// <summary> Return the number points of the first inner polygon
		/// </summary>
		virtual public int NumPoints
		{
			get
			{
				return m_List.Count;
			}
			
		}
		/// <summary> Always returns false since PolySimples cannot be holes.
		/// </summary>
		virtual public bool Hole
		{
			get
			{
				return false;
			}
			
		}
		/// <summary> Throws IllegalStateException if called.
		/// </summary>
		virtual public bool IsHole
		{
			set
			{
				throw new System.SystemException("PolySimple cannot be a hole");
			}
			
		}
		// <summary> Returns the area of the polygon.
		// <p>
		// The algorithm for the area of a complex polygon was take from
		// code by Joseph O'Rourke author of " Computational Geometry in C".
		// </summary>
		virtual public double Area
		{
			get
			{
				if (NumPoints < 3)
				{
					return 0.0;
				}
				double ax = getX(0);
				double ay = getY(0);
				double area = 0.0;
				for (int i = 1; i < (NumPoints - 1); i++)
				{
					double bx = getX(i);
					double by = getY(i);
					double cx = getX(i + 1);
					double cy = getY(i + 1);
					double tarea = ((cx - bx) * (ay - by)) - ((ax - bx) * (cy - by));
					area += tarea;
				}
				area = 0.5 * System.Math.Abs(area);
				return area;
			}
			
		}
		// -----------------
		// --- Constants ---
		// -----------------
		
		// ------------------------
		// --- Member Variables ---
		// ------------------------
		/// <summary> The list of Point2D objects in the polygon.
		/// </summary>
		//UPGRADE_NOTE: The initialization of  'm_List' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
		protected internal List<PointF> m_List;
		
		/// <summary>Flag used by the Clip algorithm 
		/// </summary>
		private bool m_Contributes = true;
		
		// --------------------
		// --- Constructors ---
		// --------------------
		/// <summary>Creates a new instance of PolySimple 
		/// </summary>
		public PolySimple()
		{
            _pointsHashCode = -1;
            InitBlock();
		}

        public PolySimple(PolySimple poly)
            : this()
        {
            // Copy the contents of the array
            int n = poly.NumPoints;
            for (int i = 0; i < n; i++)
            {
                m_List.Add(poly.Points[i]);
            }
        }

	    private long GetPointsHashCode()
	    {
            long ret = 0;
	        int n = NumPoints;
            long X, Y;
            long PointHash;
            for (int i = 0; i < n; i++)
	        {
	            X = (long) Math.Round(getX(i) * 1000);
	            Y = (long) Math.Round(getY(i) * 1000);
	            PointHash = X + Y * 999959;
	            ret ^= PointHash << i;
	        }
	        return ret;
	    }

        /// <summary>
        /// Duplicates the polygon and returns the results.
        /// </summary>
        public IPolygon duplicate()
        {
            return new PolySimple(this);
        }



        public double ShapeCoeff;
        public double HeightCoeff;

		// ----------------------
		// --- Object Methods ---
		// ----------------------
		// <summary> Return true if the given object is equal to this one.
		// <p>
		// <strong>WARNING:</strong> This method failse if the first point
		// appears more than once in the list.
		// </summary>
		public  override bool Equals(System.Object obj)
		{
			if (!(obj is PolySimple))
			{
				return false;
			}
			PolySimple that = (PolySimple) obj;
			
			int this_num = this.m_List.Count;
			int that_num = that.m_List.Count;
			if (this_num != that_num)
				return false;
			
			// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			// !!! WARNING: This is not the greatest algorithm.  It fails if !!!
			// !!! the first point in "this" poly appears more than once.    !!!
			// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			if (this_num > 0)
			{
				double this_x = this.getX(0);
				double this_y = this.getY(0);
				int that_first_index = - 1;
				for (int that_index = 0; (that_first_index == - 1) && (that_index < that_num); that_index++)
				{
					double that_x = that.getX(that_index);
					double that_y = that.getY(that_index);
					if ((this_x == that_x) && (this_y == that_y))
					{
						that_first_index = that_index;
					}
				}
				if (that_first_index == - 1)
					return false;
				int that_index2 = that_first_index;
				for (int this_index = 0; this_index < this_num; this_index++)
				{
					this_x = this.getX(this_index);
					this_y = this.getY(this_index);
					double that_x = that.getX(that_index2);
					double that_y = that.getY(that_index2);
					
					if ((this_x != that_x) || (this_y != that_y))
						return false;
					
					that_index2++;
					if (that_index2 >= that_num)
					{
						that_index2 = 0;
					}
				}
			}
			return true;
		}

	    private long _pointsHashCode = -1;

	    public long PointsHashCode
	    {
	        get
	        {
	            if (_pointsHashCode == -1)
                    _pointsHashCode = GetPointsHashCode();
	            return _pointsHashCode;
	        }
	    }

		// <summary> Return the hashCode of the object.
		// <p>
		// <strong>WARNING:</strong>Hash and Equals break contract.
		// *
		// </summary>
		// <returns> an integer value that is the same for two objects
		// whenever their internal representation is the same (equals() is true)
		// 
		// </returns>
		public override int GetHashCode()
		{
			// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			// !!! WARNING:  This hash and equals break the contract. !!!
			// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			int result = 17;
			result = 37 * result + m_List.GetHashCode();
			return result;
		}
		
		/// <summary> Return a string briefly describing the polygon.
		/// </summary>
		public override System.String ToString()
		{
			System.String s="PolySimple: num_points=" + NumPoints;
			for (int j = 0; j < NumPoints; j++)
			{
				s+="\r\n  "+getX(j) + "  " + getY(j);
			}
			return s;
		}
		
		// --------------------
		// --- Poly Methods ---
		// --------------------
		/// <summary> Remove all of the points.  Creates an empty polygon.
		/// </summary>
		public virtual void  clear()
		{
			m_List.Clear();
            _pointsHashCode = -1;
        }
		
		/// <summary> Add a point to the first inner polygon.
		/// </summary>
		public virtual void  add(double x, double y)
		{
            _pointsHashCode = -1;
            add(new PointF((float)x, (float)y));
		}
		
		/// <summary> Add a point to the first inner polygon.
		/// </summary>
		public virtual void  add(PointF p)
		{
            _pointsHashCode = -1;
            m_List.Add(p);
		}
		
		/// <summary> Add a rectangle to the first inner polygon.
		/// </summary>
		public virtual void  addRectangle(double x, double y, double xsize, double ysize)
		{
			add(x,y);
			add(x+xsize,y);
			add(x+xsize,y+ysize);
			add(x,y+ysize);
		}

        public virtual PointF[] Points
        {
            get
            {
                PointF[] pts = null;

                if (NumPoints > 0)
                {
                    pts = new PointF[NumPoints];
                    for (int j = 0; j < NumPoints; j++)
                    {
                        pts[j].X = Convert.ToSingle(getX(j));
                        pts[j].Y = Convert.ToSingle(getY(j));
                    }
                }
                return pts;
            }
        }
        
        /// <summary> Throws IllegalStateexception if called
		/// </summary>
		public virtual void  add(IPolygon p)
		{
			throw new System.SystemException("Cannot add poly to a simple poly.");
		}
		
		/// <summary> Return a Poly that is the union of two polygons.
		/// The returned polygon is simple.
		/// *
		/// </summary>
		/// <returns> The returned Poly is of type PolySimple
		/// 
		/// </returns>
		public static IPolygon operator| (PolySimple left, PolySimple right) 
		{
			PolySimple tmpPoly = new PolySimple();
			tmpPoly = tmpPoly.union(left) as PolySimple;
			tmpPoly = tmpPoly.union(right) as PolySimple;
			return( tmpPoly );
		}
		
		/// <summary> Return a Poly that is the intersection of two polygons.
		/// The returned polygon is simple.
		/// *
		/// </summary>
		/// <returns> The returned Poly is of type PolySimple
		/// 
		/// </returns>
		public static IPolygon operator& (PolySimple left, PolySimple right) 
		{
			PolySimple tmpPoly = new PolySimple();
			tmpPoly = tmpPoly.union(left) as PolySimple;
			tmpPoly = tmpPoly.intersection(right) as PolySimple;
			return( tmpPoly );
		}
		
		/// <summary> Return a Poly that is the exclusive-or of left polygon with the right polygon.
		/// The returned polygon is simple.
		/// *
		/// </summary>
		/// <returns> The returned Poly is of type PolySimple
		/// 
		/// </returns>
		public static IPolygon operator^ (PolySimple left, PolySimple right) 
		{
			PolySimple tmpPoly = new PolySimple();
			tmpPoly = tmpPoly.union(left) as PolySimple;
			tmpPoly = tmpPoly.xor(right) as PolySimple;
			return( tmpPoly );
		}
		
		/// <summary> Return a Poly that is the difference between left polygon and the right polygon.
		/// The returned polygon is simple.
		/// *
		/// </summary>
		/// <returns> The returned Poly is of type PolySimple
		/// 
		/// </returns>
		public static IPolygon operator- (PolySimple left, PolySimple right) 
		{
			PolySimple tmpPoly = new PolySimple();
			tmpPoly = tmpPoly.union(left) as PolySimple;
			tmpPoly = tmpPoly.diff(right) as PolySimple;
			return( tmpPoly );
		}
		
		/// <summary> Return a Poly that is the union of two polygons.
		/// The returned polygon is simple.
		/// *
		/// </summary>
		/// <returns> The returned Poly is of type PolySimple
		/// 
		/// </returns>
		public static IPolygon operator+ (PolySimple left, PolySimple right) 
		{
			PolySimple tmpPoly = new PolySimple();
			tmpPoly = tmpPoly.union(left) as PolySimple;
			tmpPoly = tmpPoly.union(right) as PolySimple;
			return( tmpPoly );
		}

		/// <summary> Returns <code>this</code> if <code>polyIndex = 0</code>, else it throws
		/// IllegalStateException.
		/// </summary>
		public virtual IPolygon getInnerPoly(int polyIndex)
		{
			if (polyIndex != 0)
			{
				throw new System.SystemException("PolySimple only has one poly");
			}
			return this;
		}
		
		
		
		/// <summary> Return the X value of the point at the index in the first inner polygon
		/// </summary>
		public virtual double getX(int index)
		{
			return ((PointF) m_List[index]).X;
		}
		
		/// <summary> Return the Y value of the point at the index in the first inner polygon
		/// </summary>
		public virtual double getY(int index)
		{
			return ((PointF) m_List[index]).Y;
		}
		
		
		
		/// <summary> Return true if the given inner polygon is contributing to the set operation.
		/// This method should NOT be used outside the Clip algorithm.
		/// *
		/// @throws IllegalStateException if <code>polyIndex != 0</code>
		/// </summary>
		public virtual bool isContributing(int polyIndex)
		{
			if (polyIndex != 0)
			{
				throw new System.SystemException("PolySimple only has one poly");
			}
			return m_Contributes;
		}
		
		/// <summary> Set whether or not this inner polygon is constributing to the set operation.
		/// This method should NOT be used outside the Clip algorithm.
		/// *
		/// @throws IllegalStateException if <code>polyIndex != 0</code>
		/// </summary>
		public virtual void  setContributing(int polyIndex, bool contributes)
		{
			if (polyIndex != 0)
			{
				throw new System.SystemException("PolySimple only has one poly");
			}
			m_Contributes = contributes;
		}
		
		/// <summary> Return a Poly that is the intersection of this polygon with the given polygon.
		/// The returned polygon is simple.
		/// *
		/// </summary>
		/// <returns> The returned Poly is of type PolySimple
		/// 
		/// </returns>
		public virtual IPolygon intersection(IPolygon p)
		{
			return Clip.intersection(this, p, this.GetType());
		}

		/// <summary> Return a Poly that is the multiintersection of this polygon with the given polygon.
		/// The returned polygon is simple.
		/// *
		/// </summary>
		/// <returns> The returned Poly is of type PolySimple
		/// 
		/// </returns>
		public virtual IPolygon multiintersection(IPolygon p)
		{
			throw new System.SystemException("Cannot multiintersect in a simple poly.");
		}

		/// <summary> Return a Poly that is the union of this polygon with the given polygon.
		/// The returned polygon is simple.
		/// *
		/// </summary>
		/// <returns> The returned Poly is of type PolySimple
		/// 
		/// </returns>
		public virtual IPolygon union(IPolygon p)
		{
			return Clip.Union(this, p, this.GetType());
		}
		
		/// <summary> Return a Poly that is the difference between this polygon and given polygon.
		/// The returned polygon is simple.
		/// *
		/// </summary>
		/// <returns> the returned Poly will be an instance of PolySimple.
		/// 
		/// </returns>
		public virtual IPolygon diff(IPolygon p)
		{
			return Clip.Diff(this, p, this.GetType());
		}

		/// <summary> Return a Poly that is the exclusive-or of this polygon with the given polygon.
		/// The returned polygon is simple.
		/// *
		/// </summary>
		/// <returns> The returned Poly is of type PolySimple
		/// 
		/// </returns>
		public virtual IPolygon xor(IPolygon p)
		{
			return Clip.Xor(p, this, this.GetType());
		}
		
		
		// -----------------------
		// --- Package Methods ---
		// -----------------------
	}
}
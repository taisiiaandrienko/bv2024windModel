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

using System;
using System.Collections;
using System.Drawing;

namespace Macs3.Core.Mathematics.GeneralPolygonClipperLibrary
{
    // <summary> <code>PolyDefault</code> is a default <code>Poly</code> implementation.  
	// It provides support for both complex and simple polygons.  A <i>complex polygon</i> 
	// is a polygon that consists of more than one polygon.  A <i>simple polygon</i> is a 
	// more traditional polygon that contains of one inner polygon and is just a 
	// collection of points.
	// <p>
	// <b>Implementation Note:</b> If a point is added to an empty <code>PolyDefault</code>
	// object, it will create an inner polygon of type <code>PolySimple</code>.
	// *
	// </summary>
	// <seealso cref="">PolySimple
	// *
	// </seealso>
	// <author>   Dan Bridenbecker, Solution Engineering, Inc./// 
	// </author>
	public class PolyDefault : IPolygon
	{
		private void  InitBlock()
		{
			m_List = new ArrayList();
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
		/// <strong>WARNING</strong> Not supported on complex polygons.
		/// </summary>
		virtual public RectangleF Bounds
		{
			get
			{
				if (m_List.Count == 0)
				{
					//return new RectangleF.Double();
					return RectangleF.Empty;
				}
				else if (m_List.Count == 1)
				{
					IPolygon ip = getInnerPoly(0);
					return ip.Bounds;
				}
				else
				{
					throw new Exception("getBounds not supported on complex poly.");
				}
			}
			
		}
		/// <summary> Returns the number of inner polygons - inner polygons are assumed to return one here.
		/// </summary>
		virtual public int NumInnerPoly
		{
			get
			{
				return m_List.Count;
			}
			
		}
		/// <summary> Return the number points of the first inner polygon
		/// </summary>
		virtual public int NumPoints
		{
			get
			{
				return ((IPolygon) m_List[0]).NumPoints;
			}
			
		}
		/// <summary> Return true if this polygon is a hole.  Holes are assumed to be inner polygons of
		/// a more complex polygon.
		/// *
		/// @throws IllegalStateException if called on a complex polygon.
		/// </summary>
		virtual public bool Hole
		{
			get
			{
				if (m_List.Count > 1)
				{
					throw new SystemException("Cannot call on a poly made up of more than one poly.");
				}
				return m_IsHole;
			}
			
		}
		/// <summary> Set whether or not this polygon is a hole.  Cannot be called on a complex polygon.
		/// *
		/// @throws IllegalStateException if called on a complex polygon.
		/// </summary>
		virtual public bool IsHole
		{
			set
			{
				if (m_List.Count > 1)
				{
					throw new SystemException("Cannot call on a poly made up of more than one poly.");
				}
				m_IsHole = value;
			}
			
		}
		/// <summary> Return the area of the polygon in square units.
		/// </summary>
		virtual public double Area
		{
			get
			{
				double area = 0.0;
				for (int i = 0; i < NumInnerPoly; i++)
				{
					IPolygon p = getInnerPoly(i);
					double tarea = p.Area * (p.Hole?- 1.0:1.0);
					area += tarea;
				}
				return area;
			}
			
		}
		// -----------------
		// --- Constants ---
		// -----------------
		
		// ------------------------
		// --- Member Variables ---
		// ------------------------
		/// <summary> Only applies to the first poly and can only be used with a poly that contains one poly
		/// </summary>
		private bool m_IsHole = false;
		//UPGRADE_NOTE: The initialization of  'm_List' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
		public ArrayList m_List;
		
		// --------------------
		// --- Constructors ---
		// --------------------
		/// <summary>Creates a new instance of PolyDefault 
		/// </summary>
		public PolyDefault()
		{
            _pointsHashCode = -1;
            InitBlock();
		}

        /// <summary>
        /// Copy constructor that duplicates a polygon.
        /// </summary>
        public PolyDefault(PolyDefault poly)
            : this()
        {
            // Copy the contents of the array
            for (int i = 0; i < poly.NumInnerPoly; i++)
            {
                // Copy this polygon
                IPolygon ip = poly.getInnerPoly(i);
                m_List.Add(ip.duplicate());
            }
        }

        /// <summary>
		/// Duplicates the polygon and returns the results.
		/// </summary>
		public IPolygon duplicate()
        {
            return new PolyDefault(this);
        }

        public PolyDefault(bool isHole) : this()
		{
			m_IsHole = isHole;
		}
		
		// ----------------------
		// --- Object Methods ---
		// ----------------------
		/// <summary> Return true if the given object is equal to this one.
		/// </summary>
		public  override bool Equals(Object obj)
		{
			if (!(obj is PolyDefault))
			{
				return false;
			}
			PolyDefault that = (PolyDefault) obj;
			
			if (this.m_IsHole != that.m_IsHole)
				return false;
			if (!this.m_List.Equals(that.m_List))
				return false;
			
			return true;
		}
		
		/// <summary> Return the hashCode of the object.
		/// *
		/// </summary>
		/// <returns> an integer value that is the same for two objects
		/// whenever their internal representation is the same (equals() is true)
		/// *
		/// </returns>
		public override int GetHashCode()
		{
			int result = 17;
			result = 37 * result + m_List.GetHashCode();
			return result;
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

	    private long GetPointsHashCode()
	    {
	        long ret = 0;
	        for (int i = 0; i < m_List.Count; i++)
	        {
	            IPolygon p = getInnerPoly(i);
	            ret ^= p.PointsHashCode << i;
	        }
	        return ret;
	    }

	    /// <summary>*
        /// </summary>
        public override String ToString()
		{
			String s=base.ToString();
			for (int i = 0; i < m_List.Count; i++)
			{
				IPolygon p = getInnerPoly(i);
				s+="\r\n"+"InnerPoly(" + i + "):";
				s+="\r\n"+"  Hole=" + p.Hole;
				for (int j = 0; j < p.NumPoints; j++)
				{
					s+="\r\n  "+p.getX(j) + "  " + p.getY(j);
				}
			}
			return s;
		}
		
		// ----------------------
		// --- Public Methods ---
		// ----------------------
		/// <summary> Remove all of the points.  Creates an empty polygon.
		/// </summary>
		public virtual void  clear()
		{
            _pointsHashCode = -1;
            m_List.Clear();
		}
		
		// <summary> Add a point to the first inner polygon.
		// <p>
		// <b>Implementation Note:</b> If a point is added to an empty PolyDefault object,
		// it will create an inner polygon of type <code>PolySimple</code>.
		// </summary>
		public virtual void add(double x, double y)
		{
            _pointsHashCode = -1;
            add(new PointF((float)x, (float)y));
		}
		
		// <summary> Add a point to the first inner polygon.
		// <p>
		// <b>Implementation Note:</b> If a point is added to an empty PolyDefault object,
		// it will create an inner polygon of type <code>PolySimple</code>.
		// </summary>
		public virtual void add(PointF p)
		{
            _pointsHashCode = -1;
            if (m_List.Count == 0)
			{
				m_List.Add(new PolySimple());
			}
			((IPolygon) m_List[0]).add(p);
		}
		
		// <summary> Add points to the first inner polygon.
		// <p>
		// <b>Implementation Note:</b> If a point is added to an empty PolyDefault object,
		// it will create an inner polygon of type <code>PolySimple</code>.
		// </summary>
		public virtual void add(PointF [] pts)
		{
            _pointsHashCode = -1;
            if (m_List.Count == 0)
				m_List.Add(new PolySimple());

			foreach(PointF pt in pts)
				((IPolygon) m_List[0]).add(pt);
		}

		// <summary> Add points to the first inner polygon.
		// <p>
		// <b>Implementation Note:</b> If a point is added to an empty PolyDefault object,
		// it will create an inner polygon of type <code>PolySimple</code>.
		// </summary>
		public virtual void add(Point [] pts)
		{
            _pointsHashCode = -1;
            if (m_List.Count == 0)
				m_List.Add(new PolySimple());

			foreach(Point pt in pts)
				((IPolygon) m_List[0]).add(pt);
		}

		/// <summary> Add an inner polygon to this polygon - assumes that adding polygon does not
		/// have any inner polygons.
		/// *
		/// @throws IllegalStateException if the number of inner polygons is greater than
		/// zero and this polygon was designated a hole.  This would break the assumption
		/// that only simple polygons can be holes.
		/// </summary>
		public virtual void add(IPolygon p)
		{
	        _pointsHashCode = -1;
            if ((m_List.Count > 0) && m_IsHole)
			{
				throw new SystemException("Cannot add polys to something designated as a hole.");
			}
			m_List.Add(p);
		}
		
		/// <summary> Add a rectangle to the first inner polygon.
		/// </summary>
		public virtual void addRectangle(double x, double y, double xsize, double ysize)
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
		
		/// <summary> Add a rectangle to the first inner polygon.
		/// </summary>
		public virtual void  addRectangle(RectangleF r)
		{
			add(r.X, r.Y);
			add(r.X+r.Width, r.Y);
			add(r.X+r.Width, r.Y+r.Height);
			add(r.X, r.Y+r.Height);
		}
		
		/// <summary> Returns the polygon at this index.
		/// </summary>
		public virtual IPolygon getInnerPoly(int polyIndex)
		{
			return (IPolygon) m_List[polyIndex];
		}
		
		
		
		/// <summary> Return the X value of the point at the index in the first inner polygon
		/// </summary>
		public virtual double getX(int index)
		{
			return ((IPolygon) m_List[0]).getX(index);
		}
		
		/// <summary> Return the Y value of the point at the index in the first inner polygon
		/// </summary>
		public virtual double getY(int index)
		{
			return ((IPolygon) m_List[0]).getY(index);
		}
		
		
		
		/// <summary> Return true if the given inner polygon is contributing to the set operation.
		/// This method should NOT be used outside the Clip algorithm.
		/// </summary>
		public virtual bool isContributing(int polyIndex)
		{
			return ((IPolygon) m_List[polyIndex]).isContributing(0);
		}
		
		/// <summary> Set whether or not this inner polygon is constributing to the set operation.
		/// This method should NOT be used outside the Clip algorithm.
		/// *
		/// @throws IllegalStateException if called on a complex polygon
		/// </summary>
		public virtual void  setContributing(int polyIndex, bool contributes)
		{
			if (m_List.Count != 1)
			{
				string sPolyError="Only applies to polys of size 1! This poly has size="+m_List.Count;
				throw new SystemException(sPolyError);
			}
			((IPolygon) m_List[polyIndex]).setContributing(0, contributes);
		}
		
		/// <summary> Return a Poly that is the intersection of this polygon with the given polygon.
		/// The returned polygon could be complex.
		/// *
		/// </summary>
		/// <returns> the returned Poly will be an instance of PolyDefault.
		/// 
		/// </returns>
		public virtual IPolygon multiintersection(IPolygon p)
		{
			IPolygon pRet=new PolyDefault();
			for (int iThisPoly=0; iThisPoly<this.NumInnerPoly; iThisPoly++)
			{
				//Poly pThis=this.getInnerPoly(iThisPoly);
				
				PolyDefault pThis=new PolyDefault();
				pThis.add(this.getInnerPoly(iThisPoly));
				
				if (!pThis.Hole)
				{
					for (int iPoly=0; iPoly<p.NumInnerPoly; iPoly++)
					{
						IPolygon pParameter=p.getInnerPoly(iPoly);
						if (!pParameter.Hole)
						{
							PolyDefault pTemp=pThis.intersection(pParameter) as PolyDefault;
							if (!pTemp.Empty)
							{
								pRet=pRet.union(pTemp) as PolyDefault;
							}
						}
					}
				}
			}
			return pRet;
		}

		/// <summary> Return a Poly that is the intersection of this polygon with the given polygon.
		/// The returned polygon could be complex.
		/// *
		/// </summary>
		/// <returns> the returned Poly will be an instance of PolyDefault.
		/// 
		/// </returns>
		public virtual IPolygon intersection(IPolygon p)
		{
			return Clip.intersection(p, this, this.GetType());
		}

		/// <summary> Return a Poly that is the union of this polygon with the given polygon.
		/// The returned polygon could be complex.
		/// *
		/// </summary>
		/// <returns> the returned Poly will be an instance of PolyDefault.
		/// 
		/// </returns>
		public virtual IPolygon union(IPolygon p)
		{
			return Clip.Union(p, this, this.GetType());
		}

		/// <summary> Return a Poly that is the difference between this polygon and given polygon.
		/// The returned polygon could be complex.
		/// *
		/// </summary>
		/// <returns> the returned Poly will be an instance of PolyDefault.
		/// 
		/// </returns>
		public virtual IPolygon diff(IPolygon p)
		{
			return Clip.Diff(this, p, this.GetType());
		}
		
		/// <summary> Return a Poly that is the exclusive-or of this polygon with the given polygon.
		/// The returned polygon could be complex.
		/// *
		/// </summary>
		/// <returns> the returned Poly will be an instance of PolyDefault.
		/// 
		/// </returns>
		public virtual IPolygon xor(IPolygon p)
		{
			return Clip.Xor(p, this, this.GetType());
		}
		
		/// <summary> Return a Poly that is the union of two poligons.
		/// The returned polygon could be complex.
		/// *
		/// </summary>
		/// <returns> the returned Poly will be an instance of PolyDefault.
		/// 
		/// </returns>
		public static IPolygon operator| (PolyDefault left, PolyDefault right) 
		{
			PolyDefault tmpPoly = new PolyDefault();
			tmpPoly = tmpPoly.union(left) as PolyDefault;
			tmpPoly = tmpPoly.union(right) as PolyDefault;
			return( tmpPoly );
		}
		
		/// <summary> Return a Poly that is the intersection of two poligons.
		/// The returned polygon could be complex.
		/// *
		/// </summary>
		/// <returns> the returned Poly will be an instance of PolyDefault.
		/// 
		/// </returns>
		public static IPolygon operator& (PolyDefault left, PolyDefault right) 
		{
			PolyDefault tmpPoly = new PolyDefault();
			tmpPoly = tmpPoly.union(left) as PolyDefault;
			tmpPoly = tmpPoly.intersection(right) as PolyDefault;
			return( tmpPoly );
		}
		
		/// <summary> Return a Poly that is the exclusive-or of the left polygon with the right polygon.
		/// The returned polygon could be complex.
		/// *
		/// </summary>
		/// <returns> the returned Poly will be an instance of PolyDefault.
		/// 
		/// </returns>
		public static IPolygon operator^ (PolyDefault left, PolyDefault right) 
		{
			PolyDefault tmpPoly = new PolyDefault();
			tmpPoly = tmpPoly.union(left) as PolyDefault;
			tmpPoly = tmpPoly.xor(right) as PolyDefault;
			return( tmpPoly );
		}
		
		/// <summary> Return a Poly that is the difference between the left polygon and the right polygon.
		/// The returned polygon could be complex.
		/// *
		/// </summary>
		/// <returns> the returned Poly will be an instance of PolyDefault.
		/// 
		/// </returns>
		public static IPolygon operator- (PolyDefault left, PolyDefault right) 
		{
			PolyDefault tmpPoly = new PolyDefault();
			tmpPoly = tmpPoly.union(left) as PolyDefault;
			tmpPoly = tmpPoly.diff(right) as PolyDefault;
			return( tmpPoly );
		}
		
		/// <summary> Return a Poly that is the union of two poligons.
		/// The returned polygon could be complex.
		/// *
		/// </summary>
		/// <returns> the returned Poly will be an instance of PolyDefault.
		/// 
		/// </returns>
		public static IPolygon operator+ (PolyDefault left, PolyDefault right) 
		{
			PolyDefault tmpPoly = new PolyDefault();
			tmpPoly = tmpPoly.union(left) as PolyDefault;
			tmpPoly = tmpPoly.union(right) as PolyDefault;
			return( tmpPoly );
		}

		// -----------------------
		// --- Package Methods ---
		// -----------------------
		internal virtual void  print()
		{
			for (int i = 0; i < m_List.Count; i++)
			{
				IPolygon p = getInnerPoly(i);
				Console.Out.WriteLine("InnerPoly(" + i + ").hole=" + p.Hole);
				for (int j = 0; j < p.NumPoints; j++)
				{
					Console.Out.WriteLine(p.getX(j) + "  " + p.getY(j));
				}
			}
		}
	}
}
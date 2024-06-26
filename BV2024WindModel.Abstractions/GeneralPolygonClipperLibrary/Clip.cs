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

    // <summary> <code>Clip</code> is a C# (converted from Java) version of the <i>General Poly Clipper</i> algorithm
    // developed by Alan Murta (gpc@cs.man.ac.uk).  The home page for the original source can be 
    // found at <a href="http://www.cs.man.ac.uk/aig/staff/alan/software/" target="_blank">
    // http://www.cs.man.ac.uk/aig/staff/alan/software/</a>.
    // <p>
    // <b><code>polyClass:</code></b> Some of the public methods below take a <code>polyClass</code>
    // argument.  This <code>java.lang.Class</code> object is assumed to implement the <code>Poly</code>
    // interface and have a no argument constructor.  This was done so that the user of the algorithm
    // could create their own classes that implement the <code>Poly</code> interface and still uses
    // this algorithm.
    // <p>
    // <strong>Implementation Note:</strong> The converted algorithm does support the <i>difference</i>
    // operation, but a public method has not been provided and it has not been tested.  To do so,
    // simply follow what has been done for <i>intersection</i>.
    // *
    // </summary>
    // <author>   Dan Bridenbecker, Solution Engineering, Inc.
    // Rainer Hartmann, Seacos GmbH
    // Igor Protsenko, Seacos GmbH
    // </author>
    public class Clip
    {
        // -----------------
        // --- Constants ---
        // -----------------
        private static bool DEBUG = false;

        private const double GPC_EPSILON = 2.2204460492503131e-016;

        private const int LEFT = 0;
        private const int RIGHT = 1;

        private const int ABOVE = 0;
        private const int BELOW = 1;

        private const int CLIP = 0;
        private const int SUBJ = 1;

        // ------------------------
        // --- Member Variables ---
        // ------------------------

        // --------------------
        // --- Constructors ---
        // --------------------
        /// <summary>Creates a new instance of Clip 
        /// </summary>
        private Clip()
        {
        }

        // ----------------------
        // --- Static Methods ---
        // ----------------------
        // <summary> Return the intersection of <code>p1</code> and <code>p2</code> where the
        // return type is of <code>polyClass</code>.  See the note in the class description
        // for more on <ocde>polyClass</code>.
        // *
        // </summary>
        // <param name="p1">       One of the polygons to performt he intersection with
        // </param>
        // <param name="p2">       One of the polygons to performt he intersection with
        // </param>
        // <param name="polyClass">The type of <code>Poly</code> to return
        // 
        // </param>
        public static IPolygon intersection(IPolygon p1, IPolygon p2, Type polyClass)
        {
            return clip(OperationType.GPC_INT, p1, p2, polyClass);
        }

        // <summary> Return the difference of <code>p1</code> and <code>p2</code> where the
        // return type is of <code>polyClass</code>.  See the note in the class description
        // for more on <ocde>polyClass</code>.
        // *
        // </summary>
        // <param name="p1">       One of the polygons to performt he difference with
        // </param>
        // <param name="p2">       One of the polygons to performt he difference with
        // </param>
        // <param name="polyClass">The type of <code>Poly</code> to return
        // 
        // </param>
        public static IPolygon difference(IPolygon p1, IPolygon p2)
        {
            return clip(OperationType.GPC_DIFF, p1, p2, typeof(PolyDefault));
        }

        // <summary> Return the union of <code>p1</code> and <code>p2</code> where the
        // return type is of <code>polyClass</code>.  See the note in the class description
        // for more on <ocde>polyClass</code>.
        // *
        // </summary>
        // <param name="p1">       One of the polygons to performt he union with
        // </param>
        // <param name="p2">       One of the polygons to performt he union with
        // </param>
        // <param name="polyClass">The type of <code>Poly</code> to return
        // 
        // </param>
        public static IPolygon Union(IPolygon p1, IPolygon p2, Type polyClass)
        {
            return clip(OperationType.GPC_UNION, p1, p2, polyClass);
        }

        // <summary> Return the xor of <code>p1</code> and <code>p2</code> where the
        // return type is of <code>polyClass</code>.  See the note in the class description
        // for more on <ocde>polyClass</code>.
        // *
        // </summary>
        // <param name="p1">       One of the polygons to performt he xor with
        // </param>
        // <param name="p2">       One of the polygons to performt he xor with
        // </param>
        // <param name="polyClass">The type of <code>Poly</code> to return
        // 
        // </param>
        public static IPolygon Xor(IPolygon p1, IPolygon p2, Type polyClass)
        {
            return clip(OperationType.GPC_XOR, p1, p2, polyClass);
        }

        /// <summary> Return the difference of <code>p1</code> and <code>p2</code> where the
        /// return type is of <code>PolyDefault</code>. 
        /// *
        /// </summary>
        /// <param name="p1">One of the polygons to perform the difference with
        /// </param>
        /// <param name="p2">One of the polygons to perform the difference with
        /// 
        /// </param>
        /// <param name="polyClass"></param>
        public static IPolygon Diff(IPolygon p1, IPolygon p2, Type polyClass)
        {
            return clip(OperationType.GPC_DIFF, p1, p2, polyClass);
        }

        /// <summary> Return the intersection of <code>p1</code> and <code>p2</code> where the
        /// return type is of <code>PolyDefault</code>. 
        /// *
        /// </summary>
        /// <param name="p1">One of the polygons to performt he intersection with
        /// </param>
        /// <param name="p2">One of the polygons to performt he intersection with
        /// 
        /// </param>
        public static IPolygon Intersection(IPolygon p1, IPolygon p2)
        {
            return clip(OperationType.GPC_INT, p1, p2, typeof(PolyDefault));
        }

        /// <summary> Return the union of <code>p1</code> and <code>p2</code> where the
        /// return type is of <code>PolyDefault</code>. 
        /// *
        /// </summary>
        /// <param name="p1">One of the polygons to performt he union with
        /// </param>
        /// <param name="p2">One of the polygons to performt he union with
        /// 
        /// </param>
        public static IPolygon Union(IPolygon p1, IPolygon p2)
        {
            return clip(OperationType.GPC_UNION, p1, p2, typeof(PolyDefault));
        }

        /// <summary> Return the xor of <code>p1</code> and <code>p2</code> where the
        /// return type is of <code>PolyDefault</code>. 
        /// *
        /// </summary>
        /// <param name="p1">One of the polygons to perform he xor with
        /// </param>
        /// <param name="p2">One of the polygons to perform he xor with
        /// 
        /// </param>
        public static IPolygon Xor(IPolygon p1, IPolygon p2)
        {
            return clip(OperationType.GPC_XOR, p1, p2, typeof(PolyDefault));
        }

        /// <summary> Return the difference of <code>p1</code> and <code>p2</code> where the
        /// return type is of <code>PolyDefault</code>. 
        /// *
        /// </summary>
        /// <param name="p1">One of the polygons to perform the difference with
        /// </param>
        /// <param name="p2">One of the polygons to perform the difference with
        /// 
        /// </param>
        public static IPolygon Diff(IPolygon p1, IPolygon p2)
        {
            return clip(OperationType.GPC_DIFF, p1, p2, typeof(PolyDefault));
        }

        // -----------------------
        // --- Private Methods ---
        // -----------------------

        /// <summary> Create a new <code>Poly</code> type object using <code>polyClass</code>.
        /// </summary>
        private static IPolygon CreateNewPoly(Type polyClass)
        {
            try
            {
                return Activator.CreateInstance(polyClass, false) as IPolygon;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary> <code>clip()</code> is the main method of the clipper algorithm.
        /// This is where the conversion from really begins.
        /// </summary>
        private static IPolygon clip(OperationType op, IPolygon subj, IPolygon clip, Type polyClass)
        {
            IPolygon result = CreateNewPoly(polyClass);

            /* Test for trivial NULL result cases */
            if ((subj.Empty && clip.Empty) ||
                (subj.Empty && ((op == OperationType.GPC_INT) || (op == OperationType.GPC_DIFF))) ||
                (clip.Empty && (op == OperationType.GPC_INT)))
            {
                return result;
            }

            /* Identify potentialy contributing contours */
            if (((op == OperationType.GPC_INT) || (op == OperationType.GPC_DIFF)) && !subj.Empty && !clip.Empty)
            {
                minimax_test(subj, clip, op);
            }

            /* Build LMT */
            LmtTable lmt_table = new LmtTable();
            ScanBeamTreeEntries sbte = new ScanBeamTreeEntries();
            EdgeTable s_heap = null;
            EdgeTable c_heap = null;
            if (!subj.Empty)
            {
                s_heap = build_lmt(lmt_table, sbte, subj, SUBJ, op);
            }
            if (DEBUG)
            {
                Console.Out.WriteLine("");
                Console.Out.WriteLine(" ------------ After build_lmt for subj ---------");
                lmt_table.print();
            }
            if (!clip.Empty)
            {
                c_heap = build_lmt(lmt_table, sbte, clip, CLIP, op);
            }
            if (DEBUG)
            {
                Console.Out.WriteLine("");
                Console.Out.WriteLine(" ------------ After build_lmt for clip ---------");
                lmt_table.print();
            }

            /* Return a NULL result if no contours contribute */
            if (lmt_table.top_node == null)
            {
                return result;
            }

            /* Build scanbeam table from scanbeam tree */
            double[] sbt = sbte.build_sbt();

            int[] parity = new int[2];
            parity[0] = LEFT;
            parity[1] = LEFT;

            /* Invert clip polygon for difference operation */
            if (op == OperationType.GPC_DIFF)
            {
                parity[CLIP] = RIGHT;
            }

            if (DEBUG)
            {
                print_sbt(sbt);
            }

            LmtNode local_min = lmt_table.top_node;

            TopPolygonNode out_poly = new TopPolygonNode(); // used to create resulting Poly

            AetTree aet = new AetTree();
            int scanbeam = 0;

            /* Process each scanbeam */
            while (scanbeam < sbt.Length)
            {
                /* Set yb and yt to the bottom and top of the scanbeam */
                double yb = sbt[scanbeam++];
                double yt = 0.0;
                double dy = 0.0;
                if (scanbeam < sbt.Length)
                {
                    yt = sbt[scanbeam];
                    dy = yt - yb;
                }

                /* === SCANBEAM BOUNDARY PROCESSING ================================ */

                /* If LMT node corresponding to yb exists */
                if (local_min != null)
                {
                    if (local_min.y == yb)
                    {
                        /* Add edges starting at this local minimum to the AET */
                        for (EdgeNode edge = local_min.first_bound; (edge != null); edge = edge.next_bound)
                        {
                            add_edge_to_aet(aet, edge);
                        }

                        local_min = local_min.next;
                    }
                }

                if (DEBUG)
                {
                    aet.print();
                }
                /* Set dummy previous x value */
                //UPGRADE_TODO: The equivalent in .NET for field MAX_VALUE may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
                double px = -Double.MaxValue;

                /* Create bundles within AET */
                EdgeNode e0 = aet.top_node;
                EdgeNode e1 = aet.top_node;

                /* Set up bundle fields of first edge */
                aet.top_node.bundle[ABOVE][aet.top_node.type] = (aet.top_node.top.Y != yb) ? 1 : 0;
                aet.top_node.bundle[ABOVE][((aet.top_node.type == 0) ? 1 : 0)] = 0;
                aet.top_node.bstate[ABOVE] = BundleState.UNBUNDLED;

                for (EdgeNode next_edge = aet.top_node.next; (next_edge != null); next_edge = next_edge.next)
                {
                    int ne_type = next_edge.type;
                    int ne_type_opp = ((next_edge.type == 0) ? 1 : 0); //next edge type opposite

                    /* Set up bundle fields of next edge */
                    next_edge.bundle[ABOVE][ne_type] = (next_edge.top.Y != yb) ? 1 : 0;
                    next_edge.bundle[ABOVE][ne_type_opp] = 0;
                    next_edge.bstate[ABOVE] = BundleState.UNBUNDLED;

                    /* Bundle edges above the scanbeam boundary if they coincide */
                    if (next_edge.bundle[ABOVE][ne_type] == 1)
                    {
                        if (EQ(e0.xb, next_edge.xb) && EQ(e0.dx, next_edge.dx) && (e0.top.Y != yb))
                        {
                            next_edge.bundle[ABOVE][ne_type] ^= e0.bundle[ABOVE][ne_type];
                            next_edge.bundle[ABOVE][ne_type_opp] = e0.bundle[ABOVE][ne_type_opp];
                            next_edge.bstate[ABOVE] = BundleState.BUNDLE_HEAD;
                            e0.bundle[ABOVE][CLIP] = 0;
                            e0.bundle[ABOVE][SUBJ] = 0;
                            e0.bstate[ABOVE] = BundleState.BUNDLE_TAIL;
                        }
                        e0 = next_edge;
                    }
                }

                int[] horiz = new int[2];
                horiz[CLIP] = HState.NH;
                horiz[SUBJ] = HState.NH;

                int[] exists = new int[2];
                exists[CLIP] = 0;
                exists[SUBJ] = 0;

                PolygonNode cf = null;

                /* Process each edge at this scanbeam boundary */
                for (EdgeNode edge = aet.top_node; (edge != null); edge = edge.next)
                {
                    exists[CLIP] = edge.bundle[ABOVE][CLIP] + (edge.bundle[BELOW][CLIP] << 1);
                    exists[SUBJ] = edge.bundle[ABOVE][SUBJ] + (edge.bundle[BELOW][SUBJ] << 1);

                    if ((exists[CLIP] != 0) || (exists[SUBJ] != 0))
                    {
                        /* Set bundle side */
                        edge.bside[CLIP] = parity[CLIP];
                        edge.bside[SUBJ] = parity[SUBJ];

                        bool contributing = false;
                        int br = 0, bl = 0, tr = 0, tl = 0;
                        /* Determine contributing status and quadrant occupancies */
                        if ((op == OperationType.GPC_DIFF) || (op == OperationType.GPC_INT))
                        {
                            contributing = ((exists[CLIP] != 0) && ((parity[SUBJ] != 0) || (horiz[SUBJ] != 0))) ||
                                           ((exists[SUBJ] != 0) && ((parity[CLIP] != 0) || (horiz[CLIP] != 0))) ||
                                           ((exists[CLIP] != 0) && (exists[SUBJ] != 0) &&
                                            (parity[CLIP] == parity[SUBJ]));
                            br = ((parity[CLIP] != 0) && (parity[SUBJ] != 0)) ? 1 : 0;
                            bl = (((parity[CLIP] ^ edge.bundle[ABOVE][CLIP]) != 0) &&
                                  ((parity[SUBJ] ^ edge.bundle[ABOVE][SUBJ]) != 0))
                                ? 1
                                : 0;
                            tr = (((parity[CLIP] ^ ((horiz[CLIP] != HState.NH) ? 1 : 0)) != 0) &&
                                  ((parity[SUBJ] ^ ((horiz[SUBJ] != HState.NH) ? 1 : 0)) != 0))
                                ? 1
                                : 0;
                            tl =
                                (((parity[CLIP] ^ ((horiz[CLIP] != HState.NH) ? 1 : 0) ^ edge.bundle[BELOW][CLIP]) !=
                                  0) && ((parity[SUBJ] ^ ((horiz[SUBJ] != HState.NH) ? 1 : 0) ^
                                          edge.bundle[BELOW][SUBJ]) != 0))
                                    ? 1
                                    : 0;
                        }
                        else if (op == OperationType.GPC_XOR)
                        {
                            contributing = (exists[CLIP] != 0) || (exists[SUBJ] != 0);
                            br = (parity[CLIP]) ^ (parity[SUBJ]);
                            bl = (parity[CLIP] ^ edge.bundle[ABOVE][CLIP]) ^ (parity[SUBJ] ^ edge.bundle[ABOVE][SUBJ]);
                            tr = (parity[CLIP] ^ ((horiz[CLIP] != HState.NH) ? 1 : 0)) ^
                                 (parity[SUBJ] ^ ((horiz[SUBJ] != HState.NH) ? 1 : 0));
                            tl = (parity[CLIP] ^ ((horiz[CLIP] != HState.NH) ? 1 : 0) ^ edge.bundle[BELOW][CLIP]) ^
                                 (parity[SUBJ] ^ ((horiz[SUBJ] != HState.NH) ? 1 : 0) ^ edge.bundle[BELOW][SUBJ]);
                        }
                        else if (op == OperationType.GPC_UNION)
                        {
                            contributing = ((exists[CLIP] != 0) && (!(parity[SUBJ] != 0) || (horiz[SUBJ] != 0))) ||
                                           ((exists[SUBJ] != 0) && (!(parity[CLIP] != 0) || (horiz[CLIP] != 0))) ||
                                           ((exists[CLIP] != 0) && (exists[SUBJ] != 0) &&
                                            (parity[CLIP] == parity[SUBJ]));
                            br = ((parity[CLIP] != 0) || (parity[SUBJ] != 0)) ? 1 : 0;
                            bl = (((parity[CLIP] ^ edge.bundle[ABOVE][CLIP]) != 0) ||
                                  ((parity[SUBJ] ^ edge.bundle[ABOVE][SUBJ]) != 0))
                                ? 1
                                : 0;
                            tr = (((parity[CLIP] ^ ((horiz[CLIP] != HState.NH) ? 1 : 0)) != 0) ||
                                  ((parity[SUBJ] ^ ((horiz[SUBJ] != HState.NH) ? 1 : 0)) != 0))
                                ? 1
                                : 0;
                            tl =
                                (((parity[CLIP] ^ ((horiz[CLIP] != HState.NH) ? 1 : 0) ^ edge.bundle[BELOW][CLIP]) !=
                                  0) || ((parity[SUBJ] ^ ((horiz[SUBJ] != HState.NH) ? 1 : 0) ^
                                          edge.bundle[BELOW][SUBJ]) != 0))
                                    ? 1
                                    : 0;
                        }
                        else
                        {
                            throw new SystemException("Unknown op");
                        }

                        /* Update parity */
                        parity[CLIP] ^= edge.bundle[ABOVE][CLIP];
                        parity[SUBJ] ^= edge.bundle[ABOVE][SUBJ];

                        /* Update horizontal state */
                        if (exists[CLIP] != 0)
                        {
                            horiz[CLIP] = HState.next_h_state[horiz[CLIP]][((exists[CLIP] - 1) << 1) + parity[CLIP]];
                        }
                        if (exists[SUBJ] != 0)
                        {
                            horiz[SUBJ] = HState.next_h_state[horiz[SUBJ]][((exists[SUBJ] - 1) << 1) + parity[SUBJ]];
                        }

                        if (contributing)
                        {
                            double xb = edge.xb;

                            int vclass = VertexType.getType(tr, tl, br, bl);
                            switch (vclass)
                            {

                                case VertexType.EMN:
                                case VertexType.IMN:
                                    edge.outp[ABOVE] = out_poly.add_local_min(xb, yb);
                                    px = xb;
                                    cf = edge.outp[ABOVE];
                                    break;

                                case VertexType.ERI:
                                    if (xb != px)
                                    {
                                        cf.add_right(xb, yb);
                                        px = xb;
                                    }
                                    edge.outp[ABOVE] = cf;
                                    cf = null;
                                    break;

                                case VertexType.ELI:
                                    edge.outp[BELOW].add_left(xb, yb);
                                    px = xb;
                                    cf = edge.outp[BELOW];
                                    break;

                                case VertexType.EMX:
                                    if (xb != px)
                                    {
                                        cf.add_left(xb, yb);
                                        px = xb;
                                    }
                                    out_poly.merge_right(cf, edge.outp[BELOW]);
                                    cf = null;
                                    break;

                                case VertexType.ILI:
                                    if (xb != px)
                                    {
                                        cf.add_left(xb, yb);
                                        px = xb;
                                    }
                                    edge.outp[ABOVE] = cf;
                                    cf = null;
                                    break;

                                case VertexType.IRI:
                                    edge.outp[BELOW].add_right(xb, yb);
                                    px = xb;
                                    cf = edge.outp[BELOW];
                                    edge.outp[BELOW] = null;
                                    break;

                                case VertexType.IMX:
                                    if (xb != px)
                                    {
                                        cf.add_right(xb, yb);
                                        px = xb;
                                    }
                                    out_poly.merge_left(cf, edge.outp[BELOW]);
                                    cf = null;
                                    edge.outp[BELOW] = null;
                                    break;

                                case VertexType.IMM:
                                    if (xb != px)
                                    {
                                        cf.add_right(xb, yb);
                                        px = xb;
                                    }
                                    out_poly.merge_left(cf, edge.outp[BELOW]);
                                    edge.outp[BELOW] = null;
                                    edge.outp[ABOVE] = out_poly.add_local_min(xb, yb);
                                    cf = edge.outp[ABOVE];
                                    break;

                                case VertexType.EMM:
                                    if (xb != px)
                                    {
                                        cf.add_left(xb, yb);
                                        px = xb;
                                    }
                                    out_poly.merge_right(cf, edge.outp[BELOW]);
                                    edge.outp[BELOW] = null;
                                    edge.outp[ABOVE] = out_poly.add_local_min(xb, yb);
                                    cf = edge.outp[ABOVE];
                                    break;

                                case VertexType.LED:
                                    if (edge.bot.Y == yb)
                                        edge.outp[BELOW].add_left(xb, yb);
                                    edge.outp[ABOVE] = edge.outp[BELOW];
                                    px = xb;
                                    break;

                                case VertexType.RED:
                                    if (edge.bot.Y == yb)
                                        edge.outp[BELOW].add_right(xb, yb);
                                    edge.outp[ABOVE] = edge.outp[BELOW];
                                    px = xb;
                                    break;

                                default:
                                    break;

                            } /* End of switch */
                        }
                        /* End of contributing conditional */
                    }
                    /* End of edge exists conditional */
                    if (DEBUG)
                    {
                        out_poly.print();
                    }
                }
                /* End of AET loop */

                /* Delete terminating edges from the AET, otherwise compute xt */
                for (EdgeNode edge = aet.top_node; (edge != null); edge = edge.next)
                {
                    if (edge.top.Y == yb)
                    {
                        EdgeNode prev_edge = edge.prev;
                        EdgeNode next_edge = edge.next;

                        if (prev_edge != null)
                            prev_edge.next = next_edge;
                        else
                            aet.top_node = next_edge;

                        if (next_edge != null)
                            next_edge.prev = prev_edge;

                        /* Copy bundle head state to the adjacent tail edge if required */
                        if ((edge.bstate[BELOW] == BundleState.BUNDLE_HEAD) && (prev_edge != null))
                        {
                            if (prev_edge.bstate[BELOW] == BundleState.BUNDLE_TAIL)
                            {
                                prev_edge.outp[BELOW] = edge.outp[BELOW];
                                prev_edge.bstate[BELOW] = BundleState.UNBUNDLED;
                                if (prev_edge.prev != null)
                                {
                                    if (prev_edge.prev.bstate[BELOW] == BundleState.BUNDLE_TAIL)
                                    {
                                        prev_edge.bstate[BELOW] = BundleState.BUNDLE_HEAD;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (edge.top.Y == yt)
                            edge.xt = edge.top.X;
                        else
                            edge.xt = edge.bot.X + edge.dx * (yt - edge.bot.Y);
                    }
                }

                if (scanbeam < sbte.sbt_entries)
                {
                    /* === SCANBEAM INTERIOR PROCESSING ============================== */

                    /* Build intersection table for the current scanbeam */
                    ItNodeTable it_table = new ItNodeTable();
                    it_table.build_intersection_table(aet, dy);

                    /* Process each node in the intersection table */
                    for (ItNode intersect = it_table.top_node; (intersect != null); intersect = intersect.next)
                    {
                        e0 = intersect.ie[0];
                        e1 = intersect.ie[1];

                        /* Only generate output for contributing intersections */
                        if (((e0.bundle[ABOVE][CLIP] != 0) || (e0.bundle[ABOVE][SUBJ] != 0)) &&
                            ((e1.bundle[ABOVE][CLIP] != 0) || (e1.bundle[ABOVE][SUBJ] != 0)))
                        {
                            PolygonNode p = e0.outp[ABOVE];
                            PolygonNode q = e1.outp[ABOVE];
                            double ix = intersect.point.X;
                            double iy = intersect.point.Y + yb;

                            int in_clip = (((e0.bundle[ABOVE][CLIP] != 0) && !(e0.bside[CLIP] != 0)) ||
                                           ((e1.bundle[ABOVE][CLIP] != 0) && (e1.bside[CLIP] != 0)) ||
                                           (!(e0.bundle[ABOVE][CLIP] != 0) && !(e1.bundle[ABOVE][CLIP] != 0) &&
                                            (e0.bside[CLIP] != 0) && (e1.bside[CLIP] != 0)))
                                ? 1
                                : 0;

                            int in_subj = (((e0.bundle[ABOVE][SUBJ] != 0) && !(e0.bside[SUBJ] != 0)) ||
                                           ((e1.bundle[ABOVE][SUBJ] != 0) && (e1.bside[SUBJ] != 0)) ||
                                           (!(e0.bundle[ABOVE][SUBJ] != 0) && !(e1.bundle[ABOVE][SUBJ] != 0) &&
                                            (e0.bside[SUBJ] != 0) && (e1.bside[SUBJ] != 0)))
                                ? 1
                                : 0;

                            int tr = 0, tl = 0, br = 0, bl = 0;
                            /* Determine quadrant occupancies */
                            if ((op == OperationType.GPC_DIFF) || (op == OperationType.GPC_INT))
                            {
                                tr = ((in_clip != 0) && (in_subj != 0)) ? 1 : 0;
                                tl = (((in_clip ^ e1.bundle[ABOVE][CLIP]) != 0) &&
                                      ((in_subj ^ e1.bundle[ABOVE][SUBJ]) != 0))
                                    ? 1
                                    : 0;
                                br = (((in_clip ^ e0.bundle[ABOVE][CLIP]) != 0) &&
                                      ((in_subj ^ e0.bundle[ABOVE][SUBJ]) != 0))
                                    ? 1
                                    : 0;
                                bl = (((in_clip ^ e1.bundle[ABOVE][CLIP] ^ e0.bundle[ABOVE][CLIP]) != 0) &&
                                      ((in_subj ^ e1.bundle[ABOVE][SUBJ] ^ e0.bundle[ABOVE][SUBJ]) != 0))
                                    ? 1
                                    : 0;
                            }
                            else if (op == OperationType.GPC_XOR)
                            {
                                tr = (in_clip) ^ (in_subj);
                                tl = (in_clip ^ e1.bundle[ABOVE][CLIP]) ^ (in_subj ^ e1.bundle[ABOVE][SUBJ]);
                                br = (in_clip ^ e0.bundle[ABOVE][CLIP]) ^ (in_subj ^ e0.bundle[ABOVE][SUBJ]);
                                bl = (in_clip ^ e1.bundle[ABOVE][CLIP] ^ e0.bundle[ABOVE][CLIP]) ^
                                     (in_subj ^ e1.bundle[ABOVE][SUBJ] ^ e0.bundle[ABOVE][SUBJ]);
                            }
                            else if (op == OperationType.GPC_UNION)
                            {
                                tr = ((in_clip != 0) || (in_subj != 0)) ? 1 : 0;
                                tl = (((in_clip ^ e1.bundle[ABOVE][CLIP]) != 0) ||
                                      ((in_subj ^ e1.bundle[ABOVE][SUBJ]) != 0))
                                    ? 1
                                    : 0;
                                br = (((in_clip ^ e0.bundle[ABOVE][CLIP]) != 0) ||
                                      ((in_subj ^ e0.bundle[ABOVE][SUBJ]) != 0))
                                    ? 1
                                    : 0;
                                bl = (((in_clip ^ e1.bundle[ABOVE][CLIP] ^ e0.bundle[ABOVE][CLIP]) != 0) ||
                                      ((in_subj ^ e1.bundle[ABOVE][SUBJ] ^ e0.bundle[ABOVE][SUBJ]) != 0))
                                    ? 1
                                    : 0;
                            }
                            else
                            {
                                throw new SystemException("Unknown op type, " + op);
                            }

                            int vclass = VertexType.getType(tr, tl, br, bl);
                            switch (vclass)
                            {

                                case VertexType.EMN:
                                    e0.outp[ABOVE] = out_poly.add_local_min(ix, iy);
                                    e1.outp[ABOVE] = e0.outp[ABOVE];
                                    break;

                                case VertexType.ERI:
                                    if (p != null)
                                    {
                                        p.add_right(ix, iy);
                                        e1.outp[ABOVE] = p;
                                        e0.outp[ABOVE] = null;
                                    }
                                    break;

                                case VertexType.ELI:
                                    if (q != null)
                                    {
                                        q.add_left(ix, iy);
                                        e0.outp[ABOVE] = q;
                                        e1.outp[ABOVE] = null;
                                    }
                                    break;

                                case VertexType.EMX:
                                    if ((p != null) && (q != null))
                                    {
                                        p.add_left(ix, iy);
                                        out_poly.merge_right(p, q);
                                        e0.outp[ABOVE] = null;
                                        e1.outp[ABOVE] = null;
                                    }
                                    break;

                                case VertexType.IMN:
                                    e0.outp[ABOVE] = out_poly.add_local_min(ix, iy);
                                    e1.outp[ABOVE] = e0.outp[ABOVE];
                                    break;

                                case VertexType.ILI:
                                    if (p != null)
                                    {
                                        p.add_left(ix, iy);
                                        e1.outp[ABOVE] = p;
                                        e0.outp[ABOVE] = null;
                                    }
                                    break;

                                case VertexType.IRI:
                                    if (q != null)
                                    {
                                        q.add_right(ix, iy);
                                        e0.outp[ABOVE] = q;
                                        e1.outp[ABOVE] = null;
                                    }
                                    break;

                                case VertexType.IMX:
                                    if ((p != null) && (q != null))
                                    {
                                        p.add_right(ix, iy);
                                        out_poly.merge_left(p, q);
                                        e0.outp[ABOVE] = null;
                                        e1.outp[ABOVE] = null;
                                    }
                                    break;

                                case VertexType.IMM:
                                    if ((p != null) && (q != null))
                                    {
                                        p.add_right(ix, iy);
                                        out_poly.merge_left(p, q);
                                        e0.outp[ABOVE] = out_poly.add_local_min(ix, iy);
                                        e1.outp[ABOVE] = e0.outp[ABOVE];
                                    }
                                    break;

                                case VertexType.EMM:
                                    if ((p != null) && (q != null))
                                    {
                                        p.add_left(ix, iy);
                                        out_poly.merge_right(p, q);
                                        e0.outp[ABOVE] = out_poly.add_local_min(ix, iy);
                                        e1.outp[ABOVE] = e0.outp[ABOVE];
                                    }
                                    break;

                                default:
                                    break;

                            } /* End of switch */
                        }
                        /* End of contributing intersection conditional */

                        /* Swap bundle sides in response to edge crossing */
                        if (e0.bundle[ABOVE][CLIP] != 0)
                            e1.bside[CLIP] = (e1.bside[CLIP] == 0) ? 1 : 0;
                        if (e1.bundle[ABOVE][CLIP] != 0)
                            e0.bside[CLIP] = (e0.bside[CLIP] == 0) ? 1 : 0;
                        if (e0.bundle[ABOVE][SUBJ] != 0)
                            e1.bside[SUBJ] = (e1.bside[SUBJ] == 0) ? 1 : 0;
                        if (e1.bundle[ABOVE][SUBJ] != 0)
                            e0.bside[SUBJ] = (e0.bside[SUBJ] == 0) ? 1 : 0;

                        /* Swap e0 and e1 bundles in the AET */
                        EdgeNode prev_edge = e0.prev;
                        EdgeNode next_edge = e1.next;
                        if (next_edge != null)
                        {
                            next_edge.prev = e0;
                        }

                        if (e0.bstate[ABOVE] == BundleState.BUNDLE_HEAD)
                        {
                            bool search = true;
                            while (search)
                            {
                                prev_edge = prev_edge.prev;
                                if (prev_edge != null)
                                {
                                    if (prev_edge.bstate[ABOVE] != BundleState.BUNDLE_TAIL)
                                    {
                                        search = false;
                                    }
                                }
                                else
                                {
                                    search = false;
                                }
                            }
                        }
                        if (prev_edge == null)
                        {
                            aet.top_node.prev = e1;
                            e1.next = aet.top_node;
                            aet.top_node = e0.next;
                        }
                        else
                        {
                            prev_edge.next.prev = e1;
                            e1.next = prev_edge.next;
                            prev_edge.next = e0.next;
                        }
                        e0.next.prev = prev_edge;
                        e1.next.prev = e1;
                        e0.next = next_edge;
                        if (DEBUG)
                        {
                            out_poly.print();
                        }
                    }
                    /* End of IT loop*/

                    /* Prepare for next scanbeam */
                    for (EdgeNode edge = aet.top_node; (edge != null); edge = edge.next)
                    {
                        EdgeNode next_edge = edge.next;
                        EdgeNode succ_edge = edge.succ;
                        if ((edge.top.Y == yt) && (succ_edge != null))
                        {
                            /* Replace AET edge by its successor */
                            succ_edge.outp[BELOW] = edge.outp[ABOVE];
                            succ_edge.bstate[BELOW] = edge.bstate[ABOVE];
                            succ_edge.bundle[BELOW][CLIP] = edge.bundle[ABOVE][CLIP];
                            succ_edge.bundle[BELOW][SUBJ] = edge.bundle[ABOVE][SUBJ];
                            EdgeNode prev_edge = edge.prev;
                            if (prev_edge != null)
                                prev_edge.next = succ_edge;
                            else
                                aet.top_node = succ_edge;
                            if (next_edge != null)
                                next_edge.prev = succ_edge;
                            succ_edge.prev = prev_edge;
                            succ_edge.next = next_edge;
                        }
                        else
                        {
                            /* Update this edge */
                            edge.outp[BELOW] = edge.outp[ABOVE];
                            edge.bstate[BELOW] = edge.bstate[ABOVE];
                            edge.bundle[BELOW][CLIP] = edge.bundle[ABOVE][CLIP];
                            edge.bundle[BELOW][SUBJ] = edge.bundle[ABOVE][SUBJ];
                            edge.xb = edge.xt;
                        }
                        edge.outp[ABOVE] = null;
                    }
                }
            } /* === END OF SCANBEAM PROCESSING ================================== */

            /* Generate result polygon from out_poly */
            result = out_poly.getResult(polyClass);

            return result;
        }

        private static bool EQ(double a, double b)
        {
            return (Math.Abs(a - b) <= GPC_EPSILON);
        }

        private static int PREV_INDEX(int i, int n)
        {
            return ((i - 1 + n) % n);
        }

        private static int NEXT_INDEX(int i, int n)
        {
            return ((i + 1) % n);
        }

        private static bool OPTIMAL(IPolygon p, int i)
        {
            return (p.getY(PREV_INDEX(i, p.NumPoints)) != p.getY(i)) ||
                   (p.getY(NEXT_INDEX(i, p.NumPoints)) != p.getY(i));
        }

        private static RectangleF[] create_contour_bboxes(IPolygon p)
        {
            RectangleF[] box = new RectangleF[p.NumInnerPoly];

            /* Construct contour bounding boxes */
            for (int c = 0; c < p.NumInnerPoly; c++)
            {
                IPolygon inner_poly = p.getInnerPoly(c);
                box[c] = inner_poly.Bounds;
            }
            return box;
        }

        private static void minimax_test(IPolygon subj, IPolygon clip, OperationType op)
        {
            RectangleF[] s_bbox = create_contour_bboxes(subj);
            RectangleF[] c_bbox = create_contour_bboxes(clip);

            int subj_num_poly = subj.NumInnerPoly;
            int clip_num_poly = clip.NumInnerPoly;
            bool[][] o_table = new bool[subj_num_poly][];
            for (int i = 0; i < subj_num_poly; i++)
            {
                o_table[i] = new bool[clip_num_poly];
            }

            /* Check all subject contour bounding boxes against clip boxes */
            for (int s = 0; s < subj_num_poly; s++)
            {
                for (int c = 0; c < clip_num_poly; c++)
                {
                    //o_table[s][c] = (!((s_bbox[s].MaxX < c_bbox[c].MinX) || (s_bbox[s].MinX > c_bbox[c].MaxX))) && (!((s_bbox[s].MaxY < c_bbox[c].MinY) || (s_bbox[s].MinY > c_bbox[c].MaxY)));
                    o_table[s][c] = (!((s_bbox[s].Right < c_bbox[c].Left) || (s_bbox[s].Left > c_bbox[c].Right))) &&
                                    (!((s_bbox[s].Bottom < c_bbox[c].Top) || (s_bbox[s].Top > c_bbox[c].Bottom)));
                }
            }

            /* For each clip contour, search for any subject contour overlaps */
            for (int c = 0; c < clip_num_poly; c++)
            {
                bool overlap = false;
                for (int s = 0; !overlap && (s < subj_num_poly); s++)
                {
                    overlap = o_table[s][c];
                }
                if (!overlap)
                {
                    clip.setContributing(c, false); // Flag non contributing status
                }
            }

            if (op == OperationType.GPC_INT)
            {
                /* For each subject contour, search for any clip contour overlaps */
                for (int s = 0; s < subj_num_poly; s++)
                {
                    bool overlap = false;
                    for (int c = 0; !overlap && (c < clip_num_poly); c++)
                    {
                        overlap = o_table[s][c];
                    }
                    if (!overlap)
                    {
                        subj.setContributing(s, false); // Flag non contributing status
                    }
                }
            }
        }

        private static LmtNode bound_list(LmtTable lmt_table, double y)
        {
            if (lmt_table.top_node == null)
            {
                lmt_table.top_node = new LmtNode(y);
                return lmt_table.top_node;
            }
            else
            {
                LmtNode prev = null;
                LmtNode node = lmt_table.top_node;
                bool done = false;
                while (!done)
                {
                    if (y < node.y)
                    {
                        /* Insert a new LMT node before the current node */
                        LmtNode existing_node = node;
                        node = new LmtNode(y);
                        node.next = existing_node;
                        if (prev == null)
                        {
                            lmt_table.top_node = node;
                        }
                        else
                        {
                            prev.next = node;
                        }
                        //               if( existing_node == lmt_table.top_node )
                        //               {
                        //                  lmt_table.top_node = node ;
                        //               }
                        done = true;
                    }
                    else if (y > node.y)
                    {
                        /* Head further up the LMT */
                        if (node.next == null)
                        {
                            node.next = new LmtNode(y);
                            node = node.next;
                            done = true;
                        }
                        else
                        {
                            prev = node;
                            node = node.next;
                        }
                    }
                    else
                    {
                        /* Use this existing LMT node */
                        done = true;
                    }
                }
                return node;
            }
        }

        private static void insert_bound(LmtNode lmt_node, EdgeNode e)
        {
            if (lmt_node.first_bound == null)
            {
                /* Link node e to the tail of the list */
                lmt_node.first_bound = e;
            }
            else
            {
                bool done = false;
                EdgeNode prev_bound = null;
                EdgeNode current_bound = lmt_node.first_bound;
                while (!done)
                {
                    /* Do primary sort on the x field */
                    if (e.bot.X < current_bound.bot.X)
                    {
                        /* Insert a new node mid-list */
                        if (prev_bound == null)
                        {
                            lmt_node.first_bound = e;
                        }
                        else
                        {
                            prev_bound.next_bound = e;
                        }
                        e.next_bound = current_bound;

                        //               EdgeNode existing_bound = current_bound ;
                        //               current_bound = e ;
                        //               current_bound.next_bound = existing_bound ;
                        //               if( lmt_node.first_bound == existing_bound )
                        //               {
                        //                  lmt_node.first_bound = current_bound ;
                        //               }
                        done = true;
                    }
                    else if (e.bot.X == current_bound.bot.X)
                    {
                        /* Do secondary sort on the dx field */
                        if (e.dx < current_bound.dx)
                        {
                            /* Insert a new node mid-list */
                            if (prev_bound == null)
                            {
                                lmt_node.first_bound = e;
                            }
                            else
                            {
                                prev_bound.next_bound = e;
                            }
                            e.next_bound = current_bound;
                            //                  EdgeNode existing_bound = current_bound ;
                            //                  current_bound = e ;
                            //                  current_bound.next_bound = existing_bound ;
                            //                  if( lmt_node.first_bound == existing_bound )
                            //                  {
                            //                     lmt_node.first_bound = current_bound ;
                            //                  }
                            done = true;
                        }
                        else
                        {
                            /* Head further down the list */
                            if (current_bound.next_bound == null)
                            {
                                current_bound.next_bound = e;
                                done = true;
                            }
                            else
                            {
                                prev_bound = current_bound;
                                current_bound = current_bound.next_bound;
                            }
                        }
                    }
                    else
                    {
                        /* Head further down the list */
                        if (current_bound.next_bound == null)
                        {
                            current_bound.next_bound = e;
                            done = true;
                        }
                        else
                        {
                            prev_bound = current_bound;
                            current_bound = current_bound.next_bound;
                        }
                    }
                }
            }
        }

        private static void add_edge_to_aet(AetTree aet, EdgeNode edge)
        {
            if (aet.top_node == null)
            {
                /* Append edge onto the tail end of the AET */
                aet.top_node = edge;
                edge.prev = null;
                edge.next = null;
            }
            else
            {
                EdgeNode current_edge = aet.top_node;
                EdgeNode prev = null;
                bool done = false;
                while (!done)
                {
                    /* Do primary sort on the xb field */
                    if (edge.xb < current_edge.xb)
                    {
                        /* Insert edge here (before the AET edge) */
                        edge.prev = prev;
                        edge.next = current_edge;
                        current_edge.prev = edge;
                        if (prev == null)
                        {
                            aet.top_node = edge;
                        }
                        else
                        {
                            prev.next = edge;
                        }
                        //               if( current_edge == aet.top_node )
                        //               {
                        //                  aet.top_node = edge ;
                        //               }
                        //               current_edge = edge ;
                        done = true;
                    }
                    else if (edge.xb == current_edge.xb)
                    {
                        /* Do secondary sort on the dx field */
                        if (edge.dx < current_edge.dx)
                        {
                            /* Insert edge here (before the AET edge) */
                            edge.prev = prev;
                            edge.next = current_edge;
                            current_edge.prev = edge;
                            if (prev == null)
                            {
                                aet.top_node = edge;
                            }
                            else
                            {
                                prev.next = edge;
                            }
                            //                  if( current_edge == aet.top_node )
                            //                  {
                            //                     aet.top_node = edge ;
                            //                  }
                            //                  current_edge = edge ;
                            done = true;
                        }
                        else
                        {
                            /* Head further into the AET */
                            prev = current_edge;
                            if (current_edge.next == null)
                            {
                                current_edge.next = edge;
                                edge.prev = current_edge;
                                edge.next = null;
                                done = true;
                            }
                            else
                            {
                                current_edge = current_edge.next;
                            }
                        }
                    }
                    else
                    {
                        /* Head further into the AET */
                        prev = current_edge;
                        if (current_edge.next == null)
                        {
                            current_edge.next = edge;
                            edge.prev = current_edge;
                            edge.next = null;
                            done = true;
                        }
                        else
                        {
                            current_edge = current_edge.next;
                        }
                    }
                }
            }
        }

        private static void add_to_sbtree(ScanBeamTreeEntries sbte, double y)
        {
            if (sbte.sb_tree == null)
            {
                /* Add a new tree node here */
                sbte.sb_tree = new ScanBeamTree(y);
                sbte.sbt_entries++;
                return;
            }
            ScanBeamTree tree_node = sbte.sb_tree;
            bool done = false;
            while (!done)
            {
                if (tree_node.y > y)
                {
                    if (tree_node.less == null)
                    {
                        tree_node.less = new ScanBeamTree(y);
                        sbte.sbt_entries++;
                        done = true;
                    }
                    else
                    {
                        tree_node = tree_node.less;
                    }
                }
                else if (tree_node.y < y)
                {
                    if (tree_node.more == null)
                    {
                        tree_node.more = new ScanBeamTree(y);
                        sbte.sbt_entries++;
                        done = true;
                    }
                    else
                    {
                        tree_node = tree_node.more;
                    }
                }
                else
                {
                    done = true;
                }
            }
        }

        private static EdgeTable build_lmt(LmtTable lmt_table, ScanBeamTreeEntries sbte, IPolygon p, int type,
            OperationType op)
        {
            /* Create the entire input polygon edge table in one go */
            EdgeTable edge_table = new EdgeTable();

            for (int c = 0; c < p.NumInnerPoly; c++)
            {
                IPolygon ip = p.getInnerPoly(c);
                if (ip.Empty)
                    continue;
                if (!ip.isContributing(0))
                {
                    /* Ignore the non-contributing contour */
                    ip.setContributing(0, true);
                }
                else
                {
                    /* Perform contour optimisation */
                    int num_vertices = 0;
                    int e_index = 0;
                    edge_table = new EdgeTable();
                    for (int i = 0; i < ip.NumPoints; i++)
                    {
                        if (OPTIMAL(ip, i))
                        {
                            double x = ip.getX(i);
                            double y = ip.getY(i);
                            edge_table.addNode(x, y);

                            /* Record vertex in the scanbeam table */
                            add_to_sbtree(sbte, ip.getY(i));

                            num_vertices++;
                        }
                    }

                    /* Do the contour forward pass */
                    for (int min = 0; min < num_vertices; min++)
                    {
                        /* If a forward local minimum... */
                        if (edge_table.FWD_MIN(min))
                        {
                            /* Search for the next local maximum... */
                            int num_edges = 1;
                            int max = NEXT_INDEX(min, num_vertices);
                            while (edge_table.NOT_FMAX(max))
                            {
                                num_edges++;
                                max = NEXT_INDEX(max, num_vertices);
                            }

                            /* Build the next edge list */
                            int v = min;
                            EdgeNode e = edge_table.getNode(e_index);
                            e.bstate[BELOW] = BundleState.UNBUNDLED;
                            e.bundle[BELOW][CLIP] = 0;
                            e.bundle[BELOW][SUBJ] = 0;

                            for (int i = 0; i < num_edges; i++)
                            {
                                EdgeNode ei = edge_table.getNode(e_index + i);
                                EdgeNode ev = edge_table.getNode(v);

                                ei.xb = ev.vertex.X;
                                ei.bot.X = ev.vertex.X;
                                ei.bot.Y = ev.vertex.Y;

                                v = NEXT_INDEX(v, num_vertices);
                                ev = edge_table.getNode(v);

                                ei.top.X = ev.vertex.X;
                                ei.top.Y = ev.vertex.Y;
                                ei.dx = (ev.vertex.X - ei.bot.X) / (ei.top.Y - ei.bot.Y);
                                ei.type = type;
                                ei.outp[ABOVE] = null;
                                ei.outp[BELOW] = null;
                                ei.next = null;
                                ei.prev = null;
                                ei.succ = ((num_edges > 1) && (i < (num_edges - 1)))
                                    ? edge_table.getNode(e_index + i + 1)
                                    : null;
                                ei.pred = ((num_edges > 1) && (i > 0)) ? edge_table.getNode(e_index + i - 1) : null;
                                ei.next_bound = null;
                                ei.bside[CLIP] = (op == OperationType.GPC_DIFF) ? RIGHT : LEFT;
                                ei.bside[SUBJ] = LEFT;
                            }
                            insert_bound(bound_list(lmt_table, edge_table.getNode(min).vertex.Y), e);
                            if (DEBUG)
                            {
                                Console.Out.WriteLine("fwd");
                                lmt_table.print();
                            }
                            e_index += num_edges;
                        }
                    }

                    /* Do the contour reverse pass */
                    for (int min = 0; min < num_vertices; min++)
                    {
                        /* If a reverse local minimum... */
                        if (edge_table.REV_MIN(min))
                        {
                            /* Search for the previous local maximum... */
                            int num_edges = 1;
                            int max = PREV_INDEX(min, num_vertices);
                            while (edge_table.NOT_RMAX(max))
                            {
                                num_edges++;
                                max = PREV_INDEX(max, num_vertices);
                            }

                            /* Build the previous edge list */
                            int v = min;
                            EdgeNode e = edge_table.getNode(e_index);
                            e.bstate[BELOW] = BundleState.UNBUNDLED;
                            e.bundle[BELOW][CLIP] = 0;
                            e.bundle[BELOW][SUBJ] = 0;

                            for (int i = 0; i < num_edges; i++)
                            {
                                EdgeNode ei = edge_table.getNode(e_index + i);
                                EdgeNode ev = edge_table.getNode(v);

                                ei.xb = ev.vertex.X;
                                ei.bot.X = ev.vertex.X;
                                ei.bot.Y = ev.vertex.Y;

                                v = PREV_INDEX(v, num_vertices);
                                ev = edge_table.getNode(v);

                                ei.top.X = ev.vertex.X;
                                ei.top.Y = ev.vertex.Y;
                                ei.dx = (ev.vertex.X - ei.bot.X) / (ei.top.Y - ei.bot.Y);
                                ei.type = type;
                                ei.outp[ABOVE] = null;
                                ei.outp[BELOW] = null;
                                ei.next = null;
                                ei.prev = null;
                                ei.succ = ((num_edges > 1) && (i < (num_edges - 1)))
                                    ? edge_table.getNode(e_index + i + 1)
                                    : null;
                                ei.pred = ((num_edges > 1) && (i > 0)) ? edge_table.getNode(e_index + i - 1) : null;
                                ei.next_bound = null;
                                ei.bside[CLIP] = (op == OperationType.GPC_DIFF) ? RIGHT : LEFT;
                                ei.bside[SUBJ] = LEFT;
                            }
                            insert_bound(bound_list(lmt_table, edge_table.getNode(min).vertex.Y), e);
                            if (DEBUG)
                            {
                                Console.Out.WriteLine("rev");
                                lmt_table.print();
                            }
                            e_index += num_edges;
                        }
                    }
                }
            }
            return edge_table;
        }

        private static StNode add_st_edge(StNode st, ItNodeTable it, EdgeNode edge, double dy)
        {
            if (st == null)
            {
                /* Append edge onto the tail end of the ST */
                st = new StNode(edge, null);
            }
            else
            {
                double den = (st.xt - st.xb) - (edge.xt - edge.xb);

                /* If new edge and ST edge don't cross */
                if ((edge.xt >= st.xt) || (edge.dx == st.dx) || (Math.Abs(den) <= GPC_EPSILON))
                {
                    /* No intersection - insert edge here (before the ST edge) */
                    StNode existing_node = st;
                    st = new StNode(edge, existing_node);
                }
                else
                {
                    /* Compute intersection between new edge and ST edge */
                    double r = (edge.xb - st.xb) / den;
                    double x = st.xb + r * (st.xt - st.xb);
                    double y = r * dy;

                    /* Insert the edge pointers and the intersection point in the IT */
                    it.top_node = add_intersection(it.top_node, st.edge, edge, x, y);

                    /* Head further into the ST */
                    st.prev = add_st_edge(st.prev, it, edge, dy);
                }
            }
            return st;
        }

        private static ItNode add_intersection(ItNode it_node, EdgeNode edge0, EdgeNode edge1, double x, double y)
        {
            if (it_node == null)
            {
                /* Append a new node to the tail of the list */
                it_node = new ItNode(edge0, edge1, x, y, null);
            }
            else
            {
                if (it_node.point.Y > y)
                {
                    /* Insert a new node mid-list */
                    ItNode existing_node = it_node;
                    it_node = new ItNode(edge0, edge1, x, y, existing_node);
                }
                else
                {
                    /* Head further down the list */
                    it_node.next = add_intersection(it_node.next, edge0, edge1, x, y);
                }
            }
            return it_node;
        }

        // ---------------------
        // --- Inner Classes ---
        // ---------------------
        private class OperationType
        {
            private String m_Type;

            private OperationType(String type)
            {
                m_Type = type;
            }

            //UPGRADE_NOTE: Final was removed from the declaration of 'GPC_DIFF '. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1003"'
            public static readonly OperationType GPC_DIFF = new OperationType("Difference");

            //UPGRADE_NOTE: Final was removed from the declaration of 'GPC_INT '. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1003"'
            public static readonly OperationType GPC_INT = new OperationType("Intersection");

            //UPGRADE_NOTE: Final was removed from the declaration of 'GPC_XOR '. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1003"'
            public static readonly OperationType GPC_XOR = new OperationType("Exclusive or");

            //UPGRADE_NOTE: Final was removed from the declaration of 'GPC_UNION '. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1003"'
            public static readonly OperationType GPC_UNION = new OperationType("Union");

            public override String ToString()
            {
                return m_Type;
            }
        }

        /// <summary> Edge intersection classes
        /// </summary>
        private class VertexType
        {
            public const int NUL = 0; /* Empty non-intersection            */
            public const int EMX = 1; /* External maximum                  */
            public const int ELI = 2; /* External left intermediate        */
            public const int TED = 3; /* Top edge                          */
            public const int ERI = 4; /* External right intermediate       */
            public const int RED = 5; /* Right edge                        */
            public const int IMM = 6; /* Internal maximum and minimum      */
            public const int IMN = 7; /* Internal minimum                  */
            public const int EMN = 8; /* External minimum                  */
            public const int EMM = 9; /* External maximum and minimum      */
            public const int LED = 10; /* Left edge                         */
            public const int ILI = 11; /* Internal left intermediate        */
            public const int BED = 12; /* Bottom edge                       */
            public const int IRI = 13; /* Internal right intermediate       */
            public const int IMX = 14; /* Internal maximum                  */

            public const int FUL = 15;
            /* Full non-intersection             */

            public static int getType(int tr, int tl, int br, int bl)
            {
                return tr + (tl << 1) + (br << 2) + (bl << 3);
            }
        }

        /// <summary> Horizontal edge states            
        /// </summary>
        private class HState
        {
            public const int NH = 0; /* No horizontal edge                */
            public const int BH = 1; /* Bottom horizontal edge            */
            public const int TH = 2; /* Top horizontal edge               */

            /* Horizontal edge state transitions within scanbeam boundary */
            //UPGRADE_NOTE: Final was removed from the declaration of 'next_h_state '. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1003"'
            public static readonly int[][] next_h_state =
            {
                new int[] {BH, TH, TH, BH, NH, NH}, new int[] {NH, NH, NH, NH, TH, TH},
                new int[] {NH, NH, NH, NH, BH, BH}
            };
        }

        /// <summary> Edge bundle state                 
        /// </summary>
        private class BundleState
        {
            private String m_State;

            private BundleState(String state)
            {
                m_State = state;
            }

            //UPGRADE_NOTE: Final was removed from the declaration of 'UNBUNDLED '. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1003"'
            public static readonly BundleState UNBUNDLED = new BundleState("UNBUNDLED");

            // Isolated edge not within a bundle
            //UPGRADE_NOTE: Final was removed from the declaration of 'BUNDLE_HEAD '. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1003"'
            public static readonly BundleState BUNDLE_HEAD = new BundleState("BUNDLE_HEAD");

            // Bundle head node
            //UPGRADE_NOTE: Final was removed from the declaration of 'BUNDLE_TAIL '. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1003"'
            public static readonly BundleState BUNDLE_TAIL = new BundleState("BUNDLE_TAIL");
            // Passive bundle tail node

            public override String ToString()
            {
                return m_State;
            }
        }

        /// <summary> Internal vertex list datatype
        /// </summary>
        private class VertexNode
        {
            internal double x; // X coordinate component
            internal double y; // Y coordinate component

            internal VertexNode next;
            // Pointer to next vertex in list

            public VertexNode(double x, double y)
            {
                this.x = x;
                this.y = y;
                this.next = null;
            }
        }

        /// <summary> Internal contour / tristrip type
        /// </summary>
        private class PolygonNode
        {
            private void InitBlock()
            {
                v = new VertexNode[2];
            }

            internal int active; /* Active flag / vertex count        */

            internal bool hole; /* Hole / external contour flag      */

            //UPGRADE_NOTE: The initialization of  'v' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
            internal VertexNode[] v;

            /* Left and right vertex list ptrs   */
            internal PolygonNode next; /* Pointer to next polygon contour   */

            internal PolygonNode proxy;
            /* Pointer to actual structure used  */

            public PolygonNode()
            {
                InitBlock();
            }

            public PolygonNode(PolygonNode next, double x, double y) : this()
            {
                /* Make v[LEFT] and v[RIGHT] point to new vertex */
                VertexNode vn = new VertexNode(x, y);
                this.v[LEFT] = vn;
                this.v[RIGHT] = vn;

                this.next = next;
                this.proxy = this; /* Initialise proxy to point to p itself */
                this.active = 1; //TRUE
            }

            public virtual void add_right(double x, double y)
            {
                VertexNode nv = new VertexNode(x, y);

                /* Add vertex nv to the right end of the polygon's vertex list */
                proxy.v[RIGHT].next = nv;

                /* Update proxy->v[RIGHT] to point to nv */
                proxy.v[RIGHT] = nv;
            }

            public virtual void add_left(double x, double y)
            {
                VertexNode nv = new VertexNode(x, y);

                /* Add vertex nv to the left end of the polygon's vertex list */
                nv.next = proxy.v[LEFT];

                /* Update proxy->[LEFT] to point to nv */
                proxy.v[LEFT] = nv;
            }
        }

        private class TopPolygonNode
        {
            internal PolygonNode top_node = null;

            public virtual PolygonNode add_local_min(double x, double y)
            {
                PolygonNode existing_min = top_node;

                top_node = new PolygonNode(existing_min, x, y);

                return top_node;
            }

            public virtual void merge_left(PolygonNode p, PolygonNode q)
            {
                /* Label contour as a hole */
                q.proxy.hole = true;

                if (p.proxy != q.proxy)
                {
                    /* Assign p's vertex list to the left end of q's list */
                    p.proxy.v[RIGHT].next = q.proxy.v[LEFT];
                    q.proxy.v[LEFT] = p.proxy.v[LEFT];

                    /* Redirect any p.proxy references to q.proxy */
                    PolygonNode target = p.proxy;
                    for (PolygonNode node = top_node; (node != null); node = node.next)
                    {
                        if (node.proxy == target)
                        {
                            node.active = 0;
                            node.proxy = q.proxy;
                        }
                    }
                }
            }

            public virtual void merge_right(PolygonNode p, PolygonNode q)
            {
                /* Label contour as external */
                q.proxy.hole = false;

                if (p.proxy != q.proxy)
                {
                    /* Assign p's vertex list to the right end of q's list */
                    q.proxy.v[RIGHT].next = p.proxy.v[LEFT];
                    q.proxy.v[RIGHT] = p.proxy.v[RIGHT];

                    /* Redirect any p->proxy references to q->proxy */
                    PolygonNode target = p.proxy;
                    for (PolygonNode node = top_node; (node != null); node = node.next)
                    {
                        if (node.proxy == target)
                        {
                            node.active = 0;
                            node.proxy = q.proxy;
                        }
                    }
                }
            }

            public virtual int count_contours()
            {
                int nc = 0;
                for (PolygonNode polygon = top_node; (polygon != null); polygon = polygon.next)
                {
                    if (polygon.active != 0)
                    {
                        /* Count the vertices in the current contour */
                        int nv = 0;
                        for (VertexNode v = polygon.proxy.v[LEFT]; (v != null); v = v.next)
                        {
                            nv++;
                        }

                        /* Record valid vertex counts in the active field */
                        if (nv > 2)
                        {
                            polygon.active = nv;
                            nc++;
                        }
                        else
                        {
                            /* Invalid contour: just free the heap */
                            //                  VertexNode nextv = null ;
                            //                  for (VertexNode v= polygon.proxy.v[LEFT]; (v != null); v = nextv)
                            //                  {
                            //                     nextv= v.next;
                            //                     v = null ;
                            //                  }
                            polygon.active = 0;
                        }
                    }
                }
                return nc;
            }

            public virtual IPolygon getResult(Type polyClass)
            {
                IPolygon result = CreateNewPoly(polyClass);
                int num_contours = count_contours();
                if (num_contours > 0)
                {
                    int c = 0;
                    PolygonNode npoly_node = null;
                    for (PolygonNode poly_node = top_node; (poly_node != null); poly_node = npoly_node)
                    {
                        npoly_node = poly_node.next;
                        if (poly_node.active != 0)
                        {
                            IPolygon poly = result;
                            if (num_contours > 1)
                            {
                                poly = CreateNewPoly(polyClass);
                            }
                            if (poly_node.proxy.hole)
                            {
                                poly.IsHole = poly_node.proxy.hole;
                            }

                            // ------------------------------------------------------------------------
                            // --- This algorithm puts the verticies into the poly in reverse order ---
                            // ------------------------------------------------------------------------
                            for (VertexNode vtx = poly_node.proxy.v[LEFT]; (vtx != null); vtx = vtx.next)
                            {
                                poly.add(vtx.x, vtx.y);
                            }
                            if (num_contours > 1)
                            {
                                result.add(poly);
                            }
                            c++;
                        }
                    }

                    // -----------------------------------------
                    // --- Sort holes to the end of the list ---
                    // -----------------------------------------
                    IPolygon orig = result;
                    result = CreateNewPoly(polyClass);
                    for (int i = 0; i < orig.NumInnerPoly; i++)
                    {
                        IPolygon inner = orig.getInnerPoly(i);
                        if (!inner.Hole)
                        {
                            result.add(inner);
                        }
                    }
                    for (int i = 0; i < orig.NumInnerPoly; i++)
                    {
                        IPolygon inner = orig.getInnerPoly(i);
                        if (inner.Hole)
                        {
                            result.add(inner);
                        }
                    }
                }
                return result;
            }

            public virtual void print()
            {
                Console.Out.WriteLine("---- out_poly ----");
                int c = 0;
                PolygonNode npoly_node = null;
                for (PolygonNode poly_node = top_node; (poly_node != null); poly_node = npoly_node)
                {
                    Console.Out.WriteLine("contour=" + c + "  active=" + poly_node.active + "  hole=" +
                                          poly_node.proxy.hole);
                    npoly_node = poly_node.next;
                    if (poly_node.active != 0)
                    {
                        int v = 0;
                        for (VertexNode vtx = poly_node.proxy.v[LEFT]; (vtx != null); vtx = vtx.next)
                        {
                            Console.Out.WriteLine("v=" + v + "  vtx.x=" + vtx.x + "  vtx.y=" + vtx.y);
                        }
                        c++;
                    }
                }
            }
        }

        private class EdgeNode
        {
            public EdgeNode()
            {
                InitBlock();
            }

            private void InitBlock()
            {
                outp = new PolygonNode[2];
                bstate = new BundleState[2];
                bside = new int[2];
                bundle = new int[2][];
                vertex = new PointF();
                bot = new PointF();
                top = new PointF();
                for (int i = 0; i < 2; i++)
                {
                    bundle[i] = new int[2];
                }
            }

            //UPGRADE_NOTE: The initialization of  'vertex' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
            internal PointF vertex;

            /* Piggy-backed contour vertex data  */
            //UPGRADE_NOTE: The initialization of  'bot' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
            internal PointF bot;

            /* Edge lower (x, y) coordinate      */
            //UPGRADE_NOTE: The initialization of  'top' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
            internal PointF top;

            /* Edge upper (x, y) coordinate      */
            internal double xb; /* Scanbeam bottom x coordinate      */
            internal double xt; /* Scanbeam top x coordinate         */
            internal double dx; /* Change in x for a unit y increase */

            internal int type; /* Clip / subject edge flag          */

            //UPGRADE_NOTE: The initialization of  'bundle' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
            internal int[][] bundle;

            /* Bundle edge flags                 */
            //UPGRADE_NOTE: The initialization of  'bside' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
            internal int[] bside;

            /* Bundle left / right indicators    */
            //UPGRADE_NOTE: The initialization of  'bstate' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
            internal BundleState[] bstate;

            /* Edge bundle state                 */
            //UPGRADE_NOTE: The initialization of  'outp' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
            internal PolygonNode[] outp;

            /* Output polygon / tristrip pointer */
            internal EdgeNode prev; /* Previous edge in the AET          */
            internal EdgeNode next; /* Next edge in the AET              */
            internal EdgeNode pred; /* Edge connected at the lower end   */
            internal EdgeNode succ; /* Edge connected at the upper end   */
            internal EdgeNode next_bound; /* Pointer to next bound in LMT      */
        }

        private class AetTree
        {
            internal EdgeNode top_node;

            public virtual void print()
            {
                Console.Out.WriteLine("");
                Console.Out.WriteLine("aet");
                for (EdgeNode edge = top_node; (edge != null); edge = edge.next)
                {
                    Console.Out.WriteLine("edge.vertex.x=" + edge.vertex.X + "  edge.vertex.y=" + edge.vertex.Y);
                }
            }
        }

        private class EdgeTable
        {
            public EdgeTable()
            {
                InitBlock();
            }

            private void InitBlock()
            {
                m_List = new ArrayList();
            }

            //UPGRADE_NOTE: The initialization of  'm_List' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
            private ArrayList m_List;

            public virtual void addNode(double x, double y)
            {
                EdgeNode node = new EdgeNode();
                node.vertex.X = (float) x;
                node.vertex.Y = (float) y;
                m_List.Add(node);
            }

            public virtual EdgeNode getNode(int index)
            {
                return (EdgeNode) m_List[index];
            }

            public virtual bool FWD_MIN(int i)
            {
                EdgeNode prev = (EdgeNode) m_List[PREV_INDEX(i, m_List.Count)];
                EdgeNode next = (EdgeNode) m_List[NEXT_INDEX(i, m_List.Count)];
                EdgeNode ith = (EdgeNode) m_List[i];
                return ((prev.vertex.Y >= ith.vertex.Y) && (next.vertex.Y > ith.vertex.Y));
            }

            public virtual bool NOT_FMAX(int i)
            {
                EdgeNode next = (EdgeNode) m_List[NEXT_INDEX(i, m_List.Count)];
                EdgeNode ith = (EdgeNode) m_List[i];
                return (next.vertex.Y > ith.vertex.Y);
            }

            public virtual bool REV_MIN(int i)
            {
                EdgeNode prev = (EdgeNode) m_List[PREV_INDEX(i, m_List.Count)];
                EdgeNode next = (EdgeNode) m_List[NEXT_INDEX(i, m_List.Count)];
                EdgeNode ith = (EdgeNode) m_List[i];
                return ((prev.vertex.Y > ith.vertex.Y) && (next.vertex.Y >= ith.vertex.Y));
            }

            public virtual bool NOT_RMAX(int i)
            {
                EdgeNode prev = (EdgeNode) m_List[PREV_INDEX(i, m_List.Count)];
                EdgeNode ith = (EdgeNode) m_List[i];
                return (prev.vertex.Y > ith.vertex.Y);
            }
        }

        /// <summary> Local minima table
        /// </summary>
        private class LmtNode
        {
            internal double y; /* Y coordinate at local minimum     */
            internal EdgeNode first_bound; /* Pointer to bound list             */

            internal LmtNode next;
            /* Pointer to next local minimum     */

            public LmtNode(double yvalue)
            {
                y = yvalue;
            }
        }

        private class LmtTable
        {
            internal LmtNode top_node;

            public virtual void print()
            {
                int n = 0;
                LmtNode lmt = top_node;
                while (lmt != null)
                {
                    Console.Out.WriteLine("lmt(" + n + ")");
                    for (EdgeNode edge = lmt.first_bound; (edge != null); edge = edge.next_bound)
                    {
                        Console.Out.WriteLine("edge.vertex.x=" + edge.vertex.X + "  edge.vertex.y=" + edge.vertex.Y);
                    }
                    n++;
                    lmt = lmt.next;
                }
            }
        }

        /// <summary> Scanbeam tree 
        /// </summary>
        private class ScanBeamTree
        {
            internal double y; /* Scanbeam node y value             */
            internal ScanBeamTree less; /* Pointer to nodes with lower y     */

            internal ScanBeamTree more;
            /* Pointer to nodes with higher y    */

            public ScanBeamTree(double yvalue)
            {
                y = yvalue;
            }
        }

        /// <summary>*
        /// </summary>
        private class ScanBeamTreeEntries
        {
            internal int sbt_entries;
            internal ScanBeamTree sb_tree;

            public virtual double[] build_sbt()
            {
                double[] sbt = new double[sbt_entries];

                int entries = 0;
                entries = inner_build_sbt(entries, sbt, sb_tree);
                if (entries != sbt_entries)
                {
                    throw new SystemException("Something went wrong buildign sbt from tree.");
                }
                return sbt;
            }

            private int inner_build_sbt(int entries, double[] sbt, ScanBeamTree sbt_node)
            {
                if (sbt_node.less != null)
                {
                    entries = inner_build_sbt(entries, sbt, sbt_node.less);
                }
                sbt[entries] = sbt_node.y;
                entries++;
                if (sbt_node.more != null)
                {
                    entries = inner_build_sbt(entries, sbt, sbt_node.more);
                }
                return entries;
            }
        }

        /// <summary> Intersection table
        /// </summary>
        private class ItNode
        {
            private void InitBlock()
            {
                ie = new EdgeNode[2];
                point = new PointF();
            }

            //UPGRADE_NOTE: The initialization of  'ie' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
            internal EdgeNode[] ie;

            /* Intersecting edge (bundle) pair   */
            //UPGRADE_NOTE: The initialization of  'point' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
            internal PointF point;

            /* Point of intersection             */
            internal ItNode next;
            /* The next intersection table node  */

            public ItNode(EdgeNode edge0, EdgeNode edge1, double x, double y, ItNode next)
            {
                InitBlock();
                this.ie[0] = edge0;
                this.ie[1] = edge1;
                this.point.X = (float) x;
                this.point.Y = (float) y;
                this.next = next;
            }
        }

        private class ItNodeTable
        {
            internal ItNode top_node;

            public virtual void build_intersection_table(AetTree aet, double dy)
            {
                StNode st = null;

                /* Process each AET edge */
                for (EdgeNode edge = aet.top_node; (edge != null); edge = edge.next)
                {
                    if ((edge.bstate[ABOVE] == BundleState.BUNDLE_HEAD) || (edge.bundle[ABOVE][CLIP] != 0) ||
                        (edge.bundle[ABOVE][SUBJ] != 0))
                    {
                        st = add_st_edge(st, this, edge, dy);
                    }
                }
            }
        }

        /// <summary> Sorted edge table
        /// </summary>
        private class StNode
        {
            internal EdgeNode edge; /* Pointer to AET edge               */
            internal double xb; /* Scanbeam bottom x coordinate      */
            internal double xt; /* Scanbeam top x coordinate         */
            internal double dx; /* Change in x for a unit y increase */

            internal StNode prev;
            /* Previous edge in sorted list      */

            public StNode(EdgeNode edge, StNode prev)
            {
                this.edge = edge;
                this.xb = edge.xb;
                this.xt = edge.xt;
                this.dx = edge.dx;
                this.prev = prev;
            }
        }

        // -------------
        // --- DEBUG ---
        // -------------
        private static void print_sbt(double[] sbt)
        {
            Console.Out.WriteLine("");
            Console.Out.WriteLine("sbt.length=" + sbt.Length);
            for (int i = 0; i < sbt.Length; i++)
            {
                Console.Out.WriteLine("sbt[" + i + "]=" + sbt[i]);
            }
        }
    }
}
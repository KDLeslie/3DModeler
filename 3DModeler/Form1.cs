using System;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using static _3DModeler.Operations;

namespace _3DModeler
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Stopwatch Stopwatch = new Stopwatch(); // Stores the total time from start
        Viewport MainView = new Viewport(); // The interface that displays the 3D graphics
        int FrameCount = 0; // Stores how many frames are rendered each second
        int FPS = 0; // Stores the current frame rate of the viewport
        float FrameTime = 0; // Stores the cumulative time between frames         
        float Tick = 0; // A time variable showing the point in time before rendering the current frame
        float Tock = 0; // A time variable showing the point in time before rendering the last frame
        float ElapsedTime = 0;  // Stores the time between each frame in seconds
        List<Mesh> Meshes = new List<Mesh>(); // Stores each mesh loaded from an obj file
        PointF LastCursorPosition;
        bool MousePressed = false;
        // Stores what keys the user is currently pressing
        Dictionary<Keys, bool> KeyPressed = new Dictionary<Keys, bool>()
        {
            { Keys.O, false },
            { Keys.L, false },
            { Keys.K, false },
            { Keys.OemSemicolon, false },
            { Keys.W, false },
            { Keys.S, false },
            { Keys.D, false },
            { Keys.A, false },
            { Keys.R, false },
            { Keys.F, false },
        };

        struct Triangle
        {
            public Triangle() { }
            public Triangle(ref Triangle tri)
            {
                for (int i = 0; i < 3; i++)
                {
                    this.p[i] = tri.p[i];
                    this.t[i] = tri.t[i];
                }
                this.lum = tri.lum;
                this.normal = tri.normal;
                this.drawSide0_1 = tri.drawSide0_1;
                this.drawSide1_2 = tri.drawSide1_2;
                this.drawSide2_0 = tri.drawSide2_0;
            }
            public Triangle(Vec3D p1, Vec3D p2, Vec3D p3)
            {
                this.p[0] = p1;
                this.p[1] = p2;
                this.p[2] = p3;
            }
            public Triangle(Vec3D p1, Vec3D p2, Vec3D p3, Vec2D t1, Vec2D t2, Vec2D t3)
            {
                this.p[0] = p1;
                this.p[1] = p2;
                this.p[2] = p3;
                this.t[0] = t1;
                this.t[1] = t2;
                this.t[2] = t3;
            }

            public Vec3D[] p = new Vec3D[3]; // Stores vertex coordinates for the triangle
            public Vec2D[] t = new Vec2D[3]; // Stores texel coordinates for each vertex
            public float lum = 0; // Calculated luminosity for the triangle based on light source
            public Vec3D normal = new Vec3D(); // Normal vector of the triangle
            // A small hack used to draw the wireframe of a quadrilateral
            // correctly. Eventually will also be used to fix the wireframe
            // of clipped triangles
            public bool drawSide0_1 = true;
            public bool drawSide1_2 = true;
            public bool drawSide2_0 = true;
        }

        struct Quadrilateral
        {
            public Quadrilateral() { }
            public Quadrilateral(ref Quadrilateral quad)
            {
                for (int i = 0; i < 4; i++)
                {
                    this.p[i] = quad.p[i];
                    this.t[i] = quad.t[i];
                }
                this.lum = quad.lum;
                this.normal = quad.normal;
            }
            // A quadrilateral value type used to present triangles in 3D objects
            public Quadrilateral(Vec3D p1, Vec3D p2, Vec3D p3, Vec3D p4)
            {
                this.p[0] = p1;
                this.p[1] = p2;
                this.p[2] = p3;
                this.p[3] = p4;
            }
            public Quadrilateral(Vec3D p1, Vec3D p2, Vec3D p3, Vec3D p4, Vec2D t1, Vec2D t2, Vec2D t3, Vec2D t4)
            {
                this.p[0] = p1;
                this.p[1] = p2;
                this.p[2] = p3;
                this.p[3] = p4;
                this.t[0] = t1;
                this.t[1] = t2;
                this.t[2] = t3;
                this.t[3] = t4;
            }

            public Vec3D[] p = new Vec3D[4]; // Stores vertex coordinates for the quadrilateral
            public Vec2D[] t = new Vec2D[4]; // Stores texel coordinates for each vertex
            public float lum = 0; // Calculated luminosity for the quadrilateral based on the light source
            public Vec3D normal = new Vec3D(); // Normal vector for the quadrilateral
        }

        // A structure used for storing mesh information for a 3D object
        struct Mesh
        {
            public Mesh() { }
            public string name = "DefaultMesh"; // Used to identify different meshes
            public string materialName = "DefaultMaterial"; // Used to identify which material the mesh uses in the mtl file
            public List<Triangle> tris = new List<Triangle>();
            public List<Quadrilateral> quads = new List<Quadrilateral>();
            public Material material = new Material(); // Stores the mesh's material data
            public int[] Translation = new int[3] { 0, 0, 0 };
            public int[] Rotation = new int[3] { 0, 0, 0 };
            public int[] Scale = new int[3] { 1, 1, 1 };

        }
        // Currently only used for texture information
        struct Material
        {
            public Material() { }
            public float Ns = 250;
            public float[] Ka = new float[3] { 1, 1, 1 };
            public float[] Kd = new float[3];
            public float[] Ks = new float[3];
            public float[] Ke = new float[3];
            public float Ni = 1.45f;
            public float d = 1;
            public int illum = 2;
            public bool hasTexture = false;
            public string texturePath = "";
            public DirectBitmap texture = new DirectBitmap();
        }
        // A class that contains all the information regarding a view into the world
        class Viewport
        {
            public Viewport() { }
            public Viewport(int screenWidth, int screenHeight, int pixelWidth, int pixelHeight)
            {
                this.ScreenWidth = screenWidth / pixelWidth;
                this.ScreenHeight = screenHeight / pixelHeight;
                this.PixelWidth = pixelWidth;
                this.PixelHeight = pixelHeight;
                this.DepthBuffer = new float[screenWidth * screenHeight];
            }
            public Mat4x4 ProjMat = new Mat4x4(); // Matrix that converts from view space to screen space
            public Vec3D CameraPosition = new Vec3D(0, 0, -5); // Location of camera in world space
            public Vec3D LookDirection = new Vec3D(); // Direction vector along the direction camera points
            public float Pitch { get; set; } // Camera rotation about the x-axis
            public float Yaw { get; set; } // Camera rotation about the y-axis
            public float ThetaX { get; set; } // World rotation around x-axis
            public float ThetaY { get; set; } // World rotation around y-axis
            public float ThetaZ { get; set; } // World rotation around z-axis
            public int ScreenWidth { get; set; }
            public int ScreenHeight { get; set; }
            public int PixelWidth { get; set; }
            public int PixelHeight { get; set; }
            public float[] DepthBuffer { get; set; } = new float[0]; // Used to determine the z-depth of each screen pixel
            public DirectBitmap Frame { get; set; } = new DirectBitmap(1, 1); // A bitmap representing the frame drawn to the viewport

            // Takes in a plane and a triangle and creates 0-2 new triangles based
            // on how the triangle intersects the plane. Returns the number of new
            // triangles created.
            public int Triangle_ClipAgainstPlane(Vec3D plane_p, Vec3D plane_n, Triangle in_tri, ref Triangle out_tri1, ref Triangle out_tri2)
            {
                // Make sure plane normal is indeed normal
                plane_n = Vector_Normalize(ref plane_n);

                // Return signed shortest distance from point to plane, plane normal must be normalized
                Func<Vec3D, float> dist = (Vec3D p) =>
                {
                    return (Vector_DotProduct(ref plane_n, ref p) - Vector_DotProduct(ref plane_n, ref plane_p));
                };

                // Create two temporary storage arrays to classify points either side of plane
                // If distance sign is positive, point lies on "inside" of plane
                Vec3D[] inside_points = new Vec3D[3] { new Vec3D(), new Vec3D(), new Vec3D() };
                int nInsidePointCount = 0;
                Vec3D[] outside_points = new Vec3D[3] { new Vec3D(), new Vec3D(), new Vec3D() };
                int nOutsidePointCount = 0;
                Vec2D[] inside_tex = new Vec2D[3] { new Vec2D(), new Vec2D(), new Vec2D() };
                int nInsideTexCount = 0;
                Vec2D[] outside_tex = new Vec2D[3] { new Vec2D(), new Vec2D(), new Vec2D() };
                int nOutsideTexCount = 0;

                // Get signed distance of each point in triangle to plane
                float d0 = dist(in_tri.p[0]);
                float d1 = dist(in_tri.p[1]);
                float d2 = dist(in_tri.p[2]);

                if (d0 >= 0)
                {
                    inside_points[nInsidePointCount++] = in_tri.p[0];
                    inside_tex[nInsideTexCount++] = in_tri.t[0];
                }
                else
                {
                    outside_points[nOutsidePointCount++] = in_tri.p[0];
                    outside_tex[nOutsideTexCount++] = in_tri.t[0];
                }
                if (d1 >= 0)
                {
                    inside_points[nInsidePointCount++] = in_tri.p[1];
                    inside_tex[nInsideTexCount++] = in_tri.t[1];
                }
                else
                {
                    outside_points[nOutsidePointCount++] = in_tri.p[1];
                    outside_tex[nOutsideTexCount++] = in_tri.t[1];
                }
                if (d2 >= 0)
                {
                    inside_points[nInsidePointCount++] = in_tri.p[2];
                    inside_tex[nInsideTexCount++] = in_tri.t[2];
                }
                else
                {
                    outside_points[nOutsidePointCount++] = in_tri.p[2];
                    outside_tex[nOutsideTexCount++] = in_tri.t[2];
                }

                // Now classify triangle points, and break the input triangle into 
                // smaller output triangles if required. There are four possible
                // outcomes...
                if (nInsidePointCount == 0)
                {
                    // All points lie on the outside of plane, so clip whole triangle
                    // It ceases to exist

                    return 0; // No returned triangles are valid
                }
                else if (nInsidePointCount == 3)
                {
                    // All points lie on the inside of plane, so do nothing
                    // and allow the triangle to simply pass through
                    out_tri1 = in_tri;

                    return 1; // Just the one returned original triangle is valid
                }
                else if (nInsidePointCount == 1 && nOutsidePointCount == 2)
                {
                    // Triangle should be clipped. As two points lie outside
                    // the plane, the triangle simply becomes a smaller triangle

                    // Copy appearance info to new triangle
                    out_tri1.lum = in_tri.lum;

                    // The inside point is valid, so keep that...
                    out_tri1.p[0] = inside_points[0];
                    out_tri1.t[0] = inside_tex[0];

                    // but the two new points are at the locations where the 
                    // original sides of the triangle (lines) intersect with the plane
                    float t = 0;
                    out_tri1.p[1] = Vector_IntersectPlane(ref plane_p, ref plane_n, ref inside_points[0], ref outside_points[0], ref t);
                    out_tri1.t[1].u = t * (outside_tex[0].u - inside_tex[0].u) + inside_tex[0].u;
                    out_tri1.t[1].v = t * (outside_tex[0].v - inside_tex[0].v) + inside_tex[0].v;
                    out_tri1.t[1].w = t * (outside_tex[0].w - inside_tex[0].w) + inside_tex[0].w;

                    out_tri1.p[2] = Vector_IntersectPlane(ref plane_p, ref plane_n, ref inside_points[0], ref outside_points[1], ref t);
                    out_tri1.t[2].u = t * (outside_tex[1].u - inside_tex[0].u) + inside_tex[0].u;
                    out_tri1.t[2].v = t * (outside_tex[1].v - inside_tex[0].v) + inside_tex[0].v;
                    out_tri1.t[2].w = t * (outside_tex[1].w - inside_tex[0].w) + inside_tex[0].w;

                    return 1; // Return the newly formed single triangle
                }
                else if (nInsidePointCount == 2 && nOutsidePointCount == 1)
                {
                    // Triangle should be clipped. As two points lie inside the plane,
                    // the clipped triangle becomes a "quad". Fortunately, we can
                    // represent a quad with two new triangles

                    // Copy appearance info to new triangles
                    out_tri1.lum = in_tri.lum;
                    out_tri2.lum = in_tri.lum;

                    // The first triangle consists of the two inside points and a new
                    // point determined by the location where one side of the triangle
                    // intersects with the plane
                    out_tri1.p[0] = inside_points[0];
                    out_tri1.p[1] = inside_points[1];
                    out_tri1.t[0] = inside_tex[0];
                    out_tri1.t[1] = inside_tex[1];
                    float t = 0;
                    out_tri1.p[2] = Vector_IntersectPlane(ref plane_p, ref plane_n, ref inside_points[0], ref outside_points[0], ref t);
                    out_tri1.t[2].u = t * (outside_tex[0].u - inside_tex[0].u) + inside_tex[0].u;
                    out_tri1.t[2].v = t * (outside_tex[0].v - inside_tex[0].v) + inside_tex[0].v;
                    out_tri1.t[2].w = t * (outside_tex[0].w - inside_tex[0].w) + inside_tex[0].w;

                    // The second triangle is composed of one of he inside points, a
                    // new point determined by the intersection of the other side of the 
                    // triangle and the plane, and the newly created point above
                    out_tri2.p[0] = inside_points[1];
                    out_tri2.t[0] = inside_tex[1];
                    out_tri2.p[1] = out_tri1.p[2];
                    out_tri2.t[1] = out_tri1.t[2];
                    out_tri2.p[2] = Vector_IntersectPlane(ref plane_p, ref plane_n, ref inside_points[1], ref outside_points[0], ref t);
                    out_tri2.t[2].u = t * (outside_tex[0].u - inside_tex[1].u) + inside_tex[1].u;
                    out_tri2.t[2].v = t * (outside_tex[0].v - inside_tex[1].v) + inside_tex[1].v;
                    out_tri2.t[2].w = t * (outside_tex[0].w - inside_tex[1].w) + inside_tex[1].w;

                    return 2; // Return two newly formed triangles which form a quad
                }
                else
                {
                    return 0;
                }
            }
            // Prepares each triangle to be drawn. Each triangle that is ready
            // to be drawn is stored in the list that's passed in as an parameter
            public void PrepareForRasterization(Mat4x4 worldMat, Mat4x4 matView, Mat4x4 matTransform, Triangle tri, ref List<Triangle> vecTrianglesToRaster,
                bool culling)
            {
                Triangle triTransformed = new Triangle();
                // World Matrix Transform
                triTransformed.p[0] = tri.p[0] * worldMat * matTransform;
                triTransformed.p[1] = tri.p[1] * worldMat * matTransform;
                triTransformed.p[2] = tri.p[2] * worldMat * matTransform;
                triTransformed.t[0] = tri.t[0];
                triTransformed.t[1] = tri.t[1];
                triTransformed.t[2] = tri.t[2];
                triTransformed.drawSide0_1 = tri.drawSide0_1;
                triTransformed.drawSide1_2 = tri.drawSide1_2;
                triTransformed.drawSide2_0 = tri.drawSide2_0;

                // Calculate triangle's Normal 
                Vec3D normal, line1, line2;

                // Get lines on either side of triangle
                line1 = triTransformed.p[1] - triTransformed.p[0];
                line2 = triTransformed.p[2] - triTransformed.p[0];

                // Take the cross product of lines to get normal to triangle surface
                normal = Vector_CrossProduct(ref line1, ref line2);
                normal = Vector_Normalize(ref normal);

                // Get Ray from camera to triangle
                Vec3D vCameraRay = triTransformed.p[0] - CameraPosition;

                // If ray is aligned with normal then triangle is visible
                // if not it is culled, in other words, triangles with normals 
                // facing away from the camera won't be seen. If culling is 
                // disabled we draw the triangles regardless. 
                if (Vector_DotProduct(ref normal, ref vCameraRay) < 0.0f | !culling)
                {
                    // Illumination
                    Vec3D light_direction = new Vec3D(0.0f, -1.0f, 1.0f);
                    light_direction = Vector_Normalize(ref light_direction);
                    // How "aligned" are light direction and triangle surface normal?
                    // The less the triangle normal and the light direction are aligned
                    // the dimmer the triangle. Normal and light vectors are in opposite
                    // directions so we negate the dot product
                    float lum = MathF.Max(0.1f, -Vector_DotProduct(ref light_direction, ref normal));
                    triTransformed.lum = lum;

                    // Convert World Space --> View Space
                    Triangle triViewed = new Triangle();
                    triViewed.p[0] = triTransformed.p[0] * matView;
                    triViewed.p[1] = triTransformed.p[1] * matView;
                    triViewed.p[2] = triTransformed.p[2] * matView;
                    triViewed.lum = triTransformed.lum;
                    triViewed.drawSide0_1 = triTransformed.drawSide0_1;
                    triViewed.drawSide1_2 = triTransformed.drawSide1_2;
                    triViewed.drawSide2_0 = triTransformed.drawSide2_0;
                    triViewed.t[0] = triTransformed.t[0];
                    triViewed.t[1] = triTransformed.t[1];
                    triViewed.t[2] = triTransformed.t[2];

                    // Clip the Viewed Triangle against near plane, this could form two additional
                    // triangles.
                    Triangle[] clipped = new Triangle[2] { new Triangle(), new Triangle() };
                    int nClippedTriangles = Triangle_ClipAgainstPlane(new Vec3D(0.0f, 0.0f, 0.1f), new Vec3D(0.0f, 0.0f, 1.0f),
                        triViewed, ref clipped[0], ref clipped[1]);

                    // We may end up with multiple triangles form the clip, so project as
                    // required
                    for (int n = 0; n < nClippedTriangles; n++)
                    {
                        // Project triangles from 3D --> 2D
                        // View space -> screen space
                        Triangle triProjected = new Triangle();
                        triProjected.p[0] = clipped[n].p[0] * ProjMat;
                        triProjected.p[1] = clipped[n].p[1] * ProjMat;
                        triProjected.p[2] = clipped[n].p[2] * ProjMat;
                        triProjected.lum = clipped[n].lum;
                        triProjected.drawSide0_1 = clipped[n].drawSide0_1;
                        triProjected.drawSide1_2 = clipped[n].drawSide1_2;
                        triProjected.drawSide2_0 = clipped[n].drawSide2_0;
                        triProjected.t[0] = clipped[n].t[0];
                        triProjected.t[1] = clipped[n].t[1];
                        triProjected.t[2] = clipped[n].t[2];

                        // Divide the texture coordinates by z-component to add perspective
                        triProjected.t[0].u = triProjected.t[0].u / triProjected.p[0].w;
                        triProjected.t[1].u = triProjected.t[1].u / triProjected.p[1].w;
                        triProjected.t[2].u = triProjected.t[2].u / triProjected.p[2].w;

                        triProjected.t[0].v = triProjected.t[0].v / triProjected.p[0].w;
                        triProjected.t[1].v = triProjected.t[1].v / triProjected.p[1].w;
                        triProjected.t[2].v = triProjected.t[2].v / triProjected.p[2].w;

                        // Set texel depth to be reciprocal so we can get the un-normalized
                        // coordinates back
                        triProjected.t[0].w = 1.0f / triProjected.p[0].w;
                        triProjected.t[1].w = 1.0f / triProjected.p[1].w;
                        triProjected.t[2].w = 1.0f / triProjected.p[2].w;

                        // Each vertex is divided by the z-component to add perspective
                        triProjected.p[0] = triProjected.p[0] / triProjected.p[0].w;
                        triProjected.p[1] = triProjected.p[1] / triProjected.p[1].w;
                        triProjected.p[2] = triProjected.p[2] / triProjected.p[2].w;

                        // We must invert x because our system uses a left-hand coordinate system	
                        triProjected.p[0].x *= -1.0f;
                        triProjected.p[1].x *= -1.0f;
                        triProjected.p[2].x *= -1.0f;
                        // We must invert y because pixels are drawn top-down
                        triProjected.p[0].y *= -1.0f;
                        triProjected.p[1].y *= -1.0f;
                        triProjected.p[2].y *= -1.0f;

                        // Projection Matrix gives results from -1 to +1 through dividing by Z
                        // so we offset vertices to occupy the screen
                        Vec3D vOffsetView = new Vec3D(1, 1, 0);
                        triProjected.p[0] = triProjected.p[0] + vOffsetView;
                        triProjected.p[1] = triProjected.p[1] + vOffsetView;
                        triProjected.p[2] = triProjected.p[2] + vOffsetView;

                        // vertices are now between 0 and 2 so we scale into view
                        triProjected.p[0].x *= 0.5f * ScreenWidth;
                        triProjected.p[0].y *= 0.5f * ScreenHeight;
                        triProjected.p[1].x *= 0.5f * ScreenWidth;
                        triProjected.p[1].y *= 0.5f * ScreenHeight;
                        triProjected.p[2].x *= 0.5f * ScreenWidth;
                        triProjected.p[2].y *= 0.5f * ScreenHeight;

                        // Store triangle for sorting
                        vecTrianglesToRaster.Add(triProjected);
                    }
                }
            }
            // Converts a triangle's luminance to an argb color value
            public Color GetYellowShade(float lum)
            {
                return Color.FromArgb(255, (int)(lum * 255), (int)(lum * 255), 0);
            }
            public Color GetShade(float lum)
            {
                return Color.FromArgb(255, (int)(lum * 255), (int)(lum * 255), (int)(lum * 255));
            }

            // Draws a triangle to a bitmap. Depending on whether the user
            // has toggled shading or texturing, the color information of
            // each triangle will be used accordingly.
            public void DrawTriangle(Triangle tri, DirectBitmap texture, bool texturing = false, bool shading = true, bool isSelected = false)
            {
                // The pixel domain is integers. Can't move half a pixel
                int x1 = (int)tri.p[0].x;
                int y1 = (int)tri.p[0].y;
                float u1 = tri.t[0].u;
                float v1 = tri.t[0].v;
                float w1 = tri.t[0].w;

                int x2 = (int)tri.p[1].x;
                int y2 = (int)tri.p[1].y;
                float u2 = tri.t[1].u;
                float v2 = tri.t[1].v;
                float w2 = tri.t[1].w;

                int x3 = (int)tri.p[2].x;
                int y3 = (int)tri.p[2].y;
                float u3 = tri.t[2].u;
                float v3 = tri.t[2].v;
                float w3 = tri.t[2].w;

                float lum = tri.lum;

                // Swaps the variables so that y1 < y2 < y3
                // Lower y value = higher up on screen
                if (y2 < y1)
                {
                    (y1, y2) = (y2, y1);
                    (x1, x2) = (x2, x1);
                    (u1, u2) = (u2, u1);
                    (v1, v2) = (v2, v1);
                    (w1, w2) = (w2, w1);
                }

                if (y3 < y1)
                {
                    (y1, y3) = (y3, y1);
                    (x1, x3) = (x3, x1);
                    (u1, u3) = (u3, u1);
                    (v1, v3) = (v3, v1);
                    (w1, w3) = (w3, w1);
                }

                if (y3 < y2)
                {
                    (y2, y3) = (y3, y2);
                    (x2, x3) = (x3, x2);
                    (u2, u3) = (u3, u2);
                    (v2, v3) = (v3, v2);
                    (w2, w3) = (w3, w2);
                }

                // Variables relating to one side of triangle
                int dy1 = y2 - y1;
                int dx1 = x2 - x1;
                float dv1 = v2 - v1;
                float du1 = u2 - u1;
                float dw1 = w2 - w1;

                // Variables relating to other side of triangle
                int dy2 = y3 - y1;
                int dx2 = x3 - x1;
                float dv2 = v3 - v1;
                float du2 = u3 - u1;
                float dw2 = w3 - w1;

                // Texel coordinates per pixel
                float tex_u, tex_v, tex_w;

                // Depicts how much we step in that unit direction per row of pixels
                float dax_step = 0, dbx_step = 0,
                    du1_step = 0, dv1_step = 0,
                    du2_step = 0, dv2_step = 0,
                    dw1_step = 0, dw2_step = 0;

                // As long as the line is not horizontal, dy will not be zero                
                if (dy1 != 0)
                    // How much we step in that unit direction per pixel downwards
                    dax_step = dx1 / MathF.Abs(dy1);
                if (dy1 != 0)
                    du1_step = du1 / MathF.Abs(dy1);
                if (dy1 != 0)
                    dv1_step = dv1 / MathF.Abs(dy1);
                if (dy1 != 0)
                    dw1_step = dw1 / MathF.Abs(dy1);

                if (dy2 != 0)
                    dbx_step = dx2 / MathF.Abs(dy2);
                if (dy2 != 0)
                    du2_step = du2 / MathF.Abs(dy2);
                if (dy2 != 0)
                    dv2_step = dv2 / MathF.Abs(dy2);
                if (dy2 != 0)
                    dw2_step = dw2 / MathF.Abs(dy2);

                // As long as the line isn't flat, draw top half of triangle
                if (dy1 != 0)
                {
                    // For each row of pixels
                    for (int i = y1; i <= y2; i++)
                    {
                        // Get horizontal pixel start and end points
                        int ax = (int)(x1 + (i - y1) * dax_step);
                        int bx = (int)(x1 + (i - y1) * dbx_step);

                        // Get start and end points in texel space
                        float tex_su = u1 + (i - y1) * du1_step;
                        float tex_sv = v1 + (i - y1) * dv1_step;
                        float tex_sw = w1 + (i - y1) * dw1_step;

                        float tex_eu = u1 + (i - y1) * du2_step;
                        float tex_ev = v1 + (i - y1) * dv2_step;
                        float tex_ew = w1 + (i - y1) * dw2_step;
                        // Ensure we go from left to right in pixel space and flip any
                        // correlated components if necessary
                        if (ax > bx)
                        {
                            (ax, bx) = (bx, ax);
                            (tex_su, tex_eu) = (tex_eu, tex_su);
                            (tex_sv, tex_ev) = (tex_ev, tex_sv);
                            (tex_sw, tex_ew) = (tex_ew, tex_sw);
                        }

                        tex_u = tex_su;
                        tex_v = tex_sv;
                        tex_w = tex_sw;

                        // Stores interpolation along the line between start and end points
                        float t = 0.0f;
                        float tstep = 1.0f / (bx - ax);


                        // For each pixel in the row
                        for (int j = ax; j < bx; j++)
                        {
                            // Position in texel space for each pixel we cross
                            tex_u = (1.0f - t) * tex_su + t * tex_eu;
                            tex_v = (1.0f - t) * tex_sv + t * tex_ev;
                            // z-depth of the pixel. Higher value = closer pixel
                            tex_w = (1.0f - t) * tex_sw + t * tex_ew;
                            // If this pixel is closer than the one currently drawn here we draw 
                            // the new pixel
                            if (tex_w > DepthBuffer[i * ScreenWidth + j])
                            {
                                // Divide by tex_w to get denormalized texel coordinate
                                float u = (tex_u / tex_w);
                                // The v axis is in texel space is inverted compared to the y-axis in bitmaps
                                // 1 - v undoes the inversion
                                float v = (1 - tex_v / tex_w);
                                // uv coordinates are between 0 and 1. Anything outside those values will
                                // be wrapped through repetition 
                                u = u >= 0 ? u % 1 : (u % 1 + 1.0f) % 1;
                                v = v >= 0 ? v % 1 : (v % 1 + 1.0f) % 1;
                                // Scale up texel coordinates to the height and width of the textures
                                int w = (int)(u * texture.Width);
                                int h = (int)(v * texture.Height);
                                if (isSelected)
                                {
                                    if (shading)
                                    {
                                        Color col = GetYellowShade(lum);
                                        Frame.SetPixel(j, i, col);
                                    }
                                    else
                                    {
                                        Frame.SetPixel(j, i, Color.Yellow);
                                    }
                                }
                                else if (texturing)
                                {

                                    if (shading)
                                    {
                                        Color color = texture.GetPixel(w, h);
                                        // Naïve implementation of shading until mtl
                                        // processing is added
                                        color = Color.FromArgb(255, (int)(color.R * lum), (int)(color.G * lum), (int)(color.B * lum));
                                        Frame.SetPixel(j, i, color);
                                    }
                                    else
                                    {
                                        Color color = texture.GetPixel(w, h);
                                        Frame.SetPixel(j, i, color);
                                    }
                                }
                                else
                                {
                                    if (shading)
                                    {
                                        Color color = GetShade(lum);
                                        Frame.SetPixel(j, i, color);
                                    }
                                    else
                                    {
                                        Frame.SetPixel(j, i, Color.White);
                                    }

                                }
                                // Update depth buffer
                                DepthBuffer[i * ScreenWidth + j] = tex_w;
                            }
                            // Update interpolation
                            t += tstep;
                        }
                    }
                }

                // Recalculate values to work with bottom side of triangle
                dy1 = y3 - y2;
                dx1 = x3 - x2;
                dv1 = v3 - v2;
                du1 = u3 - u2;
                dw1 = w3 - w2;

                if (dy1 != 0)
                    dax_step = dx1 / MathF.Abs(dy1);
                if (dy2 != 0)
                    dbx_step = dx2 / MathF.Abs(dy2);

                du1_step = 0; dv1_step = 0;
                if (dy1 != 0)
                    du1_step = du1 / MathF.Abs(dy1);
                if (dy1 != 0)
                    dv1_step = dv1 / MathF.Abs(dy1);
                if (dy1 != 0)
                    dw1_step = dw1 / MathF.Abs(dy1);

                // As long as the line isn't flat, draw bottom half of triangle
                if (dy1 != 0)
                {
                    for (int i = y2; i <= y3; i++)
                    {
                        int ax = (int)(x2 + (i - y2) * dax_step);
                        int bx = (int)(x1 + (i - y1) * dbx_step);
                        float tex_su = u2 + (i - y2) * du1_step;
                        float tex_sv = v2 + (i - y2) * dv1_step;
                        float tex_sw = w2 + (i - y2) * dw1_step;

                        float tex_eu = u1 + (i - y1) * du2_step;
                        float tex_ev = v1 + (i - y1) * dv2_step;
                        float tex_ew = w1 + (i - y1) * dw2_step;

                        if (ax > bx)
                        {
                            (ax, bx) = (bx, ax);
                            (tex_su, tex_eu) = (tex_eu, tex_su);
                            (tex_sv, tex_ev) = (tex_ev, tex_sv);
                            (tex_sw, tex_ew) = (tex_ew, tex_sw);
                        }

                        tex_u = tex_su;
                        tex_v = tex_sv;
                        tex_w = tex_sw;

                        float tstep = 1.0f / (bx - ax);
                        float t = 0.0f;

                        for (int j = ax; j < bx; j++)
                        {
                            tex_u = (1.0f - t) * tex_su + t * tex_eu;
                            tex_v = (1.0f - t) * tex_sv + t * tex_ev;
                            tex_w = (1.0f - t) * tex_sw + t * tex_ew;

                            if (tex_w > DepthBuffer[i * ScreenWidth + j])
                            {
                                float u = (tex_u / tex_w);
                                float v = (1 - tex_v / tex_w);
                                u = u >= 0 ? u % 1 : (u % 1 + 1.0f) % 1;
                                v = v >= 0 ? v % 1 : (v % 1 + 1.0f) % 1;
                                int w = (int)(u * texture.Width);
                                int h = (int)(v * texture.Height);
                                if (isSelected)
                                {
                                    if (shading)
                                    {
                                        Color col = GetYellowShade(lum);
                                        Frame.SetPixel(j, i, col);
                                    }
                                    else
                                    {
                                        Frame.SetPixel(j, i, Color.Yellow);
                                    }
                                }
                                else if (texturing)
                                {

                                    if (shading)
                                    {
                                        Color color = texture.GetPixel(w, h);
                                        // Naïve implementation of shading until mtl
                                        // processing is added
                                        color = Color.FromArgb(255, (int)(color.R * lum), (int)(color.G * lum), (int)(color.B * lum));
                                        Frame.SetPixel(j, i, color);
                                    }
                                    else
                                    {
                                        Color color = texture.GetPixel(w, h);
                                        Frame.SetPixel(j, i, color);
                                    }
                                }
                                else
                                {
                                    if (shading)
                                    {
                                        Color color = GetShade(lum);
                                        Frame.SetPixel(j, i, color);
                                    }
                                    else
                                    {
                                        Frame.SetPixel(j, i, Color.White);
                                    }

                                }
                                DepthBuffer[i * ScreenWidth + j] = tex_w;
                            }
                            t += tstep;
                        }
                    }
                }
            }

            // Faster DrawLine than using Graphics.DrawLine.
            // Taken from the link below
            // https://rosettacode.org/wiki/Bitmap/Bresenham%27s_line_algorithm#C#
            public void DrawLine(int x0, int y0, int x1, int y1, Color color)
            {
                int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
                int dy = Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
                int err = (dx > dy ? dx : -dy) / 2, e2;
                for (; ; )
                {
                    Frame.SetPixel(x0, y0, color);
                    if (x0 == x1 && y0 == y1) break;
                    e2 = err;
                    if (e2 > -dx)
                    {
                        err -= dy;
                        x0 += sx;
                    }
                    if (e2 < dy)
                    {
                        err += dx;
                        y0 += sy;
                    }
                }
            }
        }

        // A bitmap class used for efficient getting and setting of pixels
        // by A.Konzel. Taken from the link below
        // https://stackoverflow.com/questions/24701703/c-sharp-faster-alternatives-to-setpixel-and-getpixel-for-bitmaps-for-windows-f
        public class DirectBitmap : IDisposable
        {
            public DirectBitmap() { }

            // Initializes a bitmap from an image file
            public DirectBitmap(string filePath)
            {
                // Create a temporary bitmap
                Bitmap bitmap = new Bitmap(filePath);
                Width = bitmap.Width;
                Height = bitmap.Height;
                Pixels = new Int32[Width * Height];
                // Lock the bitmap in memory
                BitmapData data = bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                // Get the address of the bitmap data in memory
                IntPtr ptr = data.Scan0;
                // Copy the ARGB values from the temporary bitmap to a new pixel data array
                Marshal.Copy(ptr, Pixels, 0, Width * Height);
                BitsHandle = GCHandle.Alloc(Pixels, GCHandleType.Pinned);
                // Create a new bitmap that points to that array;
                Bitmap = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppArgb, BitsHandle.AddrOfPinnedObject());
                // Unlock the bits.
                bitmap.UnlockBits(data);
                bitmap.Dispose();
            }

            public DirectBitmap(int width, int height)
            {
                Width = width;
                Height = height;
                Pixels = new Int32[Width * Height];
                BitsHandle = GCHandle.Alloc(Pixels, GCHandleType.Pinned);
                Bitmap = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppArgb, BitsHandle.AddrOfPinnedObject());
            }
            // Initializes a bitmap consisting of one color
            public DirectBitmap(int width, int height, Color color)
            {
                Width = width;
                Height = height;
                Pixels = new Int32[Width * Height];
                int argb = color.ToArgb();
                for (int i = 0; i < Width * Height; i++)
                {
                    Pixels[i] = argb;
                }
                BitsHandle = GCHandle.Alloc(Pixels, GCHandleType.Pinned);
                Bitmap = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppArgb, BitsHandle.AddrOfPinnedObject());
            }

            public Bitmap Bitmap { get; private set; } // The actual bitmap
            public Int32[] Pixels { get; private set; } // Color data for each pixel
            public bool Disposed { get; private set; }
            public int Height { get; private set; }
            public int Width { get; private set; }
            protected GCHandle BitsHandle { get; private set; } // Used for retaining the pixel color data in memory

            public void SetPixel(int x, int y, Color color)
            {
                Pixels[y * Width + x] = color.ToArgb();
            }

            public Color GetPixel(int x, int y)
            {
                return Color.FromArgb(Pixels[y * Width + x]);
            }

            public void Dispose()
            {
                if (Disposed)
                    return;
                Disposed = true;
                Bitmap.Dispose();
                BitsHandle.Free();
            }
        }

        // Reads and loads in objects from an obj file. Returns false if
        // no objects could be loaded.
        private bool LoadObjectsFromFile(string filename, ref string materialFileName)
        {
            // Assume no mtl file unless one is given in the file
            materialFileName = "";

            Mesh mesh = new Mesh();

            try
            {
                // Create an instance of StreamReader to read from a file.
                // The 'using' statement also closes the StreamReader
                using (StreamReader sr = new StreamReader(filename))
                {
                    // Store the vertices and texel coordinates in parallel lists.
                    List<Vec3D> verts = new List<Vec3D>();
                    List<Vec2D> texs = new List<Vec2D>();
                    string? line;
                    // Read lines from the file until the end of the file is reached
                    while ((line = sr.ReadLine()) != null)
                    {
                        // Catch any empty lines
                        if (line.Length == 0)
                            continue;

                        // If the line begins with 'm' it specifies the material library template file
                        if (line[0] == 'm')
                        {
                            // Split the line via spaces since the first string specifies the
                            // type of line.
                            string[] name = line.Split(' ');
                            // Sometimes the file name has spaces in it so rebuild the original
                            // line without the first part
                            materialFileName = string.Join(" ", name.Skip(1).ToArray());
                        }
                        // 'o' specifies a new object group
                        else if (line[0] == 'o')
                        {
                            // Add the last mesh to the list of meshes. If there are multiple
                            // object groups listed, this results in the first mesh added being
                            // empty and leaves out the last mesh listed in the file. This is
                            // corrected later on
                            Meshes.Add(mesh);
                            // Everything underneath is a new object group
                            mesh = new Mesh();
                            // Assumes the object group name doesn't have spaces in it
                            mesh.name = line.Split(' ')[1];
                        }
                        else if (line[0] == 'v')
                        {
                            // Each coordinate is space separated
                            string[] coords = line.Split(' ');
                            // If the line begins with 'vt' it contains texel coordinates
                            if (line[1] == 't')
                            {
                                Vec2D vec = new Vec2D
                                {
                                    // 0th coord is junk character
                                    u = float.Parse(coords[1]),
                                    v = float.Parse(coords[2])
                                };
                                texs.Add(vec);
                            }
                            // If the line begins only with 'v' it contains vertex coordinates
                            else
                            {
                                Vec3D vec = new Vec3D
                                {
                                    x = float.Parse(coords[1]),
                                    y = float.Parse(coords[2]),
                                    z = float.Parse(coords[3])
                                };
                                verts.Add(vec);
                            }
                        }
                        // 'u' specifies that the mesh uses a material in the mtl file
                        else if (line[0] == 'u')
                        {
                            mesh.materialName = line.Split(' ')[1];
                        }
                        // 'f' specifies vertex and texel coordinates for each face
                        // through indices into each list
                        else if (line[0] == 'f')
                        {
                            // Lines without '/' do not have texel (or normal) coordinates listed
                            // for the face
                            if (!line.Contains('/'))
                            {
                                string[] indices = line.Split(' ');
                                // Triangles will have 3 indices (plus one for the junk character
                                // at the beginning) 
                                if (indices.Length == 4)
                                {
                                    Triangle triangle = new Triangle();
                                    // Index through pool of vertices to get the ones corresponding
                                    // to this face. obj files use 1 indexing so our indices are off
                                    // by 1.
                                    triangle.p[0] = verts[int.Parse(indices[1]) - 1];
                                    triangle.p[1] = verts[int.Parse(indices[2]) - 1];
                                    triangle.p[2] = verts[int.Parse(indices[3]) - 1];
                                    mesh.tris.Add(triangle);
                                }
                                // Quadrilaterals will have 4 indices
                                else if (indices.Length == 5)
                                {
                                    Quadrilateral quadrilateral = new Quadrilateral();
                                    quadrilateral.p[0] = verts[int.Parse(indices[1]) - 1];
                                    quadrilateral.p[1] = verts[int.Parse(indices[2]) - 1];
                                    quadrilateral.p[2] = verts[int.Parse(indices[3]) - 1];
                                    quadrilateral.p[3] = verts[int.Parse(indices[4]) - 1];
                                    mesh.quads.Add(quadrilateral);
                                }
                            }
                            else
                            {
                                string[] indexPairs = line.Split(' ');
                                if (indexPairs.Length == 4)
                                {
                                    // Temporary arrays to store the indices for the vertices and texel
                                    // coordinates
                                    int[] p = new int[3];
                                    int[] t = new int[3];
                                    for (int i = 0; i < 3; i++)
                                    {
                                        // Vertex and texel coordinate indices are separated by '/' 
                                        string[] p_t = indexPairs[i + 1].Split('/');
                                        p[i] = int.Parse(p_t[0]);
                                        t[i] = int.Parse(p_t[1]);
                                    }
                                    Triangle triangle = new Triangle();
                                    for (int i = 0; i < 3; i++)
                                    {
                                        triangle.p[i] = verts[p[i] - 1];
                                        triangle.t[i] = texs[t[i] - 1];
                                    }
                                    mesh.tris.Add(triangle);
                                }
                                else if (indexPairs.Length == 5)
                                {
                                    int[] p = new int[4];
                                    int[] t = new int[4];
                                    for (int i = 0; i < 4; i++)
                                    {
                                        string[] p_t = indexPairs[i + 1].Split('/');
                                        p[i] = int.Parse(p_t[0]);
                                        t[i] = int.Parse(p_t[1]);
                                    }
                                    Quadrilateral quadrilateral = new Quadrilateral();
                                    for (int i = 0; i < 4; i++)
                                    {
                                        quadrilateral.p[i] = verts[p[i] - 1];
                                        quadrilateral.t[i] = texs[t[i] - 1];
                                    }
                                    mesh.quads.Add(quadrilateral);
                                }
                            }
                        }
                    }
                }
                // Whether the file specifies obj groups or not, the last
                // mesh has not yet been added to our list of meshes so we
                // add it now
                Meshes.Add(mesh);
                // There's an extra mesh (the first one) in the list if there
                // was at least one object group specified in the file. Thus,
                // it needs to be removed
                if (Meshes.Count > 1)
                {
                    Meshes.RemoveAt(0);
                }
                foreach (Mesh m in Meshes)
                {
                    ObjectList.Items.Add(m.name);
                }

                return true;
            }
            // TODO: have several different catches based on the error
            // encountered
            catch
            {
                ResetWorld();
                return false;
            }
        }

        // Loads the material file associated with an object file and
        // maps each material's identifying name to it's material template
        // via a dictionary that's passed in. Returns true if it
        // successfully parsed the file (not including textures) 
        private bool LoadMaterialsFromFile(string filename, string folderPath, Dictionary<string, Material> object_Material)
        {
            string matName = "";
            Material material = new Material();
            try
            {
                using (StreamReader sr = new StreamReader(filename))
                {
                    string? line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        // Catch any empty lines
                        if (line.Length == 0)
                            continue;
                        // 'n' specifies the material's identifying name
                        if (line[0] == 'n')
                        {
                            // Much like for obj files, adds the last material to the
                            // material mapping. Yet again, this results in the first
                            // mapping added to be a default material template and leaves
                            // out the last material template listed in the file.
                            object_Material[matName] = material;
                            material = new Material();
                            matName = line.Split(' ')[1];
                        }
                        else if (line[0] == 'N')
                        {
                            if (line[1] == 's')
                            {
                                string[] ns = line.Split(' ');
                                material.Ns = float.Parse(ns[1]);
                            }
                            if (line[1] == 'i')
                            {
                                string[] ni = line.Split(' ');
                                material.Ni = float.Parse(ni[1]);
                            }
                        }
                        else if (line[0] == 'K')
                        {
                            if (line[1] == 'a')
                            {
                                string[] ka = line.Split(' ');
                                material.Ka[0] = float.Parse(ka[1]);
                                material.Ka[1] = float.Parse(ka[2]);
                                material.Ka[2] = float.Parse(ka[3]);
                            }
                            else if (line[1] == 'd')
                            {
                                string[] kd = line.Split(' ');
                                material.Kd[0] = float.Parse(kd[1]);
                                material.Kd[1] = float.Parse(kd[2]);
                                material.Kd[2] = float.Parse(kd[3]);
                            }
                            else if (line[1] == 's')
                            {
                                string[] ks = line.Split(' ');
                                material.Ks[0] = float.Parse(ks[1]);
                                material.Ks[1] = float.Parse(ks[2]);
                                material.Ks[2] = float.Parse(ks[3]);
                            }
                            else if (line[1] == 'e')
                            {
                                string[] ke = line.Split(' ');
                                material.Ke[0] = float.Parse(ke[1]);
                                material.Ke[1] = float.Parse(ke[2]);
                                material.Ke[2] = float.Parse(ke[3]);
                            }

                        }
                        else if (line[0] == 'd')
                        {
                            string[] d = line.Split(' ');
                            material.d = float.Parse(d[1]);
                        }
                        else if (line[0] == 'i')
                        {
                            string[] i = line.Split(' ');
                            material.illum = int.Parse(i[1]);
                        }
                        // 'n' specifies the material's texture path
                        else if (line[0] == 'm')
                        {
                            string[] texName = line.Split(' ');
                            // Account for spaces in the filepath
                            material.texturePath = string.Join(" ", texName.Skip(1).ToArray());
                            try
                            {
                                // Look for the image through a relative path
                                material.texture = new DirectBitmap(Path.Combine(folderPath, material.texturePath));
                                material.hasTexture = true;

                            }
                            catch (ArgumentException)
                            {
                                try
                                {
                                    // Look for the image through an absolute path
                                    material.texture = new DirectBitmap(material.texturePath);
                                    material.hasTexture = true;
                                }
                                catch (ArgumentException)
                                {
                                    MessageBox.Show($"Could not find texture file '{material.texturePath}'");
                                }
                            }
                        }
                    }
                }
                // Add the last material template to the dictionary
                object_Material[matName] = material;
                // Remove the default material template if it exists in
                // the dictionary
                object_Material.Remove("");
                return true;
            }
            catch
            {
                object_Material.Clear();
                return false;
            }
        }

        // Returns all camera related settings to default
        private void ResetCamera()
        {
            MainView.CameraPosition = new Vec3D(0, 0, -5);
            MainView.LookDirection = new Vec3D();
            MainView.Yaw = 0;
            MainView.Pitch = 0;
            MainView.ThetaX = 0;
            MainView.ThetaY = 0;
            MainView.ThetaZ = 0;
            // CameraSpeedSlider.Value = 8;
        }

        private void ResetWorld()
        {
            Meshes.Clear();
            ObjectList.Items.Clear();
        }

        private Mat4x4 GetTransformationMatrix(Mesh mesh)
        {

            Mat4x4 matTranslate = Matrix_MakeTranslation(mesh.Translation[0], mesh.Translation[1], mesh.Translation[2]);
            Mat4x4 matScale = Matrix_MakeScale(mesh.Scale[0], mesh.Scale[1], mesh.Scale[2]);
            Mat4x4 matRotate = Matrix_MakeRotationX(mesh.Rotation[0]) * Matrix_MakeRotationY(mesh.Rotation[1]) * Matrix_MakeRotationZ(mesh.Rotation[2]);
            return matScale * matRotate * matTranslate;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // this.DoubleBuffered = true; // May not be needed since picturebox is already double buffered
            Stopwatch.Start();
            // How often the tick event is run
            Clock.Interval = 20;
            Clock.Enabled = true;
            // Initialize the class and components pertaining to the main view
            // into the world. Increasing pixel width and height improves performance
            // at little at the cost of a lot of visual quality
            MainView = new Viewport(ViewWindow.Width, ViewWindow.Height, 1, 1);
            // Setup Projection Matrix
            MainView.ProjMat = Matrix_MakeProjection(90, (float)MainView.ScreenHeight / (float)MainView.ScreenWidth, 0.1f, 1000.0f);
        }

        private void Clock_Tick(object sender, EventArgs e)
        {
            Tock = (float)Stopwatch.Elapsed.TotalSeconds;
            // Get the time it took to render the previous frame
            ElapsedTime = Tock - Tick;
            Tick = Tock;
            // Run the paint event to render the current frame
            ViewWindow.Refresh();
            FrameCount += 1;
            FrameTime += ElapsedTime;
            // When FrameTime surpasses a second, display how many
            // frames were drawn in that time period. This shows how
            // many frames were rendered per second
            if (FrameTime >= 1.0f)
            {
                FPS = FrameCount;
                FrameCount = 0;
                // Possibly will change to 'FrameTime = 0' later
                FrameTime -= 1;
            }
            Text = $"3DModeler - FPS: {FPS} - Frame: {FrameCount}";
            // FPS calculation method from https://github.com/OneLoneCoder/olcPixelGameEngine/blob/147c25a018c917030e59048b5920c269ef583c50/olcPixelGameEngine.h#L3823
        }

        // Called once per clock tick event.
        // Renders each frame
        private void Viewer_Paint(object sender, PaintEventArgs e)
        {
            float speed = CameraSpeedSlider.Value;
            // Will be replaced with always moving upwards (no matter camera orientation) later
            if (KeyPressed[Keys.O])
                MainView.CameraPosition.y += speed * ElapsedTime; // Travel along positive y-axis
            // Will be replaced with always moving downwards
            if (KeyPressed[Keys.L])
                MainView.CameraPosition.y -= speed * ElapsedTime; // Travel along negative y-axis

            // Will be replaced with always moving leftwards
            if (KeyPressed[Keys.K])
                MainView.CameraPosition.x -= speed * ElapsedTime; // Travel along negative x-axis
            // Will be replaced with always moving rightwards
            if (KeyPressed[Keys.OemSemicolon])
                MainView.CameraPosition.x += speed * ElapsedTime; // Travel along positive x-axis

            // A velocity vector used to control the forward movement of the camera
            Vec3D Velocity = MainView.LookDirection * (speed * ElapsedTime);

            // Moves camera forward
            if (KeyPressed[Keys.W])
                MainView.CameraPosition += Velocity;
            // Moves camera backward
            if (KeyPressed[Keys.S])
                MainView.CameraPosition -= Velocity;

            // Camera looks leftwards
            if (KeyPressed[Keys.A])
                MainView.Yaw -= 2.0f * ElapsedTime;
            // Camera looks rightwards
            if (KeyPressed[Keys.D])
                MainView.Yaw += 2.0f * ElapsedTime;
            // Camera looks upwards
            if (KeyPressed[Keys.R])
                MainView.Pitch -= 2.0f * ElapsedTime;
            // Camera looks downwards
            if (KeyPressed[Keys.F])
                MainView.Pitch += 2.0f * ElapsedTime;


            // Set up "World Transform"
            Mat4x4 worldMat = Matrix_MakeIdentity();

            // Rotates the world
            Mat4x4 worldMatRotX = Matrix_MakeRotationX(MainView.ThetaX);
            Mat4x4 worldMatRotY = Matrix_MakeRotationY(MainView.ThetaY);
            Mat4x4 worldMatRotZ = Matrix_MakeRotationZ(MainView.ThetaZ);

            // Scales the world
            Mat4x4 worldMatScale = Matrix_MakeScale(1, 1, 1);

            // Offsets the world
            Mat4x4 worldMatTrans = Matrix_MakeTranslation(0.0f, 0.0f, 0.0f);

            // Apply transformations in correct order
            worldMat *= worldMatScale; // Transform by scaling
            worldMat *= worldMatRotX * worldMatRotY * worldMatRotZ; // Transform by rotation
            worldMat *= worldMatTrans; // Transform by translation

            // Create "Point At" Matrix for camera
            Vec3D vUp = new Vec3D(0, 1, 0); // Default up direction for camera is along the positive y-axis
            Vec3D vForwardCam = new Vec3D(0, 0, 1); // Default forward direction for camera is along the positive z-axis

            // TODO: Fix this. (gets choppy when looking directly up/down)
            MainView.Pitch = MainView.Pitch > 0 ? MathF.Min(3.1415f / 2, MainView.Pitch) : MathF.Max(-3.1415f / 2, MainView.Pitch); // Cap pitch from being able to rotate too far.
            Mat4x4 cameraMatRotX = Matrix_MakeRotationX(MainView.Pitch);
            Mat4x4 cameraMatRotY = Matrix_MakeRotationY(MainView.Yaw);
            // Rotated forward vector becomes the camera's look direction
            MainView.LookDirection = vForwardCam * cameraMatRotX * cameraMatRotY;
            // Offset the look direction to the camera location to get the target the camera points at
            Vec3D vTarget = MainView.CameraPosition + MainView.LookDirection;
            // Construct the "Point At" matrix
            Mat4x4 matCamera = Matrix_PointAt(ref MainView.CameraPosition, ref vTarget, ref vUp);

            // Construct the "Look At" matrix from the "Point At" matrix inverse
            Mat4x4 matView = Matrix_QuickInverse(ref matCamera);

            // Dispose of the previous frame
            MainView.Frame.Dispose();
            // Create a new background color for the frame
            ViewWindow.BackColor = Color.Cyan;
            // By default, the bitmap produced is entirely transparent
            MainView.Frame = new DirectBitmap(MainView.ScreenWidth, MainView.ScreenHeight);
            // Clear depth buffer each frame
            for (int i = 0; i < MainView.ScreenWidth * MainView.ScreenHeight; i++)
            {
                MainView.DepthBuffer[i] = 0.0f;
            }


            // Draw each mesh
            for (int i = 0; i < Meshes.Count; i++)
            {
                Mesh mesh = Meshes[i];

                // Get Transformation Matrix
                Mat4x4 matTransform = GetTransformationMatrix(mesh);


                bool isSelected = false;
                if (ObjectList.SelectedIndex == i)
                {
                    isSelected = true;
                }
                // Store triangles for rasterization later
                List<Triangle> vecTrianglesToRaster = new List<Triangle>();

                // Prepare each triangle for drawing
                foreach (Triangle tri in mesh.tris)
                {
                    MainView.PrepareForRasterization(worldMat, matView, matTransform, tri, ref vecTrianglesToRaster, CullingToolStripMenuItem.Checked);
                }
                // Split each quad into two tris and prepare both tris for drawing.
                // To keep the quad from looking like two tris in wireframe mode, a
                // hack to keep specific sides from being drawn is used
                foreach (Quadrilateral quad in mesh.quads)
                {
                    Triangle tri1 = new Triangle(quad.p[0], quad.p[1], quad.p[2], quad.t[0], quad.t[1], quad.t[2]);
                    tri1.drawSide2_0 = false;
                    MainView.PrepareForRasterization(worldMat, matView, matTransform, tri1, ref vecTrianglesToRaster, CullingToolStripMenuItem.Checked);
                    Triangle tri2 = new Triangle(quad.p[0], quad.p[2], quad.p[3], quad.t[0], quad.t[2], quad.t[3]);
                    tri2.drawSide0_1 = false;
                    MainView.PrepareForRasterization(worldMat, matView, matTransform, tri2, ref vecTrianglesToRaster, CullingToolStripMenuItem.Checked);
                }

                // Sort triangles from back to front through approximating
                // the triangles' z positions. Useful for transparency. Currently
                // used as a hack to prevent wireframe being seen through solid objects
                // (Does not currently work if there are multiple meshes)
                if (SolidToolStripMenuItem.Checked & WireframeToolStripMenuItem.Checked)
                {
                    vecTrianglesToRaster.Sort((Triangle t1, Triangle t2) =>
                    {
                        float z1 = (t1.p[0].z + t1.p[1].z + t1.p[2].z) / 3.0f;
                        float z2 = (t2.p[0].z + t2.p[1].z + t2.p[2].z) / 3.0f;
                        if (z2 - z1 > 0)
                        {
                            return 1;
                        }
                        else if (z1 - z2 == 0)
                        {
                            return 0;
                        }
                        else
                        {
                            return -1;
                        }
                    });
                }

                // Loop through all transformed, viewed, projected, and sorted triangles
                foreach (Triangle triToRaster in vecTrianglesToRaster)
                {
                    // Clip triangles against all four screen edges, this could yield
                    // a bunch of triangles, so create a queue that we traverse to 
                    // ensure we only test new triangles generated against planes
                    Queue<Triangle> listTriangles = new Queue<Triangle>();

                    // Add initial triangle
                    listTriangles.Enqueue(triToRaster);
                    int nNewTriangles = 1;

                    for (int p = 0; p < 4; p++)
                    {
                        int nTrisToAdd = 0;
                        while (nNewTriangles > 0)
                        {
                            // Take triangle from front of queue
                            Triangle test = listTriangles.Dequeue();
                            nNewTriangles--;

                            // Clip it against a plane. We only need to test each 
                            // subsequent plane, against subsequent new triangles
                            // as all triangles after a plane clip are guaranteed
                            // to lie on the inside of the plane.
                            Triangle[] clipped = new Triangle[2] { new Triangle(), new Triangle() };
                            switch (p)
                            {
                                case 0:
                                    nTrisToAdd = MainView.Triangle_ClipAgainstPlane(new Vec3D(0.0f, 0.0f, 0.0f), new Vec3D(0.0f, 1.0f, 0.0f),
                                        test, ref clipped[0], ref clipped[1]);
                                    break;
                                case 1:
                                    nTrisToAdd = MainView.Triangle_ClipAgainstPlane(new Vec3D(0.0f, (float)MainView.ScreenHeight - 1, 0.0f),
                                        new Vec3D(0.0f, -1.0f, 0.0f), test, ref clipped[0], ref clipped[1]);
                                    break;
                                case 2:
                                    nTrisToAdd = MainView.Triangle_ClipAgainstPlane(new Vec3D(0.0f, 0.0f, 0.0f), new Vec3D(1.0f, 0.0f, 0.0f),
                                        test, ref clipped[0], ref clipped[1]);
                                    break;
                                case 3:
                                    nTrisToAdd = MainView.Triangle_ClipAgainstPlane(new Vec3D((float)MainView.ScreenWidth - 1, 0.0f, 0.0f),
                                        new Vec3D(-1.0f, 0.0f, 0.0f), test, ref clipped[0], ref clipped[1]);
                                    break;
                            }

                            // Clipping may yield a variable number of triangles, so
                            // add these new ones to the back of the queue for subsequent
                            // clipping against next planes
                            for (int w = 0; w < nTrisToAdd; w++)
                                listTriangles.Enqueue(clipped[w]);
                        }
                        nNewTriangles = listTriangles.Count();
                    }

                    // Draw the transformed, viewed, projected, sorted, clipped triangles to a bitmap
                    foreach (Triangle t in listTriangles)
                    {
                        if (SolidToolStripMenuItem.Checked)
                            MainView.DrawTriangle(t, mesh.material.texture, mesh.material.hasTexture & TextureToolStripMenuItem.Checked,
                                ShadingToolStripMenuItem.Checked, isSelected);

                        if (WireframeToolStripMenuItem.Checked)
                        {
                            // Currently does not work properly as the wireframe for an object will be drawn
                            // regardless of if it's behind another object. Sorting the triangles first helps
                            // mitigate the problem
                            if (t.drawSide0_1)
                                MainView.DrawLine((int)t.p[0].x, (int)t.p[0].y, (int)t.p[1].x, (int)t.p[1].y, Color.Black);

                            if (t.drawSide1_2)
                                MainView.DrawLine((int)t.p[1].x, (int)t.p[1].y, (int)t.p[2].x, (int)t.p[2].y, Color.Black);

                            if (t.drawSide2_0)
                                MainView.DrawLine((int)t.p[2].x, (int)t.p[2].y, (int)t.p[0].x, (int)t.p[0].y, Color.Black);
                        }
                    }
                }
            }

            // Speeds up rendering when pixel width/height is large
            //e.Graphics.CompositingMode = CompositingMode.SourceCopy; // Produces borders on viewport edges
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

            // Draw the bitmap to the screen
            e.Graphics.DrawImage(MainView.Frame.Bitmap, 0, 0, MainView.PixelWidth * MainView.ScreenWidth,
                MainView.PixelHeight * MainView.ScreenHeight);
        }

        // Sets the current state of any pressed key
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            KeyPressed[e.KeyCode] = true;
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            KeyPressed[e.KeyCode] = false;
        }

        private void Viewer_MouseDown(object sender, MouseEventArgs e)
        {
            LastCursorPosition = Cursor.Position;
            MousePressed = true;
        }

        private void Viewer_MouseUp(object sender, MouseEventArgs e)
        {
            MousePressed = false;
        }

        private void Viewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (MousePressed)
            {
                // Basic cursor controls are implemented however the camera's orientation
                // is not taken into account yet 
                MainView.ThetaY -= (Cursor.Position.X - LastCursorPosition.X) * 0.005f;
                MainView.ThetaX -= (Cursor.Position.Y - LastCursorPosition.Y) * 0.005f;
                LastCursorPosition = Cursor.Position;
            }
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            // Updates the viewport with the form
            MainView.ScreenWidth = ViewWindow.Width / MainView.PixelWidth;
            MainView.ScreenHeight = ViewWindow.Height / MainView.PixelHeight;
            MainView.DepthBuffer = new float[MainView.ScreenWidth * MainView.ScreenHeight];
            MainView.ProjMat = Matrix_MakeProjection(90, (float)MainView.ScreenHeight / (float)MainView.ScreenWidth, 0.1f, 1000.0f);
        }

        // Basic implementation of opining obj files. Not fully complete
        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // Setup OpenFileDialog properties
                openFileDialog.Filter = "All files (*.*)|*.*|obj files (*.obj)|*.obj";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = false;

                // If an obj file is selected
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Delete meshed already in the world            
                    ResetWorld();

                    string filePath = openFileDialog.FileName;
                    string folderPath = Path.GetDirectoryName(filePath);
                    string materialName = "";
                    if (LoadObjectsFromFile(filePath, ref materialName))
                    {
                        if (materialName != "")
                        {
                            string materialPath = Path.Combine(folderPath, materialName);
                            Dictionary<string, Material> materialMap = new Dictionary<string, Material>();
                            if (LoadMaterialsFromFile(materialPath, folderPath, materialMap))
                            {
                                // For each mesh, we find and link its material template
                                // from the mtl file. Otherwise, it will have a default
                                // material template
                                for (int i = 0; i < Meshes.Count; i++)
                                {
                                    Mesh newMesh = Meshes[i];
                                    if (materialMap.ContainsKey(newMesh.materialName))
                                    {
                                        newMesh.material = materialMap[newMesh.materialName];
                                        Meshes[i] = newMesh;
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("Could not load material template library file", "File Load Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Could not load object file", "File Load Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        // Basic obj file saving. Only saves vertex and face information atm
        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Can't save anything if there are no meshes
            if (Meshes.Count != 0)
            {
                // Initialize SaveFileDialog
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "obj files (*.obj)|*.obj";
                saveFileDialog1.FilterIndex = 2;
                saveFileDialog1.RestoreDirectory = false;

                // If a valid filename was chosen...
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    // Initialize various path names
                    string directoryPath = Path.GetDirectoryName(saveFileDialog1.FileName);
                    string chosenName = Path.GetFileNameWithoutExtension(saveFileDialog1.FileName);
                    string objPath = saveFileDialog1.FileName;
                    string mtlPath = Path.Combine(directoryPath, chosenName + ".mtl");

                    // Write the obj file
                    using (StreamWriter outputFile = new StreamWriter(objPath))
                    {
                        // Specify mtl file
                        outputFile.WriteLine($"mtllib {chosenName}.mtl");
                        // Pool of vertices start with an index of 1
                        int vertIndex = 1;
                        int texIndex = 1;
                        foreach (Mesh mesh in Meshes)
                        {
                            // Apply transformations
                            Mat4x4 transform = GetTransformationMatrix(mesh);

                            // As we loop through each polygon, we store each vertex and
                            // its index into the pool
                            Dictionary<Vec3D, int> indexOfVertex = new Dictionary<Vec3D, int>();
                            Dictionary<Vec2D, int> indexOfTexel = new Dictionary<Vec2D, int>();
                            // We also must keep track of the indices of vertices that
                            // correspond to each face
                            List<int[]> vertFaces = new List<int[]>();
                            List<int[]> texFaces = new List<int[]>();
                            // Specify new object group
                            outputFile.WriteLine($"o {mesh.name}");

                            foreach (Triangle tri in mesh.tris)
                            {
                                // We Keep track of each index for each vertex of each face
                                int[] vertFace = new int[3];
                                int[] texFace = new int[3];
                                // For each vertex in the triangle...
                                for (int i = 0; i < 3; i++)
                                {
                                    // Check whether that vertex is already in our pool of
                                    // vertices
                                    if (indexOfVertex.ContainsKey(tri.p[i] * transform))
                                    {
                                        // If it already exits in the pool, we get the index
                                        // of that vertex and link it to the face
                                        vertFace[i] = indexOfVertex[tri.p[i] * transform];
                                    }
                                    else
                                    {
                                        // If we don't already have that vertex stored, then
                                        // we store it and its index
                                        indexOfVertex[tri.p[i] * transform] = vertIndex;
                                        // We link that index to the face and we
                                        // increment the index for the next new vertex
                                        vertFace[i] = vertIndex++;
                                    }
                                    // Same goes for the texel coordinates
                                    if (indexOfTexel.ContainsKey(tri.t[i]))
                                    {
                                        texFace[i] = indexOfTexel[tri.t[i]];
                                    }
                                    else
                                    {
                                        indexOfTexel[tri.t[i]] = texIndex;
                                        texFace[i] = texIndex++;
                                    }
                                }
                                // Add each index array to our list of index arrays
                                vertFaces.Add(vertFace);
                                texFaces.Add(texFace);
                            }
                            // We repeat the above code for each quadrilateral
                            foreach (Quadrilateral quad in mesh.quads)
                            {
                                int[] vertFace = new int[4];
                                int[] texFace = new int[4];

                                for (int i = 0; i < 4; i++)
                                {
                                    if (indexOfVertex.ContainsKey(quad.p[i] * transform))
                                    {
                                        vertFace[i] = indexOfVertex[quad.p[i] * transform];
                                    }
                                    else
                                    {
                                        indexOfVertex[quad.p[i] * transform] = vertIndex;
                                        vertFace[i] = vertIndex++;
                                    }
                                    if (indexOfTexel.ContainsKey(quad.t[i]))
                                    {
                                        texFace[i] = indexOfTexel[quad.t[i]];
                                    }
                                    else
                                    {
                                        indexOfTexel[quad.t[i]] = texIndex;
                                        texFace[i] = texIndex++;
                                    }
                                }
                                vertFaces.Add(vertFace);
                                texFaces.Add(texFace);
                            }

                            // Output the collection in standard obj format
                            foreach (KeyValuePair<Vec3D, int> vertex in indexOfVertex)
                            {
                                // Explicitly set 6 decimal places
                                string line = $"v {vertex.Key.x:f6} {vertex.Key.y:f6} {vertex.Key.z:f6}";
                                outputFile.WriteLine(line);
                            }
                            if (mesh.material.hasTexture)
                            {
                                foreach (KeyValuePair<Vec2D, int> texel in indexOfTexel)
                                {
                                    string line = $"vt {texel.Key.u:f6} {texel.Key.v:f6}";
                                    outputFile.WriteLine(line);
                                }
                            }
                            // Specify the material name
                            outputFile.WriteLine($"usemtl {mesh.materialName}");

                            // Output the collection in standard obj format
                            if (mesh.material.hasTexture)
                            {
                                for (int i = 0; i < vertFaces.Count; i++)
                                {
                                    if (vertFaces[i].Length == 3)
                                    {
                                        string line = $"f {vertFaces[i][0]}/{texFaces[i][0]} {vertFaces[i][1]}/{texFaces[i][1]} " +
                                            $"{vertFaces[i][2]}/{texFaces[i][2]}";
                                        outputFile.WriteLine(line);

                                    }
                                    else if (vertFaces[i].Length == 4)
                                    {
                                        string line = $"f {vertFaces[i][0]}/{texFaces[i][0]} {vertFaces[i][1]}/{texFaces[i][1]} " +
                                            $"{vertFaces[i][2]}/{texFaces[i][2]} {vertFaces[i][3]}/{texFaces[i][3]}";
                                        outputFile.WriteLine(line);
                                    }
                                }
                            }
                            else
                            {
                                foreach (int[] face in vertFaces)
                                {
                                    if (face.Length == 3)
                                    {
                                        string line = $"f {face[0]} {face[1]} {face[2]}";
                                        outputFile.WriteLine(line);

                                    }
                                    else if (face.Length == 4)
                                    {
                                        string line = $"f {face[0]} {face[1]} {face[2]} {face[3]}";
                                        outputFile.WriteLine(line);
                                    }
                                }
                            }
                        }

                    }

                    // Write the material file
                    using (StreamWriter outputFile = new StreamWriter(mtlPath))
                    {
                        HashSet<string> materialTemplates = new HashSet<string>();
                        foreach (Mesh mesh in Meshes)
                        {
                            // Prevents duplicate material templates from being written
                            if (materialTemplates.Contains(mesh.materialName))
                                continue;
                            outputFile.WriteLine();
                            outputFile.WriteLine($"newmtl {mesh.materialName}");
                            outputFile.WriteLine($"Ns {mesh.material.Ns:f6}");
                            outputFile.WriteLine($"Ka {mesh.material.Ka[0]:f6} {mesh.material.Ka[1]:f6} {mesh.material.Ka[2]:f6}");
                            outputFile.WriteLine($"Ks {mesh.material.Ks[0]:f6} {mesh.material.Ks[1]:f6} {mesh.material.Ks[2]:f6}");
                            outputFile.WriteLine($"Ke {mesh.material.Ke[0]:f6} {mesh.material.Ke[1]:f6} {mesh.material.Ke[2]:f6}");
                            outputFile.WriteLine($"Ni {mesh.material.Ni:f6}");
                            outputFile.WriteLine($"d {mesh.material.d:f6}");
                            outputFile.WriteLine($"illum {mesh.material.illum}");
                            if (mesh.material.hasTexture)
                            {
                                outputFile.WriteLine($"map_Kd {mesh.material.texturePath}");
                                string texturePath = Path.Combine(directoryPath, mesh.material.texturePath);
                                // If the texture does not exist, we save it along side the obj and mtl files
                                if (!File.Exists(texturePath))
                                {
                                    texturePath = Path.Combine(directoryPath, mesh.material.texturePath);
                                    mesh.material.texture.Bitmap.Save(texturePath);
                                }
                            }
                            materialTemplates.Add(mesh.materialName);
                        }

                    }

                    MessageBox.Show("Save complete");
                }
            }
            else
            {
                MessageBox.Show("Nothing to save");
            }
        }

        private void CubeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mesh cube = new Mesh();
            List<Quadrilateral> quads = new List<Quadrilateral>
            {
                // North
                new Quadrilateral(new Vec3D(1,0,1), new Vec3D(1,1,1), new Vec3D(0,1,1), new Vec3D(0,0,1)),
                // South
                new Quadrilateral(new Vec3D(0,0,0), new Vec3D(0,1,0), new Vec3D(1,1,0), new Vec3D(1,0,0)),
                // East
                new Quadrilateral(new Vec3D(1,0,0), new Vec3D(1,1,0), new Vec3D(1,1,1), new Vec3D(1,0,1)),
                // West
                new Quadrilateral(new Vec3D(0,0,1), new Vec3D(0,1,1), new Vec3D(0,1,0), new Vec3D(0,0,0)),
                // Top
                new Quadrilateral(new Vec3D(0,1,0), new Vec3D(0,1,1), new Vec3D(1,1,1), new Vec3D(1,1,0)),
                // Bottom
                new Quadrilateral(new Vec3D(0,0,1), new Vec3D(0,0,0), new Vec3D(1,0,0), new Vec3D(1,0,1)),
            };
            cube.quads = quads;
            cube.name = "Cube" + Meshes.Count;
            cube.materialName = "Cube" + Meshes.Count;
            Meshes.Add(cube);
            ObjectList.Items.Add(cube.name);
        }

        private void CameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetCamera();
        }

        private void WorldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetWorld();
        }

        private void CameraSpeedSlider_ValueChanged(object sender, EventArgs e)
        {
            // Sync Slider and UpDown
            CamSpeedUpDown.Value = CameraSpeedSlider.Value;
        }

        private void CamSpeedUpDown_ValueChanged(object sender, EventArgs e)
        {
            // Sync Slider and UpDown
            CameraSpeedSlider.Value = (int)CamSpeedUpDown.Value;
        }

        // Whenever the numericUpDown is in focus, pressing keys cause
        // an error 'ding' to play. This prevents that from happening
        private void CamSpeedUpDown_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode > Keys.D9 | e.KeyCode < Keys.D0) & e.KeyCode != Keys.Back & e.KeyCode != Keys.Enter & e.KeyCode != Keys.Delete)
            {
                e.SuppressKeyPress = true;
            }
        }

        private void Form1_Click(object sender, EventArgs e)
        {
            ObjectList.ClearSelected();
        }

        private void ViewWindow_Click(object sender, EventArgs e)
        {
            ObjectList.ClearSelected();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = ObjectList.SelectedIndex;
            Meshes.RemoveAt(index);
            ObjectList.Items.RemoveAt(index);
        }


        private void ObjectList_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right & ObjectList.SelectedIndex != -1)
            {
                ContextMenuStrip.Show(Cursor.Position);
            }
        }

        // Note change to enum maybe
        private void Transform(int value, int index, string transformation, int coordinate)
        {
            switch (transformation)
            {
                case "Translation":
                    Meshes[index].Translation[coordinate] = value;
                    break;
                case "Rotation":
                    Meshes[index].Rotation[coordinate] = value;
                    break;
                case "Scale":
                    Meshes[index].Scale[coordinate] = value;
                    break;
            }
        }

        private void UpDownX_ValueChanged(object sender, EventArgs e)
        {

            if (ObjectList.SelectedIndex != -1)
            {
                int index = ObjectList.SelectedIndex;
                Transform((int)UpDownX.Value, ObjectList.SelectedIndex, TransformationBox.Text, 0);
            }
        }

        private void UpDownY_ValueChanged(object sender, EventArgs e)
        {
            if (ObjectList.SelectedIndex != -1)
            {
                int index = ObjectList.SelectedIndex;
                Transform((int)UpDownY.Value, ObjectList.SelectedIndex, TransformationBox.Text, 1);
            }
        }

        private void UpDownZ_ValueChanged(object sender, EventArgs e)
        {
            if (ObjectList.SelectedIndex != -1)
            {
                int index = ObjectList.SelectedIndex;
                Transform((int)UpDownZ.Value, ObjectList.SelectedIndex, TransformationBox.Text, 2);
            }
        }

        private void ObjectList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ObjectList.SelectedIndex != -1)
            {
                int index = ObjectList.SelectedIndex;
                switch (TransformationBox.Text)
                {
                    case "Translation":
                        UpDownX.Value = Meshes[index].Translation[0];
                        UpDownY.Value = Meshes[index].Translation[1];
                        UpDownZ.Value = Meshes[index].Translation[2];
                        break;
                    case "Rotation":
                        UpDownX.Value = Meshes[index].Rotation[0];
                        UpDownY.Value = Meshes[index].Rotation[1];
                        UpDownZ.Value = Meshes[index].Rotation[2];
                        break;
                    case "Scale":
                        UpDownX.Value = Meshes[index].Scale[0];
                        UpDownY.Value = Meshes[index].Scale[1];
                        UpDownZ.Value = Meshes[index].Scale[2];
                        break;
                }
            }
        }

        private void TransformationBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ObjectList.SelectedIndex != -1)
            {
                int index = ObjectList.SelectedIndex;
                switch (TransformationBox.Text)
                {
                    case "Translation":
                        UpDownX.Value = Meshes[index].Translation[0];
                        UpDownY.Value = Meshes[index].Translation[1];
                        UpDownZ.Value = Meshes[index].Translation[2];
                        break;
                    case "Rotation":
                        UpDownX.Value = Meshes[index].Rotation[0];
                        UpDownY.Value = Meshes[index].Rotation[1];
                        UpDownZ.Value = Meshes[index].Rotation[2];
                        break;
                    case "Scale":
                        UpDownX.Value = Meshes[index].Scale[0];
                        UpDownY.Value = Meshes[index].Scale[1];
                        UpDownZ.Value = Meshes[index].Scale[2];
                        break;
                }
            }
        }

        private void UpDownX_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode > Keys.D9 | e.KeyCode < Keys.D0) & e.KeyCode != Keys.Back & e.KeyCode != Keys.Enter & e.KeyCode != Keys.Delete & e.KeyCode != Keys.OemMinus)
            {
                e.SuppressKeyPress = true;
            }
        }

        private void UpDownY_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode > Keys.D9 | e.KeyCode < Keys.D0) & e.KeyCode != Keys.Back & e.KeyCode != Keys.Enter & e.KeyCode != Keys.Delete & e.KeyCode != Keys.OemMinus)
            {
                e.SuppressKeyPress = true;
            }
        }

        private void UpDownZ_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode > Keys.D9 | e.KeyCode < Keys.D0) & e.KeyCode != Keys.Back & e.KeyCode != Keys.Enter & e.KeyCode != Keys.Delete & e.KeyCode != Keys.OemMinus)
            {
                e.SuppressKeyPress = true;
            }
        }
    }
}
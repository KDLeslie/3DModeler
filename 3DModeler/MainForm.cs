using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using static _3DModeler.Operations;

namespace _3DModeler
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            Stopwatch = new Stopwatch();
            // Initialize the class and components pertaining to the main view
            // into the world. Increasing pixel width/height improves performance
            // a little at the cost of a lot of visual quality
            MainView = new Viewport(ViewWindow.Width, ViewWindow.Height, 1, 1);
            Meshes = new List<Mesh>();
            // Rendering frames needs these keys to be initialized in the dictionary
            // by default
            KeyPressed = new Dictionary<Keys, bool>()
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
            Stopwatch.Start();
            // How often the tick event is run
            Clock.Interval = 20;
            Clock.Enabled = true;
            // Setup Projection Matrix
            MainView.ProjMat = MakeProjection(90, (float)MainView.ScreenHeight / (float)MainView.ScreenWidth, 0.1f, 1000.0f);
        }
        Stopwatch Stopwatch; // Stores the total time from start
        Viewport MainView; // The interface that displays the 3D graphics
        int FrameCount = 0; // Counts how many frames were rendered. Reset each second
        int FPS = 0; // Stores the current frame rate of the viewport
        float FrameTime = 0; // Stores the cumulative time between frames         
        float Tick = 0; // A time variable storing the point in time right before rendering the current frame
        float Tock = 0; // A time variable storing the point in time right before rendering the last frame
        float ElapsedTime = 0;  // Stores the time between each frame in seconds
        List<Mesh> Meshes; // Stores each mesh loaded from an obj file
        PointF LastCursorPosition;
        bool MousePressed = false;
        Dictionary<Keys, bool> KeyPressed; // Stores what keys the user is currently pressing

        // A structure used for storing face information for a 3 vertex polygon
        struct Triangle
        {
            public Triangle() { }
            public Triangle(Triangle tri)
            {
                for (int i = 0; i < 3; i++)
                {
                    this.p[i] = new Vec3D(tri.p[i]);
                    this.t[i] = new Vec2D(tri.t[i]);
                }
                this.lum = tri.lum;
                this.normal = new Vec3D(tri.normal);
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
            public float lum = 0; // 0-1 luminosity value for the triangle based on a light source
            public Vec3D normal = new Vec3D(); // Normal vector of the triangle
            // Small hacks used to draw the wireframe of a quadrilateral
            // correctly. Eventually will also be used to fix the wireframe
            // of clipped triangles
            public bool drawSide0_1 = true;
            public bool drawSide1_2 = true;
            public bool drawSide2_0 = true;
        }

        // A structure used for storing face information for a 4 vertex polygon
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

            public Vec3D[] p = new Vec3D[4];
            public Vec2D[] t = new Vec2D[4];
            public float lum = 0;
            public Vec3D normal = new Vec3D();
        }

        // A structure used for storing information for a 3D object
        struct Mesh
        {
            public Mesh() { }
            public string name = "DefaultMesh"; // Used to identify different meshes
            public string materialName = "DefaultMaterial"; // Used to identify which material the mesh uses in the mtl file
            public List<Triangle> tris = new List<Triangle>();
            public List<Quadrilateral> quads = new List<Quadrilateral>();
            public Material material = new Material(); // Stores the mesh's material data
            public float[] translation = { 0, 0, 0 }; // object's translation w.r.t origin
            public float[] rotation = { 0, 0, 0 }; // object's rotation (degrees) w.r.t origin 
            public float[] scale = { 1, 1, 1 }; // object's scale w.r.t origin
        }

        // A structure containing the material information for each mesh.
        // Currently only used for texture information
        struct Material
        {
            public Material() { }
            public float ns = 250;
            public float[] ka = { 1, 1, 1 };
            public float[] kd = { 0, 0, 0 };
            public float[] ks = { 0, 0, 0 };
            public float[] ke = { 0, 0, 0 };
            public float ni = 1.45f;
            public float d = 1;
            public int illum = 2;
            public bool hasTexture = false;
            public string texturePath = "";
            public DirectBitmap texture = new DirectBitmap();
        }
        // A class containing the information pertaining to a certain view into the world
        class Viewport
        {
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
            public Vec3D LookDirection = new Vec3D(); // Direction the camera looks along
            public float Pitch { get; set; } // Camera rotation about the x-axis
            public float Yaw { get; set; } // Camera rotation about the y-axis
            public float[] Thetas { get; set; } = { 0, 0, 0 }; // World rotation around x,y, and z axes (only changes the view)
            public int ScreenWidth { get; set; }
            public int ScreenHeight { get; set; }
            public int PixelWidth { get; set; }
            public int PixelHeight { get; set; }
            public float[] DepthBuffer { get; set; } = new float[0]; // Stores the z-depth of each screen pixel
            public DirectBitmap Frame { get; set; } // A bitmap of the frame drawn to the viewport

            // Takes in a plane, its normal, and a triangle to be clipped against the plane. It
            // creates 0-2 new triangles based on how the triangle intersects the plane, passed
            // out through reference parameters. Returns the number of new triangles created.
            // (Clipping currently breaks wireframe)
            public int Triangle_ClipAgainstPlane(Vec3D plane_p, Vec3D plane_n, Triangle in_tri, ref Triangle out_tri1, ref Triangle out_tri2)
            {
                // Make sure plane normal is indeed normal
                plane_n = Normalize(ref plane_n);

                // Return signed shortest distance from point to plane, plane normal must be normalized
                Func<Vec3D, float> dist = (Vec3D p) =>
                {
                    return (DotProduct(ref plane_n, ref p) - DotProduct(ref plane_n, ref plane_p));
                };

                // Create two temporary storage arrays to classify points on either side of the plane
                // If the distance sign is positive, point lies on the "inside" of the plane
                Vec3D[] inside_points = { new Vec3D(), new Vec3D(), new Vec3D() };
                int nInsidePointCount = 0;
                Vec3D[] outside_points = { new Vec3D(), new Vec3D(), new Vec3D() };
                int nOutsidePointCount = 0;
                Vec2D[] inside_tex = { new Vec2D(), new Vec2D(), new Vec2D() };
                int nInsideTexCount = 0;
                Vec2D[] outside_tex = { new Vec2D(), new Vec2D(), new Vec2D() };
                int nOutsideTexCount = 0;

                // Get signed distance of each point in triangle to plane
                float d0 = dist(in_tri.p[0]);
                float d1 = dist(in_tri.p[1]);
                float d2 = dist(in_tri.p[2]);

                // Positive distance means the point is in the direction of the plane normal
                // i.e. it is an inside point
                if (d0 >= 0)
                {
                    inside_points[nInsidePointCount++] = in_tri.p[0];
                    inside_tex[nInsideTexCount++] = in_tri.t[0];
                }
                // Otherwise, it is an outside point
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
                    out_tri1.p[1] = IntersectPlane(ref plane_p, ref plane_n, ref inside_points[0], ref outside_points[0], ref t);
                    out_tri1.t[1].u = t * (outside_tex[0].u - inside_tex[0].u) + inside_tex[0].u;
                    out_tri1.t[1].v = t * (outside_tex[0].v - inside_tex[0].v) + inside_tex[0].v;
                    out_tri1.t[1].w = t * (outside_tex[0].w - inside_tex[0].w) + inside_tex[0].w;

                    out_tri1.p[2] = IntersectPlane(ref plane_p, ref plane_n, ref inside_points[0], ref outside_points[1], ref t);
                    out_tri1.t[2].u = t * (outside_tex[1].u - inside_tex[0].u) + inside_tex[0].u;
                    out_tri1.t[2].v = t * (outside_tex[1].v - inside_tex[0].v) + inside_tex[0].v;
                    out_tri1.t[2].w = t * (outside_tex[1].w - inside_tex[0].w) + inside_tex[0].w;

                    return 1; // Return the newly formed single triangle
                }
                else if (nInsidePointCount == 2 && nOutsidePointCount == 1)
                {
                    // Triangle should be clipped. As two points lie inside the plane,
                    // the clipped triangle becomes a quad. Fortunately, we can
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
                    out_tri1.p[2] = IntersectPlane(ref plane_p, ref plane_n, ref inside_points[0], ref outside_points[0], ref t);
                    out_tri1.t[2].u = t * (outside_tex[0].u - inside_tex[0].u) + inside_tex[0].u;
                    out_tri1.t[2].v = t * (outside_tex[0].v - inside_tex[0].v) + inside_tex[0].v;
                    out_tri1.t[2].w = t * (outside_tex[0].w - inside_tex[0].w) + inside_tex[0].w;

                    // The second triangle is composed of one of the inside points, a
                    // new point determined by the intersection of the other side of the 
                    // triangle and the plane, and the newly created point above
                    out_tri2.p[0] = inside_points[1];
                    out_tri2.t[0] = inside_tex[1];
                    out_tri2.p[1] = out_tri1.p[2];
                    out_tri2.t[1] = out_tri1.t[2];
                    out_tri2.p[2] = IntersectPlane(ref plane_p, ref plane_n, ref inside_points[1], ref outside_points[0], ref t);
                    out_tri2.t[2].u = t * (outside_tex[0].u - inside_tex[1].u) + inside_tex[1].u;
                    out_tri2.t[2].v = t * (outside_tex[0].v - inside_tex[1].v) + inside_tex[1].v;
                    out_tri2.t[2].w = t * (outside_tex[0].w - inside_tex[1].w) + inside_tex[1].w;

                    return 2; // Return two newly formed triangles
                }
                else
                {
                    return 0;
                }
            }
            // Prepares each triangle for drawing. Requires the world, view, and triangle
            // transform matrices, the triangle, and bool of whether to cull triangles.
            // The triangle is then added to the list passed in as an argument.
            public void PrepareForRasterization(Triangle tri, Mat4x4 worldMat, Mat4x4 viewMat, Mat4x4 triMat, List<Triangle> trianglesToRaster,
                bool culling)
            {
                Triangle triTransformed = new Triangle(tri);
                // Transform the triangle
                triTransformed.p[0] = tri.p[0] * worldMat * triMat;
                triTransformed.p[1] = tri.p[1] * worldMat * triMat;
                triTransformed.p[2] = tri.p[2] * worldMat * triMat;

                // Calculate triangle's Normal 
                Vec3D normal, line1, line2;

                // Get lines on either side of triangle
                line1 = triTransformed.p[1] - triTransformed.p[0];
                line2 = triTransformed.p[2] - triTransformed.p[0];

                // Take the cross product of lines to get normal to triangle's surface
                normal = CrossProduct(ref line1, ref line2);
                normal = Normalize(ref normal);

                // Get Ray from camera to triangle
                Vec3D vCameraRay = triTransformed.p[0] - CameraPosition;

                // Cull triangles with normals pointing away from the camera.
                // If culling is disabled we draw the triangles regardless. 
                if (DotProduct(ref normal, ref vCameraRay) < 0.0f | !culling)
                {
                    // Illumination
                    Vec3D light_direction = new Vec3D(0.0f, -1.0f, 1.0f);
                    light_direction = Normalize(ref light_direction);
                    // The less the triangle normal and the light direction are aligned
                    // the dimmer the triangle. Normal and light vectors are in opposite
                    // directions so we negate the dot product
                    float lum = MathF.Max(0.1f, -DotProduct(ref light_direction, ref normal));
                    triTransformed.lum = lum;

                    // Convert World Space --> View Space
                    Triangle triViewed = new Triangle(triTransformed);
                    triViewed.p[0] *= viewMat;
                    triViewed.p[1] *= viewMat;
                    triViewed.p[2] *= viewMat;

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
                        // View space -> Screen space
                        Triangle triProjected = new Triangle(clipped[n]);
                        triProjected.p[0] *= ProjMat;
                        triProjected.p[1] *= ProjMat;
                        triProjected.p[2] *= ProjMat;

                        // Divide the texture coordinates by z-component to add perspective
                        triProjected.t[0].u /= triProjected.p[0].w;
                        triProjected.t[1].u /= triProjected.p[1].w;
                        triProjected.t[2].u /= triProjected.p[2].w;

                        triProjected.t[0].v /= triProjected.p[0].w;
                        triProjected.t[1].v /= triProjected.p[1].w;
                        triProjected.t[2].v /= triProjected.p[2].w;

                        // Set texel depth to be reciprocal so we can get the un-normalized
                        // coordinates back
                        triProjected.t[0].w = 1.0f / triProjected.p[0].w;
                        triProjected.t[1].w = 1.0f / triProjected.p[1].w;
                        triProjected.t[2].w = 1.0f / triProjected.p[2].w;

                        // Each vertex is divided by the z-component to add perspective
                        triProjected.p[0] /= triProjected.p[0].w;
                        triProjected.p[1] /= triProjected.p[1].w;
                        triProjected.p[2] /= triProjected.p[2].w;

                        // We must invert x because our system uses a left-hand coordinate system	
                        triProjected.p[0].x *= -1.0f;
                        triProjected.p[1].x *= -1.0f;
                        triProjected.p[2].x *= -1.0f;
                        // We must invert y because pixels are drawn top-down
                        triProjected.p[0].y *= -1.0f;
                        triProjected.p[1].y *= -1.0f;
                        triProjected.p[2].y *= -1.0f;

                        // Projection Matrix gives results between -1 and +1 via dividing by Z.
                        // Offset vertices to be between 0 and 2
                        Vec3D vOffsetView = new Vec3D(1, 1, 0);
                        triProjected.p[0] += vOffsetView;
                        triProjected.p[1] += vOffsetView;
                        triProjected.p[2] += vOffsetView;

                        // Scale vertices to occupy the screen
                        triProjected.p[0].x *= 0.5f * ScreenWidth;
                        triProjected.p[0].y *= 0.5f * ScreenHeight;
                        triProjected.p[1].x *= 0.5f * ScreenWidth;
                        triProjected.p[1].y *= 0.5f * ScreenHeight;
                        triProjected.p[2].x *= 0.5f * ScreenWidth;
                        triProjected.p[2].y *= 0.5f * ScreenHeight;

                        // Store triangle for rasterizing
                        trianglesToRaster.Add(triProjected);
                    }
                }
            }

            // Converts a triangle's luminance to grayscale
            public Color GetShade(float lum)
            {
                return Color.FromArgb(255, (int)(lum * 255), (int)(lum * 255), (int)(lum * 255));
            }

            // Converts a triangle's luminance to a shade of yellow
            public Color GetYellowShade(float lum)
            {
                return Color.FromArgb(255, (int)(lum * 255), (int)(lum * 255), 0);
            }

            // Draws a triangle to a bitmap that represents the current frame.
            // Depending on whether the user has toggled shading or texturing, or
            // whether the triangle is part of a mesh that's been selected, the color
            // information of each triangle will be used accordingly.
            public void DrawTriangle(Triangle tri, DirectBitmap texture, bool texturing = false, bool shading = true, bool isSelected = false)
            {

                // Capture relevant triangle information
                // The pixel domain is integers. Can't move half a pixel
                int[] xs = { (int)tri.p[0].x, (int)tri.p[1].x, (int)tri.p[2].x };
                int[] ys = { (int)tri.p[0].y, (int)tri.p[1].y, (int)tri.p[2].y };
                float[] us = { tri.t[0].u, tri.t[1].u, tri.t[2].u };
                float[] vs = { tri.t[0].v, tri.t[1].v, tri.t[2].v };
                float[] ws = { tri.t[0].w, tri.t[1].w, tri.t[2].w };
                float lum = tri.lum;

                // Swaps the variables so that y[0] < y[1] < y[2]
                // Lower y value = higher up on screen
                if (ys[1] < ys[0])
                {
                    (ys[0], ys[1]) = (ys[1], ys[0]);
                    (xs[0], xs[1]) = (xs[1], xs[0]);
                    (us[0], us[1]) = (us[1], us[0]);
                    (vs[0], vs[1]) = (vs[1], vs[0]);
                    (ws[0], ws[1]) = (ws[1], ws[0]);
                }

                if (ys[2] < ys[0])
                {
                    (ys[0], ys[2]) = (ys[2], ys[0]);
                    (xs[0], xs[2]) = (xs[2], xs[0]);
                    (us[0], us[2]) = (us[2], us[0]);
                    (vs[0], vs[2]) = (vs[2], vs[0]);
                    (ws[0], ws[2]) = (ws[2], ws[0]);
                }

                if (ys[2] < ys[1])
                {
                    (ys[1], ys[2]) = (ys[2], ys[1]);
                    (xs[1], xs[2]) = (xs[2], xs[1]);
                    (us[1], us[2]) = (us[2], us[1]);
                    (vs[1], vs[2]) = (vs[2], vs[1]);
                    (ws[1], ws[2]) = (ws[2], ws[1]);
                }

                // We draw half the triangle each time, first the
                // top half, then the bottom half
                for (int p = 0; p < 2; p++)
                {
                    // Variables relating to one side of the triangle.
                    // Side changes depending on whether we are
                    // drawing the top or bottom half
                    int dy1 = ys[p + 1] - ys[p];
                    int dx1 = xs[p + 1] - xs[p];
                    float dv1 = vs[p + 1] - vs[p];
                    float du1 = us[p + 1] - us[p];
                    float dw1 = ws[p + 1] - ws[p];

                    // Variables relating to the steepest side of the triangle.
                    // Steepest side stays the same for both top and bottom halves
                    // of the triangle
                    int dy2 = ys[2] - ys[0];
                    int dx2 = xs[2] - xs[0];
                    float dv2 = vs[2] - vs[0];
                    float du2 = us[2] - us[0];
                    float dw2 = ws[2] - ws[0];

                    // Texel coordinates per pixel
                    float tex_u, tex_v, tex_w;

                    // Holds how much we step in each coordinate space per row of pixels
                    float dax_step = 0, dbx_step = 0,
                        du1_step = 0, dv1_step = 0,
                        du2_step = 0, dv2_step = 0,
                        dw1_step = 0, dw2_step = 0;

                    if (dy1 != 0)
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

                    // As long as this side isn't flat, draw first/second half of the triangle
                    if (dy1 != 0)
                    {
                        // For each row of pixels...
                        for (int i = ys[p]; i <= ys[p + 1]; i++)
                        {
                            // Get start and end pixels for the row
                            int ax = (int)(xs[p] + (i - ys[p]) * dax_step);
                            int bx = (int)(xs[0] + (i - ys[0]) * dbx_step);

                            // Get start and end texels for the row
                            float tex_su = us[p] + (i - ys[p]) * du1_step;
                            float tex_sv = vs[p] + (i - ys[p]) * dv1_step;
                            float tex_sw = ws[p] + (i - ys[p]) * dw1_step;

                            float tex_eu = us[0] + (i - ys[0]) * du2_step;
                            float tex_ev = vs[0] + (i - ys[0]) * dv2_step;
                            float tex_ew = ws[0] + (i - ys[0]) * dw2_step;

                            // Ensure we go from left to right in pixel space and flip any
                            // correlated components if necessary
                            if (ax > bx)
                            {
                                (ax, bx) = (bx, ax);
                                (tex_su, tex_eu) = (tex_eu, tex_su);
                                (tex_sv, tex_ev) = (tex_ev, tex_sv);
                                (tex_sw, tex_ew) = (tex_ew, tex_sw);
                            }

                            // Stores interpolation along the line between start and end points
                            float t = 0.0f;
                            float tstep = 1.0f / (bx - ax);

                            // For each pixel in the row...
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
                                    // Divide by tex_w to denormalize texel coordinates
                                    float u = (tex_u / tex_w);
                                    // The v-axis in texel space is inverted as opposed to the y-axis in bitmaps.
                                    // 1 - v undoes the inversion
                                    float v = (1 - tex_v / tex_w);
                                    // uv coordinates are between 0 and 1. Anything outside those values will
                                    // be wrapped via repetition 
                                    u = u >= 0 ? u % 1 : (u % 1 + 1.0f) % 1;
                                    v = v >= 0 ? v % 1 : (v % 1 + 1.0f) % 1;
                                    // Scale up texel coordinates to the height and width of the textures
                                    int w = (int)(u * texture.Width);
                                    int h = (int)(v * texture.Height);
                                    // If the triangle is part of a mesh that is currently selected, we 
                                    // colour it yellow to indicate that it's selected
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
                                    // If texturing is enabled...
                                    else if (texturing)
                                    {
                                        // If shading is enabled
                                        if (shading)
                                        {
                                            // Get the texel via the u,v-coordinates
                                            Color color = texture.GetPixel(w, h);
                                            // Perform basic shading implementation until mtl processing is added
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
                while (true)
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
        class DirectBitmap : IDisposable
        {
            public DirectBitmap()
            {

            }
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
                // Get a handle to the pixel data
                BitsHandle = GCHandle.Alloc(Pixels, GCHandleType.Pinned);
                // Create a new bitmap that uses the array for its pixel information
                Bitmap = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppArgb, BitsHandle.AddrOfPinnedObject());
                // Dispose of the temporary bitmap
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

            public Bitmap Bitmap { get; private set; }
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
        // no objects could be loaded. TODO: Add support for obj files that
        // contain normal information
        private bool LoadObjectsFromFile(string filename, ref string materialFilename)
        {
            // Assume no material template library file
            // unless one is given in the file
            materialFilename = "";

            Mesh mesh = new Mesh();

            try
            {
                // Create an instance of StreamReader to read from a file.
                // The 'using' statement also closes the StreamReader
                using (StreamReader sr = new StreamReader(filename))
                {
                    // Store the vertex and texel coordinates in parallel lists.
                    List<Vec3D> verts = new List<Vec3D>();
                    List<Vec2D> texs = new List<Vec2D>();
                    string? line;
                    // Read lines from the file until the end of the file is reached
                    while ((line = sr.ReadLine()) != null)
                    {
                        // Catch any empty lines
                        if (line.Length == 0)
                            continue;

                        // If the line begins with 'm' it specifies the mtl library template file
                        if (line[0] == 'm')
                        {
                            // Split the line via spaces. The first string specifies the
                            // type of line.
                            string[] name = line.Split(' ');
                            // Sometimes the file name has spaces in it so rebuild the original
                            // line without the first string
                            materialFilename = string.Join(" ", name.Skip(1).ToArray());
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
                        // via indices into each list
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
                                material.ns = float.Parse(ns[1]);
                            }
                            if (line[1] == 'i')
                            {
                                string[] ni = line.Split(' ');
                                material.ni = float.Parse(ni[1]);
                            }
                        }
                        else if (line[0] == 'K')
                        {
                            if (line[1] == 'a')
                            {
                                string[] ka = line.Split(' ');
                                material.ka[0] = float.Parse(ka[1]);
                                material.ka[1] = float.Parse(ka[2]);
                                material.ka[2] = float.Parse(ka[3]);
                            }
                            else if (line[1] == 'd')
                            {
                                string[] kd = line.Split(' ');
                                material.kd[0] = float.Parse(kd[1]);
                                material.kd[1] = float.Parse(kd[2]);
                                material.kd[2] = float.Parse(kd[3]);
                            }
                            else if (line[1] == 's')
                            {
                                string[] ks = line.Split(' ');
                                material.ks[0] = float.Parse(ks[1]);
                                material.ks[1] = float.Parse(ks[2]);
                                material.ks[2] = float.Parse(ks[3]);
                            }
                            else if (line[1] == 'e')
                            {
                                string[] ke = line.Split(' ');
                                material.ke[0] = float.Parse(ke[1]);
                                material.ke[1] = float.Parse(ke[2]);
                                material.ke[2] = float.Parse(ke[3]);
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
                        // 'm' specifies the material's texture path
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
                                    MessageBox.Show($"Could not find texture file '{material.texturePath}'", "File Load Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            MainView.Thetas[0] = 0;
            MainView.Thetas[1] = 0;
            MainView.Thetas[2] = 0;
            // CameraSpeedSlider.Value = 8;
        }

        // Returns the world to a default state
        private void ResetWorld()
        {
            Meshes.Clear();
            ObjectList.Items.Clear();
        }

        // Gets a transformation matrix to transform each tri according to its own
        // specific transformation data. Unlike world transforms, these have permanent
        // affects on the triangle
        private Mat4x4 GetTriTransformationMatrix(Mesh mesh)
        {

            Mat4x4 matTranslate = MakeTranslation(mesh.translation[0], mesh.translation[1], mesh.translation[2]);
            Mat4x4 matScale = MakeScale(mesh.scale[0], mesh.scale[1], mesh.scale[2]);
            Mat4x4 matRotate = MakeRotation(mesh.rotation[0], degrees: true) * MakeRotation(y: mesh.rotation[1], degrees: true)
                * MakeRotation(z: mesh.rotation[2], degrees: true);
            return matScale * matRotate * matTranslate;
        }

        // Transforms a mesh in the object list. Requires the magnitude of the
        // transform, which transform it is, the index into the list of the
        // mesh, and which coordinate the transform is being applied to
        private void TransformObject(float magnitude, string transformation, int index, int coordinate)
        {
            if (index != -1)
            {
                switch (transformation)
                {
                    case "Translation":
                        Meshes[index].translation[coordinate] = magnitude;
                        break;
                    case "Rotation":
                        Meshes[index].rotation[coordinate] = magnitude;
                        break;
                    case "Scale":
                        Meshes[index].scale[coordinate] = magnitude;
                        break;
                }
            }

        }

        private void UpdateUpDowns()
        {
            if (ObjectList.SelectedIndex != -1)
            {
                int index = ObjectList.SelectedIndex;
                switch (TransformationBox.Text)
                {
                    case "Translation":
                        UpDownX.Value = (decimal)Meshes[index].translation[0];
                        UpDownY.Value = (decimal)Meshes[index].translation[1];
                        UpDownZ.Value = (decimal)Meshes[index].translation[2];
                        break;
                    case "Rotation":
                        UpDownX.Value = (decimal)Meshes[index].rotation[0];
                        UpDownY.Value = (decimal)Meshes[index].rotation[1];
                        UpDownZ.Value = (decimal)Meshes[index].rotation[2];
                        break;
                    case "Scale":
                        UpDownX.Value = (decimal)Meshes[index].scale[0];
                        UpDownY.Value = (decimal)Meshes[index].scale[1];
                        UpDownZ.Value = (decimal)Meshes[index].scale[2];
                        break;
                }
            }
        }

        // Returns whether or not the key press should be registered
        private bool IsKeyIllegal(Keys key)
        {
            if ((key > Keys.D9 | key < Keys.D0) & key != Keys.Back & key != Keys.Enter & key != Keys.Delete & key != Keys.OemMinus & key != Keys.OemPeriod)
                return true;
            else
                return false;
        }

        // Called each time the clock passes the time interval
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

            if (KeyPressed[Keys.O])
                // Travel along positive y-axis
                // Will be replaced with always moving upwards (no matter camera orientation) later
                MainView.CameraPosition.y += speed * ElapsedTime;
            if (KeyPressed[Keys.L])
                // Travel along negative y-axis
                // Will be replaced with always moving downwards
                MainView.CameraPosition.y -= speed * ElapsedTime;

            if (KeyPressed[Keys.K])
                // Travel along positive x-axis
                // Will be replaced with always moving rightwards
                MainView.CameraPosition.x += speed * ElapsedTime;
            if (KeyPressed[Keys.OemSemicolon])
                // Travel along negative x-axis
                // Will be replaced with always moving leftwards
                MainView.CameraPosition.x -= speed * ElapsedTime;


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


            // Set up "World Transform". These transformations only affect the
            // way in which the models are seen in the program. They do not
            // permanently change the meshes
            Mat4x4 worldMat = MakeIdentity();
            Mat4x4 worldRotMat = MakeRotation(MainView.Thetas[0], MainView.Thetas[1], MainView.Thetas[2]); // Rotates the world
            Mat4x4 worldScaleMat = MakeScale(1, 1, 1); // Scales the world
            Mat4x4 worldTransMat = MakeTranslation(0.0f, 0.0f, 0.0f);  // Offsets the world

            // Apply transformations in correct order
            worldMat *= worldScaleMat; // Transform by scaling
            worldMat *= worldRotMat; // Transform by rotation
            worldMat *= worldTransMat; // Transform by translation

            // Create "Point At" Matrix for camera
            Vec3D vUp = new Vec3D(0, 1, 0); // Default up direction for camera is along the positive y-axis
            Vec3D vForwardCam = new Vec3D(0, 0, 1); // Default forward direction for camera is along the positive z-axis

            // Cap pitch from being able to rotate too far.
            // TODO: Fix apparent stutter when looking directly up/down
            MainView.Pitch = Math.Clamp(MainView.Pitch, -3.1415f / 2, 3.1415f / 2);

            Mat4x4 cameraRotMat = MakeRotation(MainView.Pitch, MainView.Yaw);
            // Rotated forward vector becomes the camera's look direction
            MainView.LookDirection = vForwardCam * cameraRotMat;
            // Offset the look direction to the camera location to get the target the camera points at
            Vec3D vTarget = MainView.CameraPosition + MainView.LookDirection;
            // Construct the "Point At" matrix
            Mat4x4 matCamera = MakePointAt(ref MainView.CameraPosition, ref vTarget, ref vUp);
            // Construct the "Look At" matrix from the "Point At" matrix inverse
            Mat4x4 viewMat = QuickInverse(ref matCamera);


            // Dispose of the previous frame
            if (MainView.Frame != null)
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

                // Get triangle transformation matrix. Unlike the world transformation
                // matrix, this matrix applies induvial transforms for each triangle 
                Mat4x4 triMat = GetTriTransformationMatrix(mesh);


                bool isSelected = false;
                // Check whether the user selected this mesh or not
                if (ObjectList.SelectedIndex == i)
                {
                    isSelected = true;
                }
                // Store triangles for rasterization later
                List<Triangle> trianglesToRaster = new List<Triangle>();

                // Prepare each triangle for drawing
                foreach (Triangle tri in mesh.tris)
                {
                    MainView.PrepareForRasterization(tri, worldMat, viewMat, triMat, trianglesToRaster, CullingToolStripMenuItem.Checked);
                }
                // Split each quad into two tris and prepare each for drawing.
                // To keep the quad from looking like two tris in wireframe mode, a
                // hack to keep specific sides from being drawn is used
                foreach (Quadrilateral quad in mesh.quads)
                {
                    Triangle tri1 = new Triangle(quad.p[0], quad.p[1], quad.p[2], quad.t[0], quad.t[1], quad.t[2]);
                    tri1.drawSide2_0 = false;
                    MainView.PrepareForRasterization(tri1, worldMat, viewMat, triMat, trianglesToRaster, CullingToolStripMenuItem.Checked);
                    Triangle tri2 = new Triangle(quad.p[0], quad.p[2], quad.p[3], quad.t[0], quad.t[2], quad.t[3]);
                    tri2.drawSide0_1 = false;
                    MainView.PrepareForRasterization(tri2, worldMat, viewMat, triMat, trianglesToRaster, CullingToolStripMenuItem.Checked);
                }

                // Sort triangles from back to front through approximating
                // the triangles' z positions. Useful for transparency. Currently
                // used as a hack to prevent wireframe from being seen through solid
                // objects (Does not currently work if there are multiple meshes)
                if (SolidToolStripMenuItem.Checked & WireframeToolStripMenuItem.Checked)
                {
                    trianglesToRaster.Sort((Triangle t1, Triangle t2) =>
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
                foreach (Triangle triToRaster in trianglesToRaster)
                {
                    // Clip triangles against all four screen edges, this could yield
                    // a bunch of triangles, so create a queue that we traverse to 
                    // ensure we only test new triangles generated against planes
                    Queue<Triangle> listTriangles = new Queue<Triangle>();

                    // Add initial triangle
                    listTriangles.Enqueue(triToRaster);
                    int nNewTriangles = 1;

                    // For each plane...
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
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            KeyPressed[e.KeyCode] = true;
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            KeyPressed[e.KeyCode] = false;
        }

        // Sets the current state of the mouse
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
                MainView.Thetas[1] -= (Cursor.Position.X - LastCursorPosition.X) * 0.005f;
                MainView.Thetas[0] -= (Cursor.Position.Y - LastCursorPosition.Y) * 0.005f;
                LastCursorPosition = Cursor.Position;
            }
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            // Updates the viewport with the form
            if (MainView != null)
            {
                MainView.ScreenWidth = ViewWindow.Width / MainView.PixelWidth;
                MainView.ScreenHeight = ViewWindow.Height / MainView.PixelHeight;
                MainView.DepthBuffer = new float[MainView.ScreenWidth * MainView.ScreenHeight];
                MainView.ProjMat = MakeProjection(90, (float)MainView.ScreenHeight / (float)MainView.ScreenWidth, 0.1f, 1000.0f);
            }

        }

        // Opens obj files
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
                    // If the obj file was read successfully...
                    if (LoadObjectsFromFile(filePath, ref materialName))
                    {
                        // If the obj file references a material file... 
                        if (materialName != "")
                        {
                            string materialPath = Path.Combine(folderPath, materialName);
                            Dictionary<string, Material> materialMap = new Dictionary<string, Material>();
                            // If the mtl file was read successfully...
                            if (LoadMaterialsFromFile(materialPath, folderPath, materialMap))
                            {
                                // For each mesh, we find and link to it its material template
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

        // Obj file saving
        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Can't save anything if there are no meshes
            if (Meshes.Count != 0)
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    // Setup OpenFileDialog properties
                    saveFileDialog.Filter = "obj files (*.obj)|*.obj";
                    saveFileDialog.FilterIndex = 2;
                    saveFileDialog.RestoreDirectory = false;

                    // If a valid filename was chosen...
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Initialize various path names
                        string objPath = saveFileDialog.FileName;
                        string directoryPath = Path.GetDirectoryName(objPath);
                        string chosenName = Path.GetFileNameWithoutExtension(objPath);
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
                                // Get matrix to transform each vertex in the mesh
                                // according to the transformations of that mesh
                                Mat4x4 transform = GetTriTransformationMatrix(mesh);

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
                                        // Apply Transformations
                                        Vec3D transformedPoint = tri.p[i] * transform;
                                        // Check whether that vertex is already in our pool of
                                        // vertices
                                        if (indexOfVertex.ContainsKey(transformedPoint))
                                        {
                                            // If it already exits in the pool, we get the index
                                            // of that vertex and link it to the face
                                            vertFace[i] = indexOfVertex[transformedPoint];
                                        }
                                        else
                                        {
                                            // If we don't already have that vertex stored, then
                                            // we store it and its index
                                            indexOfVertex[transformedPoint] = vertIndex;
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
                                        Vec3D transformedPoint = quad.p[i] * transform;
                                        if (indexOfVertex.ContainsKey(transformedPoint))
                                        {
                                            vertFace[i] = indexOfVertex[transformedPoint];
                                        }
                                        else
                                        {
                                            indexOfVertex[transformedPoint] = vertIndex;
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

                        // Writes the material file
                        using (StreamWriter outputFile = new StreamWriter(mtlPath))
                        {
                            // Prevents duplicate material templates from being written
                            HashSet<string> materialTemplates = new HashSet<string>();
                            foreach (Mesh mesh in Meshes)
                            {
                                if (materialTemplates.Contains(mesh.materialName))
                                    continue;
                                outputFile.WriteLine();
                                outputFile.WriteLine($"newmtl {mesh.materialName}");
                                outputFile.WriteLine($"Ns {mesh.material.ns:f6}");
                                outputFile.WriteLine($"Ka {mesh.material.ka[0]:f6} {mesh.material.ka[1]:f6} {mesh.material.ka[2]:f6}");
                                outputFile.WriteLine($"Ks {mesh.material.ks[0]:f6} {mesh.material.ks[1]:f6} {mesh.material.ks[2]:f6}");
                                outputFile.WriteLine($"Ke {mesh.material.ke[0]:f6} {mesh.material.ke[1]:f6} {mesh.material.ke[2]:f6}");
                                outputFile.WriteLine($"Ni {mesh.material.ni:f6}");
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

            }
            else
            {
                MessageBox.Show("Nothing to save");
            }
        }

        // Creates a cube and adds it to the world
        private void CubeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mesh cube = new Mesh();
            cube.quads = new List<Quadrilateral>
            {
                // North Face
                new Quadrilateral(new Vec3D(1,0,1), new Vec3D(1,1,1), new Vec3D(0,1,1), new Vec3D(0,0,1)),
                // South Face
                new Quadrilateral(new Vec3D(0,0,0), new Vec3D(0,1,0), new Vec3D(1,1,0), new Vec3D(1,0,0)),
                // East Face
                new Quadrilateral(new Vec3D(1,0,0), new Vec3D(1,1,0), new Vec3D(1,1,1), new Vec3D(1,0,1)),
                // West Face
                new Quadrilateral(new Vec3D(0,0,1), new Vec3D(0,1,1), new Vec3D(0,1,0), new Vec3D(0,0,0)),
                // Top Face
                new Quadrilateral(new Vec3D(0,1,0), new Vec3D(0,1,1), new Vec3D(1,1,1), new Vec3D(1,1,0)),
                // Bottom Face
                new Quadrilateral(new Vec3D(0,0,1), new Vec3D(0,0,0), new Vec3D(1,0,0), new Vec3D(1,0,1)),
            };
            cube.name = "Cube" + Meshes.Count;
            cube.materialName = "Cube" + Meshes.Count;
            Meshes.Add(cube);
            ObjectList.Items.Add(cube.name);
        }
        // Generated by ChatGPT
        private void sphereToolStripMenuItem_Click(object sender, EventArgs e)
        {
            float radius = 1.0f;
            int latitudeSegments = 30;
            int longitudeSegments = 30;

            List<Triangle> triangles = new List<Triangle>();

            // Generate vertices
            List<Vec3D> vertices = new List<Vec3D>();
            for (int lat = 0; lat <= latitudeSegments; lat++)
            {
                float theta = lat * (float)Math.PI / latitudeSegments;
                float sinTheta = (float)Math.Sin(theta);
                float cosTheta = (float)Math.Cos(theta);

                for (int lon = 0; lon <= longitudeSegments; lon++)
                {
                    float phi = lon * 2 * (float)Math.PI / longitudeSegments;
                    float sinPhi = (float)Math.Sin(phi);
                    float cosPhi = (float)Math.Cos(phi);

                    float x = cosPhi * sinTheta;
                    float y = cosTheta;
                    float z = sinPhi * sinTheta;

                    vertices.Add(new Vec3D(x * radius, y * radius, z * radius));
                }
            }

            // Generate triangles
            for (int lat = 0; lat < latitudeSegments; lat++)
            {
                for (int lon = 0; lon < longitudeSegments; lon++)
                {
                    int first = lat * (longitudeSegments + 1) + lon;
                    int second = first + longitudeSegments + 1;

                    triangles.Add(new Triangle(vertices[first], vertices[first + 1], vertices[second]));
                    triangles.Add(new Triangle(vertices[second], vertices[first + 1], vertices[second + 1]));
                }
            }


            Mesh sphere = new Mesh();
            sphere.tris = triangles;
            sphere.name = "Sphere" + Meshes.Count;
            sphere.materialName = "Sphere" + Meshes.Count;
            Meshes.Add(sphere);
            ObjectList.Items.Add(sphere.name);
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

        private void MainForm_Click(object sender, EventArgs e)
        {
            // Allows user to deselect objects by clicking on the form
            ObjectList.ClearSelected();
            ActiveControl = null;
        }

        private void ViewWindow_Click(object sender, EventArgs e)
        {
            // Allows user to deselect objects by clicking on the picturebox
            ObjectList.ClearSelected();
            ActiveControl = null;
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = ObjectList.SelectedIndex;
            Meshes.RemoveAt(index);
            ObjectList.Items.RemoveAt(index);
        }

        private void ObjectList_MouseUp(object sender, MouseEventArgs e)
        {
            // If a user right clicked on an object, show the context menu
            if (e.Button == MouseButtons.Right & ObjectList.SelectedIndex != -1)
            {
                ContextMenuStrip.Show(Cursor.Position);
            }
        }

        private void UpDownX_ValueChanged(object sender, EventArgs e)
        {
            TransformObject((float)UpDownX.Value, TransformationBox.Text, ObjectList.SelectedIndex, 0);
        }

        private void UpDownY_ValueChanged(object sender, EventArgs e)
        {
            TransformObject((float)UpDownY.Value, TransformationBox.Text, ObjectList.SelectedIndex, 1);
        }

        private void UpDownZ_ValueChanged(object sender, EventArgs e)
        {
            TransformObject((float)UpDownZ.Value, TransformationBox.Text, ObjectList.SelectedIndex, 2);
        }

        private void ObjectList_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateUpDowns();
        }

        private void TransformationBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateUpDowns();
        }

        // When certain form elements are in focus, pressing certain keys causes
        // an error 'ding' to play. This prevents that from happening.
        // Alternatively, the user could also just have clicked off the element
        private void CamSpeedUpDown_KeyDown(object sender, KeyEventArgs e)
        {
            if (IsKeyIllegal(e.KeyCode))
            {
                e.SuppressKeyPress = true;
            }
        }

        private void UpDownX_KeyDown(object sender, KeyEventArgs e)
        {
            if (IsKeyIllegal(e.KeyCode))
            {
                e.SuppressKeyPress = true;
            }
        }

        private void UpDownY_KeyDown(object sender, KeyEventArgs e)
        {
            if (IsKeyIllegal(e.KeyCode))
            {
                e.SuppressKeyPress = true;
            }
        }

        private void UpDownZ_KeyDown(object sender, KeyEventArgs e)
        {
            if (IsKeyIllegal(e.KeyCode))
            {
                e.SuppressKeyPress = true;
            }
        }
    }
}
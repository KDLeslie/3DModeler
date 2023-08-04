using System.Data;
using System.Diagnostics;
using System.Drawing.Imaging;
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
        Stopwatch stopWatch = new Stopwatch();
        View mainView;
        int frameCount = 0; // Stores how many frames are rendered each second
        float frameTime = 0; // Stores the cumulative time between frames 
        float tick = 0;
        float tock = 0;
        float fElapsedTime = 0;  // Stores the time between each frame in seconds
        List<Mesh> meshes = new List<Mesh>(); // Stores each mesh loaded from an obj file

        // Stores what keys the user is currently pressing
        Dictionary<Keys, bool> keyPressed = new Dictionary<Keys, bool>()
        {
            { Keys.Up, false },
            { Keys.Down, false },
            { Keys.Left, false },
            { Keys.Right, false },
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
            }
            public Triangle(Vec3d p1, Vec3d p2, Vec3d p3)
            {
                this.p[0] = p1;
                this.p[1] = p2;
                this.p[2] = p3;
            }
            public Triangle(Vec3d p1, Vec3d p2, Vec3d p3, Vec2d t1, Vec2d t2, Vec2d t3)
            {
                this.p[0] = p1;
                this.p[1] = p2;
                this.p[2] = p3;
                this.t[0] = t1;
                this.t[1] = t2;
                this.t[2] = t3;
            }

            public Vec3d[] p = new Vec3d[3]; // Stores vertex coordinates for the triangle
            public Vec2d[] t = new Vec2d[3]; // Stores texel coordinates for each vertex
            public float lum = 0; // Calculated luminosity for the triangle based on light source
            public Vec3d normal = new Vec3d(); // Normal vector of the triangle
            public bool drawLine0_1 = true;
            public bool drawLine1_2 = true;
            public bool drawLine2_0 = true;             
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
            }
            // A quadrilateral value type used to present triangles in 3D objects
            public Quadrilateral(Vec3d p1, Vec3d p2, Vec3d p3, Vec3d p4)
            {
                this.p[0] = p1;
                this.p[1] = p2;
                this.p[2] = p3;
                this.p[3] = p4;
            }
            public Quadrilateral(Vec3d p1, Vec3d p2, Vec3d p3, Vec3d p4, Vec2d t1, Vec2d t2, Vec2d t3, Vec2d t4)
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

            public Vec3d[] p = new Vec3d[4]; // Stores vertex coordinates for the quadrilateral
            public Vec2d[] t = new Vec2d[4]; // Stores texel coordinates for each vertex
            public float lum = 0; // Calculated luminosity for the Quadrilateral based on light source
            public Vec3d normal = new Vec3d(); // Normal vector for the quadrilateral
        }

        // A structure used for storing mesh information for a 3D object
        struct Mesh
        {
            public Mesh() { }

            public List<Triangle> tris = new List<Triangle>();

            public List<Quadrilateral> quads = new List<Quadrilateral>();

            public DirectBitmap Texture = new DirectBitmap();

            public bool hasTexture = false;
            // Loads an object from a file
            // Returns true if successful
            // Currently only supports files with a single object
            public bool LoadObjectFromFile(string sFilename, ref string materialName, ref bool hasMaterial)
            {
                // by default
                hasMaterial = false;
                materialName = "";

                // Create an instance of StreamReader to read from a file
                // The using statement also closes the StreamReader
                using (StreamReader sr = new StreamReader(sFilename))
                {
                    // Store the vertices to be indexed through 
                    List<Vec3d> verts = new List<Vec3d>();
                    List<Vec2d> texs = new List<Vec2d>();
                    string? line;
                    // Read lines from the file until the end of the file is reached
                    while ((line = sr.ReadLine()) != null)
                    {
                        // Catch any empty lines
                        if (line.Length == 0)
                            continue;

                        if (line[0] == 'v')
                        {
                            // information is space separated
                            string[] coords = line.Split(' ');
                            // If the line begins with 'vt' it contains texel coordinates
                            if (line[1] == 't')
                            {
                                Vec2d vec = new Vec2d
                                {
                                    // 0th coord is junk character
                                    u = float.Parse(coords[1]),
                                    v = float.Parse(coords[2])
                                };
                                texs.Add(vec);

                            }
                            // If the line begins with 'v' it contains vertex coordinates
                            else
                            {
                                Vec3d vec = new Vec3d
                                {
                                    x = float.Parse(coords[1]),
                                    y = float.Parse(coords[2]),
                                    z = float.Parse(coords[3])
                                };
                                verts.Add(vec);
                            }
                        }
                        // If the line begins with 'm' it specifies the material settings file
                        if (line[0] == 'm')
                        {
                            string[] name = line.Split(' ');
                            materialName = name[1];
                            hasMaterial = true;
                        }
                        if (!hasMaterial)
                        {
                            // If the line begins with 'f' it specifies the indices into each list
                            // of the vertex and texel coordinates for the face
                            if (line[0] == 'f')
                            {
                                string[] indices = line.Split(' ');
                                if (indices.Length == 4)
                                {
                                    Triangle triangle = new Triangle();
                                    // Index through pool of vertices to get the ones corresponding
                                    // to this face
                                    // obj files use 1 indexing so our indices are off by 1
                                    triangle.p[0] = verts[int.Parse(indices[1]) - 1];
                                    triangle.p[1] = verts[int.Parse(indices[2]) - 1];
                                    triangle.p[2] = verts[int.Parse(indices[3]) - 1];
                                    tris.Add(triangle);
                                }
                                else if (indices.Length == 5)
                                {
                                    Quadrilateral quadrilateral = new Quadrilateral();
                                    // Index through pool of vertices to get the ones corresponding
                                    // to this face
                                    // obj files use 1 indexing so our indices are off by 1
                                    quadrilateral.p[0] = verts[int.Parse(indices[1]) - 1];
                                    quadrilateral.p[1] = verts[int.Parse(indices[2]) - 1];
                                    quadrilateral.p[2] = verts[int.Parse(indices[3]) - 1];
                                    quadrilateral.p[3] = verts[int.Parse(indices[4]) - 1];
                                    quads.Add(quadrilateral);
                                }
                            }
                        }
                        else
                        {
                            if (line[0] == 'f')
                            {
                                string[] indexPairs = line.Split(' ');
                                if (indexPairs.Length == 4)
                                {
                                    // Temporary arrays to store the indices for the vertices and texels
                                    int[] p = new int[3];
                                    int[] t = new int[3];
                                    for (int i = 0; i < 3; i++)
                                    {
                                        // Vertex and texel indices are separated bt '/' 
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
                                    tris.Add(triangle);
                                }
                                else if (indexPairs.Length == 5)
                                {
                                    // Temporary arrays to store the indices for the vertices and texels
                                    int[] p = new int[4];
                                    int[] t = new int[4];
                                    for (int i = 0; i < 4; i++)
                                    {
                                        // Vertex and texel indices are separated bt '/' 
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
                                    quads.Add(quadrilateral);
                                }

                            }

                        }
                    }
                }
                return true;
            }

            // Loads the material file associated with the object. Returns true if successful
            // Currently only reads the texture file
            public bool LoadMaterialFromFile(string sFilename, ref string texturePath, ref bool hasTexture)
            {
                hasTexture = false;
                using (StreamReader sr = new StreamReader(sFilename))
                {
                    string? line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        // catches empty lines
                        if (line.Length == 0)
                            continue;
                        // Specifies the texture file for the object
                        if (line[0] == 'm')
                        {
                            string[] texName = line.Split(' ');
                            texturePath = texName[1];
                            hasTexture = true;
                        }
                    }
                }
                return true;
            }

            // Loads texture file. Returns true if successful
            public bool LoadTextureFromFile(string sFilename)
            {
                Texture = new DirectBitmap(sFilename);
                return true;
            }
        }

        // A class that contains all the information regarding the current view into the world
        class View
        {
            public View() { }
            public View(int screenWidth, int screenHeight, int pixelWidth, int pixelHeight)
            {
                this.screenWidth = screenWidth / pixelWidth;
                this.screenHeight = screenHeight / pixelHeight;
                this.pixelWidth = pixelWidth;
                this.pixelHeight = pixelHeight;
                this.pDepthBuffer = new float[screenWidth * screenHeight];
            }
            public Mat4x4 matProj = new Mat4x4(); // Matrix that converts from view space to screen space
            public Vec3d vCamera = new Vec3d(); // Location of camera in world space
            public Vec3d vLookDir = new Vec3d(); // Direction vector along the direction camera points
            public float fYaw { get; set; } // Camera rotation about the y-axis
            public float fPitch { get; set; } // Camera rotation about the x-axis
            public float fTheta { get; set; } // Used to rotate the world            
            public int screenWidth { get; set; }
            public int screenHeight { get; set; }
            public int pixelWidth { get; set; }
            public int pixelHeight { get; set; }
            public float[] pDepthBuffer { get; set; } = new float[1]; // Used to determine the z-depth of each screen pixel
            public DirectBitmap frame = new DirectBitmap(1, 1); // A bitmap representing the frame drawn to the picturebox

            // Takes in a plane and a triangle and creates 0-2 new triangles based on how the triangle intersects the plane.
            // Returns the number of new triangles created
            public int Triangle_ClipAgainstPlane(Vec3d plane_p, Vec3d plane_n, ref Triangle in_tri, ref Triangle out_tri1, ref Triangle out_tri2)
            {
                // Make sure plane normal is indeed normal
                plane_n = Vector_Normalize(ref plane_n);

                // Return signed shortest distance from point to plane, plane normal must be normalized
                Func<Vec3d, float> dist = (Vec3d p) =>
                {
                    return (Vector_DotProduct(ref plane_n, ref p) - Vector_DotProduct(ref plane_n, ref plane_p));
                };

                // Create two temporary storage arrays to classify points either side of plane
                // If distance sign is positive, point lies on "inside" of plane
                Vec3d[] inside_points = new Vec3d[3] { new Vec3d(), new Vec3d(), new Vec3d() };
                int nInsidePointCount = 0;
                Vec3d[] outside_points = new Vec3d[3] { new Vec3d(), new Vec3d(), new Vec3d() };
                int nOutsidePointCount = 0;
                Vec2d[] inside_tex = new Vec2d[3] { new Vec2d(), new Vec2d(), new Vec2d() };
                int nInsideTexCount = 0;
                Vec2d[] outside_tex = new Vec2d[3] { new Vec2d(), new Vec2d(), new Vec2d() };
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
                    //out_tri1.drawline0_1 = in_tri.drawline0_1;
                    //out_tri1.drawline1_2 = in_tri.drawline1_2;
                    //out_tri1.drawline2_0 = in_tri.drawline2_0;


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

                    // out_tri1.drawline2_0 = false;

                    return 1; // Return the newly formed single triangle
                }
                else if (nInsidePointCount == 2 && nOutsidePointCount == 1)
                {
                    // Triangle should be clipped. As two points lie inside the plane,
                    // the clipped triangle becomes a "quad". Fortunately, we can
                    // represent a quad with two new triangles

                    // Copy appearance info to new triangles
                    out_tri1.lum = in_tri.lum;
                    //out_tri1.drawline0_1 = in_tri.drawline0_1;
                    //out_tri1.drawline1_2 = in_tri.drawline1_2;
                    //out_tri1.drawline2_0 = in_tri.drawline2_0;

                    out_tri2.lum = in_tri.lum;
                    //out_tri2.drawline0_1 = in_tri.drawline0_1;
                    //out_tri2.drawline1_2 = in_tri.drawline1_2;
                    //out_tri2.drawline2_0 = in_tri.drawline2_0;

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
                    // out_tri1.drawline2_0 = false;

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
                    // out_tri2.drawline0_1 = false;
                    return 2; // Return two newly formed triangles which form a quad
                }
                else
                {
                    return 0;
                }
            }

            //public (int, int) Quadrilateral_ClipAgainstPlane(Vec3d plane_p, Vec3d plane_n, ref Quadrilateral in_quad, ref Quadrilateral out_quad, ref Triangle out_tri)
            //{
            //    // Make sure plane normal is indeed normal
            //    plane_n = Vector_Normalize(ref plane_n);

            //    // Return signed shortest distance from point to plane, plane normal must be normalized
            //    Func<Vec3d, float> dist = (Vec3d p) =>
            //    {
            //        return (Vector_DotProduct(ref plane_n, ref p) - Vector_DotProduct(ref plane_n, ref plane_p));
            //    };

            //    // Create two temporary storage arrays to classify points either side of plane
            //    // If distance sign is positive, point lies on "inside" of plane
            //    Vec3d[] inside_points = new Vec3d[4] { new Vec3d(), new Vec3d(), new Vec3d(), new Vec3d() };
            //    int nInsidePointCount = 0;
            //    Vec3d[] outside_points = new Vec3d[4] { new Vec3d(), new Vec3d(), new Vec3d(), new Vec3d() };
            //    int nOutsidePointCount = 0;
            //    Vec2d[] inside_tex = new Vec2d[4] { new Vec2d(), new Vec2d(), new Vec2d(), new Vec2d() };
            //    int nInsideTexCount = 0;
            //    Vec2d[] outside_tex = new Vec2d[4] { new Vec2d(), new Vec2d(), new Vec2d(), new Vec2d() };
            //    int nOutsideTexCount = 0;


            //    Dictionary<Vec3d, Vec3d> nextVertex = new Dictionary<Vec3d, Vec3d>
            //    {
            //        { in_quad.p[0], in_quad.p[1] },
            //        { in_quad.p[1], in_quad.p[2] },
            //        { in_quad.p[2], in_quad.p[3] },
            //        { in_quad.p[3], in_quad.p[0] }
            //    };

            //    Dictionary<Vec3d, Vec3d> previousVertex = new Dictionary<Vec3d, Vec3d>
            //    {
            //        {  in_quad.p[0], in_quad.p[3] },
            //        {  in_quad.p[1], in_quad.p[0] },
            //        {  in_quad.p[2], in_quad.p[1] },
            //        {  in_quad.p[3], in_quad.p[2] }
            //    };


            //    Dictionary<Vec2d, Vec2d> nextTexVertex = new Dictionary<Vec2d, Vec2d>
            //    {
            //        { in_quad.t[0], in_quad.t[1] },
            //        { in_quad.t[1], in_quad.t[2] },
            //        { in_quad.t[2], in_quad.t[3] },
            //        { in_quad.t[3], in_quad.t[0] }
            //    };

            //    Dictionary<Vec2d, Vec2d> previousTexVertex = new Dictionary<Vec2d, Vec2d>
            //    {
            //        {  in_quad.t[0], in_quad.t[3] },
            //        {  in_quad.t[1], in_quad.t[0] },
            //        {  in_quad.t[2], in_quad.t[1] },
            //        {  in_quad.t[3], in_quad.t[2] }
            //    };


            //    // Get signed distance of each point in triangle to plane
            //    float d0 = dist(in_quad.p[0]);
            //    float d1 = dist(in_quad.p[1]);
            //    float d2 = dist(in_quad.p[2]);
            //    float d3 = dist(in_quad.p[3]);

            //    if (d0 >= 0)
            //    {
            //        inside_points[nInsidePointCount++] = in_quad.p[0];
            //        inside_tex[nInsideTexCount++] = in_quad.t[0];
            //    }
            //    else
            //    {
            //        outside_points[nOutsidePointCount++] = in_quad.p[0];
            //        outside_tex[nOutsideTexCount++] = in_quad.t[0];
            //    }
            //    if (d1 >= 0)
            //    {
            //        inside_points[nInsidePointCount++] = in_quad.p[1];
            //        inside_tex[nInsideTexCount++] = in_quad.t[1];
            //    }
            //    else
            //    {
            //        outside_points[nOutsidePointCount++] = in_quad.p[1];
            //        outside_tex[nOutsideTexCount++] = in_quad.t[1];
            //    }
            //    if (d2 >= 0)
            //    {
            //        inside_points[nInsidePointCount++] = in_quad.p[2];
            //        inside_tex[nInsideTexCount++] = in_quad.t[2];
            //    }
            //    else
            //    {
            //        outside_points[nOutsidePointCount++] = in_quad.p[2];
            //        outside_tex[nOutsideTexCount++] = in_quad.t[2];
            //    }
            //    if (d3 >= 0)
            //    {
            //        inside_points[nInsidePointCount++] = in_quad.p[3];
            //        inside_tex[nInsideTexCount++] = in_quad.t[3];
            //    }
            //    else
            //    {
            //        outside_points[nOutsidePointCount++] = in_quad.p[3];
            //        outside_tex[nOutsideTexCount++] = in_quad.t[3];
            //    }

            //    // Now classify quadrilateral points, and break the input quadrilateral into 
            //    // smaller output quadrilaterals if required. There are five possible
            //    // outcomes...

            //    if (nInsidePointCount == 0)
            //    {
            //        // All points lie on the outside of plane, so clip whole quadrilateral
            //        // It ceases to exist

            //        return (0, 0); // No returned triangles are valid
            //    }
            //    else if (nInsidePointCount == 3)
            //    {
            //        // All points lie on the inside of plane, so do nothing
            //        // and allow the triangle to simply pass through
            //        out_quad = in_quad;

            //        return (1, 0); // Just the one returned original triangle is valid
            //    }
            //    else if (nInsidePointCount == 1 && nOutsidePointCount == 3)
            //    {
            //        // Triangle should be clipped. As two points lie outside
            //        // the plane, the triangle simply becomes a smaller triangle

            //        // Copy appearance info to new triangle
            //        out_tri.lum = in_quad.lum;

            //        // The inside point is valid, so keep that...
            //        out_tri.p[0] = inside_points[0];
            //        out_tri.t[0] = inside_tex[0];

            //        // but the two new points are at the locations where the 
            //        // original sides of the triangle (lines) intersect with the plane

            //        Vec3d v1 = nextVertex[inside_points[0]];
            //        Vec3d v2 = previousVertex[inside_points[0]];



            //        float t = 0;
            //        out_tri.p[1] = Vector_IntersectPlane(ref plane_p, ref plane_n, ref inside_points[0], ref v1, ref t);
            //        out_tri.t[1].u = t * (nextTexVertex[inside_tex[0]].u - inside_tex[0].u) + inside_tex[0].u;
            //        out_tri.t[1].v = t * (nextTexVertex[inside_tex[0]].v - inside_tex[0].v) + inside_tex[0].v;
            //        out_tri.t[1].w = t * (nextTexVertex[inside_tex[0]].w - inside_tex[0].w) + inside_tex[0].w;

            //        out_tri.p[2] = Vector_IntersectPlane(ref plane_p, ref plane_n, ref inside_points[0], ref v2, ref t);
            //        out_tri.t[2].u = t * (previousTexVertex[inside_tex[0]].u - inside_tex[0].u) + inside_tex[0].u;
            //        out_tri.t[2].v = t * (previousTexVertex[inside_tex[0]].v - inside_tex[0].v) + inside_tex[0].v;
            //        out_tri.t[2].w = t * (previousTexVertex[inside_tex[0]].w - inside_tex[0].w) + inside_tex[0].w;

            //        return (0, 1); // Return the newly formed single triangle
            //    }
            //    else if (nInsidePointCount == 2 && nOutsidePointCount == 2)
            //    {
            //        // Triangle should be clipped. As two points lie outside
            //        // the plane, the triangle simply becomes a smaller triangle

            //        // Copy appearance info to new triangle
            //        out_quad.lum = in_quad.lum;

            //        // The inside points are valid, so keep that...
            //        out_quad.p[0] = inside_points[0];
            //        out_quad.t[0] = inside_tex[0];

            //        out_quad.p[3] = inside_points[1];
            //        out_quad.t[3] = inside_tex[1];

            //        // but the two new points are at the locations where the 
            //        // original sides of the triangle (lines) intersect with the plane
            //        float t = 0;
            //        out_quad.p[1] = Vector_IntersectPlane(ref plane_p, ref plane_n, ref inside_points[0], ref outside_points[0], ref t);
            //        out_quad.t[1].u = t * (outside_tex[0].u - inside_tex[0].u) + inside_tex[0].u;
            //        out_quad.t[1].v = t * (outside_tex[0].v - inside_tex[0].v) + inside_tex[0].v;
            //        out_quad.t[1].w = t * (outside_tex[0].w - inside_tex[0].w) + inside_tex[0].w;

            //        out_quad.p[2] = Vector_IntersectPlane(ref plane_p, ref plane_n, ref inside_points[1], ref outside_points[1], ref t);
            //        out_quad.t[2].u = t * (outside_tex[1].u - inside_tex[1].u) + inside_tex[1].u;
            //        out_quad.t[2].v = t * (outside_tex[1].v - inside_tex[1].v) + inside_tex[1].v;
            //        out_quad.t[2].w = t * (outside_tex[1].w - inside_tex[1].w) + inside_tex[1].w;

            //        return (1, 0); // Return the newly formed single triangle
            //    }
            //    else if (nInsidePointCount == 3 && nOutsidePointCount == 1)
            //    {

            //        Vec3d v1 = nextVertex[outside_points[0]];
            //        Vec3d v2 = previousVertex[outside_points[0]];


            //        float t = 0;
            //        out_tri.p[0] = Vector_IntersectPlane(ref plane_p, ref plane_n, ref outside_points[0], ref v1, ref t);
            //        out_tri.t[0].u = t * (nextTexVertex[outside_tex[0]].u - outside_tex[0].u) + outside_tex[0].u;
            //        out_tri.t[0].v = t * (nextTexVertex[outside_tex[0]].v - outside_tex[0].v) + outside_tex[0].v;
            //        out_tri.t[0].w = t * (nextTexVertex[outside_tex[0]].w - outside_tex[0].w) + outside_tex[0].w;


            //        out_tri.p[1] = Vector_IntersectPlane(ref plane_p, ref plane_n, ref outside_points[0], ref v2, ref t);
            //        out_tri.t[1].u = t * (previousTexVertex[outside_tex[0]].u - outside_tex[0].u) + outside_tex[0].u;
            //        out_tri.t[1].v = t * (previousTexVertex[outside_tex[0]].v - outside_tex[0].v) + outside_tex[0].v;
            //        out_tri.t[1].w = t * (previousTexVertex[outside_tex[0]].w - outside_tex[0].w) + outside_tex[0].w;

            //        out_tri.p[2] = nextVertex[outside_points[0]];
            //        out_tri.t[2] = nextTexVertex[outside_tex[0]];

            //        out_tri.lum = in_quad.lum;


            //        out_quad.p[0] = new Vec3d(out_tri.p[1]);
            //        out_quad.t[0] = new Vec2d(out_tri.t[1]);
            //        out_quad.p[1] = new Vec3d(previousVertex[outside_points[0]]);
            //        out_quad.t[1] = new Vec2d(previousTexVertex[outside_tex[0]]);
            //        out_quad.p[2] = new Vec3d(previousVertex[v2]);
            //        out_quad.t[2] = new Vec2d(previousTexVertex[previousTexVertex[outside_tex[0]]]);
            //        out_quad.p[3] = new Vec3d(nextVertex[outside_points[0]]);
            //        out_quad.t[3] = new Vec2d(nextTexVertex[outside_tex[0]]);

            //        out_quad.lum = in_quad.lum;


            //        return (1, 1); // Return two newly formed triangles which form a quad
            //    }
            //    else
            //    {
            //        return (0, 0);
            //    }
            //}

            // Returns a color based on an objects luminance between 0 and 1

            public void RasterizeTriangle(Mat4x4 worldMat, Mat4x4 matView, Triangle tri, ref List<Triangle> vecTrianglesToRaster)
            {
                // Prepare each triangle for drawing

                Triangle triTransformed = new Triangle();
                // World Matrix Transform
                triTransformed.p[0] = tri.p[0] * worldMat;
                triTransformed.p[1] = tri.p[1] * worldMat;
                triTransformed.p[2] = tri.p[2] * worldMat;
                triTransformed.t[0] = tri.t[0];
                triTransformed.t[1] = tri.t[1];
                triTransformed.t[2] = tri.t[2];
                triTransformed.drawLine0_1 = tri.drawLine0_1;
                triTransformed.drawLine1_2 = tri.drawLine1_2;
                triTransformed.drawLine2_0 = tri.drawLine2_0;

                // Calculate triangle's Normal 
                Vec3d normal, line1, line2;

                // Get lines on either side of triangle
                line1 = triTransformed.p[1] - triTransformed.p[0];
                line2 = triTransformed.p[2] - triTransformed.p[0];

                // Take the cross product of lines to get normal to triangle surface
                normal = Vector_CrossProduct(ref line1, ref line2);
                normal = Vector_Normalize(ref normal);

                // Get Ray from camera to triangle
                Vec3d vCameraRay = triTransformed.p[0] - vCamera;

                // If ray is aligned with normal then triangle is visible
                // if not it is culled, in other words, triangles with normals 
                // facing away from the camera won't be seen.
                // Note: Make toggleable
                if (Vector_DotProduct(ref normal, ref vCameraRay) < 0.0f)
                {
                    // Illumination
                    Vec3d light_direction = new Vec3d(0.0f, -1.0f, 1.0f);
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
                    triViewed.drawLine0_1 = triTransformed.drawLine0_1;
                    triViewed.drawLine1_2 = triTransformed.drawLine1_2;
                    triViewed.drawLine2_0 = triTransformed.drawLine2_0;
                    triViewed.t[0] = triTransformed.t[0];
                    triViewed.t[1] = triTransformed.t[1];
                    triViewed.t[2] = triTransformed.t[2];

                    // Clip the Viewed Triangle against near plane, this could form two additional
                    // triangles.
                    int nClippedTriangles = 0;
                    Triangle[] clipped = new Triangle[2] { new Triangle(), new Triangle() };
                    nClippedTriangles = Triangle_ClipAgainstPlane(new Vec3d(0.0f, 0.0f, 0.1f), new Vec3d(0.0f, 0.0f, 1.0f), ref triViewed, ref clipped[0], ref clipped[1]);

                    // We may end up with multiple triangles form the clip, so project as
                    // required
                    for (int n = 0; n < nClippedTriangles; n++)
                    {
                        // Project triangles from 3D --> 2D
                        // View space -> screen space
                        Triangle triProjected = new Triangle();
                        triProjected.p[0] = clipped[n].p[0] * matProj;
                        triProjected.p[1] = clipped[n].p[1] * matProj;
                        triProjected.p[2] = clipped[n].p[2] * matProj;
                        triProjected.lum = clipped[n].lum;
                        triProjected.drawLine0_1 = clipped[n].drawLine0_1;
                        triProjected.drawLine1_2 = clipped[n].drawLine1_2;
                        triProjected.drawLine2_0 = clipped[n].drawLine2_0;
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
                        Vec3d vOffsetView = new Vec3d(1, 1, 0);
                        triProjected.p[0] = triProjected.p[0] + vOffsetView;
                        triProjected.p[1] = triProjected.p[1] + vOffsetView;
                        triProjected.p[2] = triProjected.p[2] + vOffsetView;

                        // vertices are now between 0 and 2 so we scale into view
                        triProjected.p[0].x *= 0.5f * screenWidth;
                        triProjected.p[0].y *= 0.5f * screenHeight;
                        triProjected.p[1].x *= 0.5f * screenWidth;
                        triProjected.p[1].y *= 0.5f * screenHeight;
                        triProjected.p[2].x *= 0.5f * screenWidth;
                        triProjected.p[2].y *= 0.5f * screenHeight;

                        // Store triangle for sorting
                        vecTrianglesToRaster.Add(triProjected);
                    }
                }
            }
            public Color GetColor(float lum)
            {
                // converts the luminance back to argb values
                return Color.FromArgb(255, (int)(lum * 255), (int)(lum * 255), (int)(lum * 255));
            }

            // Draws a triangle to a bitmap
            public void DrawTriangle(Triangle tri, DirectBitmap texture, bool hasTexture = false, bool shading = true)
            {
                // The pixel domain is integers. Can't move half a pixel
                // Not sure if rounding even matters here
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
                    for (int i = y1; i < y2; i++)
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
                            if (tex_w > pDepthBuffer[i * screenWidth + j])
                            {
                                // Divide by tex_w to get denormalized texel coordinate
                                float u = (tex_u / tex_w);
                                // The v axis is in texel space is inverted compared to the y-axis in bitmaps
                                // 1 - v undoes the inversion
                                float v = (1 - tex_v / tex_w);
                                // uv coordinates are between 0 and 1. Anything outside those values will
                                // be wrapped through repetition 
                                u = u >= 0 ? u % 1 : u % 1 + 1.0f;
                                v = v >= 0 ? v % 1 : v % 1 + 1.0f;
                                // Scale up texel coordinates to the height and width of the textures
                                int w = (int)(u * texture.Width);
                                int h = (int)(v * texture.Height);
                                if (!hasTexture)
                                {
                                    if (shading)
                                    {
                                        Color color = GetColor(lum);
                                        frame.SetPixel(j, i, color);
                                    }
                                    else
                                    {
                                        frame.SetPixel(j, i, Color.White);
                                    }
                                }
                                else
                                {
                                    if (shading)
                                    {
                                        Color color = texture.GetPixel(w, h);
                                        // Naïve implementation of shading
                                        Color newCol = Color.FromArgb(255, (int)(color.R * lum), (int)(color.G * lum), (int)(color.B * lum));
                                        frame.SetPixel(j, i, newCol);
                                    }
                                    else
                                    {
                                        Color color = texture.GetPixel(w, h);
                                        frame.SetPixel(j, i, color);
                                    }
                                }
                                // Update depth buffer
                                pDepthBuffer[i * screenWidth + j] = tex_w;
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
                    for (int i = y2; i < y3; i++)
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

                            if (tex_w > pDepthBuffer[i * screenWidth + j])
                            {
                                float u = (tex_u / tex_w);
                                float v = (1 - tex_v / tex_w);
                                u = u >= 0 ? u % 1 : u % 1 + 1.0f;
                                v = v >= 0 ? v % 1 : v % 1 + 1.0f;
                                int w = (int)(u * texture.Width);
                                int h = (int)(v * texture.Height);
                                if (!hasTexture)
                                {
                                    if (shading)
                                    {
                                        Color color = GetColor(lum);
                                        frame.SetPixel(j, i, color);
                                    }
                                    else
                                    {
                                        frame.SetPixel(j, i, Color.White);
                                    }
                                }
                                else
                                {
                                    if (shading)
                                    {
                                        Color color = texture.GetPixel(w, h);
                                        Color newCol = Color.FromArgb(255, (int)(color.R * lum), (int)(color.G * lum), (int)(color.B * lum));
                                        frame.SetPixel(j, i, newCol);
                                    }
                                    else
                                    {
                                        Color color = texture.GetPixel(w, h);
                                        frame.SetPixel(j, i, color);
                                    }
                                }
                                pDepthBuffer[i * screenWidth + j] = tex_w;
                            }
                            t += tstep;
                        }
                    }
                }
            }

            // Faster DrawLine than using Graphics.DrawLine.
            // Taken from the link below
            // https://rosettacode.org/wiki/Bitmap/Bresenham%27s_line_algorithm#C#
            public void DrawLine(int x0, int y0, int x1, int y1, DirectBitmap bitmap, Color color)
            {
                int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
                int dy = Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
                int err = (dx > dy ? dx : -dy) / 2, e2;
                for (; ; )
                {
                    bitmap.SetPixel(x0, y0, color);
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
        // by A.Konzel. Taken from
        // https://stackoverflow.com/questions/24701703/c-sharp-faster-alternatives-to-setpixel-and-getpixel-for-bitmaps-for-windows-f
        //Note: Locking and unlocking bits may work just as well
        public class DirectBitmap : IDisposable
        {
            public DirectBitmap() { }

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
                // Get the address of the first line
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

        private void Form1_Load(object sender, EventArgs e)
        {
            // this.DoubleBuffered = true; // may not be needed since picturebox is already double buffered
            stopWatch.Start();
            // How often the tick event is run
            Clock.Interval = 20;
            Clock.Enabled = true;
            // Initialize the class and components pertaining to the main view into the world
            mainView = new View(Viewer.Width, Viewer.Height, 1, 1);
            // Setup Projection Matrix
            mainView.matProj = Matrix_MakeProjection(90, (float)mainView.screenHeight / (float)mainView.screenWidth, 0.1f, 1000.0f);
        }

        private void Clock_Tick(object sender, EventArgs e)
        {
            // https://github.com/OneLoneCoder/olcPixelGameEngine/blob/147c25a018c917030e59048b5920c269ef583c50/olcPixelGameEngine.h#L3823
            // Runs paint event. Renders frame
            tock = (float)stopWatch.Elapsed.TotalSeconds;
            // Get elapsed time between the before rendering current frame and
            // before rending the previous frame
            fElapsedTime = tock - tick;
            tick = tock;
            // Run the paint event to render the next frame
            Viewer.Refresh();
            frameCount += 1;
            frameTime += fElapsedTime;
            if (frameTime >= 1.0f)
            {
                FPS.Text = $"FPS: {frameCount}";
                frameCount = 0;
                frameTime -= 1; // Possibly change to set to 0
            }
            // Temporary labels
            label1.Text = $"Frame: {frameCount}";
            label2.Text = $"ELapsed time: {stopWatch.Elapsed.TotalSeconds}";
            // sw.Restart();
            // fps calculation idea from https://github.com/OneLoneCoder/olcPixelGameEngine/blob/147c25a018c917030e59048b5920c269ef583c50/olcPixelGameEngine.h#L3823
        }

        // Called once per clock tick event
        // Renders the frame
        private void Viewer_Paint(object sender, PaintEventArgs e)
        {
            
            if (keyPressed[Keys.Up])
                mainView.vCamera.y += 8.0f * fElapsedTime; // Travel along positive y-axis

            if (keyPressed[Keys.Down])
                mainView.vCamera.y -= 8.0f * fElapsedTime; // Travel along negative y-axis

            if (keyPressed[Keys.Left])
                mainView.vCamera.x -= 8.0f * fElapsedTime; // Travel along negative x-axis

            if (keyPressed[Keys.Right])
                mainView.vCamera.x += 8.0f * fElapsedTime; // Travel along positive x-axis

            // A velocity vector used to control the forward movement of the camera
            Vec3d vVelocity = mainView.vLookDir * (8.0f * fElapsedTime);

            // Standard FPS Control scheme, but turn instead of strafe
            if (keyPressed[Keys.W])
                mainView.vCamera = mainView.vCamera + vVelocity;

            if (keyPressed[Keys.S])
                mainView.vCamera = mainView.vCamera - vVelocity;

            if (keyPressed[Keys.A])
                mainView.fYaw -= 2.0f * fElapsedTime;

            if (keyPressed[Keys.D])
                mainView.fYaw += 2.0f * fElapsedTime;

            if (keyPressed[Keys.R])
                mainView.fPitch -= 2.0f * fElapsedTime;

            if (keyPressed[Keys.F])
                mainView.fPitch += 2.0f * fElapsedTime;


            // Set up "World Transform"
            Mat4x4 worldMat = Matrix_MakeIdentity();
            // Temporary spinning of the world
            // mainView.fTheta += 1.0f * fElapsedTime;
            // Rotates the world
            Mat4x4 worldMatRotX = Matrix_MakeRotationX(mainView.fTheta);
            Mat4x4 worldMatRotY = Matrix_MakeRotationY(mainView.fTheta * 2);
            Mat4x4 worldMatRotZ = Matrix_MakeRotationZ(mainView.fTheta * 0.5f);

            // Scales the world
            Mat4x4 worldMatScale = Matrix_MakeScale(1, 1, 1);

            // Offsets the world
            Mat4x4 worldMatTrans = Matrix_MakeTranslation(0.0f, 0.0f, 5.0f);

            // Transformations must be done in this order
            worldMat *= worldMatScale; // Transform by scaling
            worldMat *= worldMatRotX * worldMatRotY * worldMatRotZ; // Transform by rotation about the origin
            worldMat *= worldMatTrans; // Transform by translation

            // Create "Point At" Matrix for camera
            Vec3d vUp = new Vec3d(0, 1, 0); // Default up direction for camera is along the positive y-axis
            Vec3d vForwardCam = new Vec3d(0, 0, 1); // Default forward direction for camera is along the positive z-axis
            // Cap pitch from being able to rotate too far.
            // TODO: Fix this
            mainView.fPitch = mainView.fPitch > 0 ? MathF.Min(3.1415f / 2, mainView.fPitch) : MathF.Max(-3.1415f / 2, mainView.fPitch);
            Mat4x4 cameraMatRotX = Matrix_MakeRotationX(mainView.fPitch);
            Mat4x4 cameraMatRotY = Matrix_MakeRotationY(mainView.fYaw);
            Mat4x4 cameraMatRot = cameraMatRotX * cameraMatRotY;
            // Rotated forward vector becomes the camera's look direction
            mainView.vLookDir = vForwardCam * cameraMatRot;
            // Offset look direction to the camera location to get the target the camera points at
            Vec3d vTarget = mainView.vCamera + mainView.vLookDir;
            // Construct the "Point At" matrix
            Mat4x4 matCamera = Matrix_PointAt(ref mainView.vCamera, ref vTarget, ref vUp);

            // Construct the "Look At" matrix from the inverse
            Mat4x4 matView = Matrix_QuickInverse(ref matCamera);

            // Dispose of the previous frame
            mainView.frame.Dispose();
            // Create a new background color for the frame
            mainView.frame = new DirectBitmap(mainView.screenWidth, mainView.screenHeight, Color.Cyan);

            // Clear depth buffer each frame
            for (int i = 0; i < mainView.screenWidth * mainView.screenHeight; i++)
            {
                mainView.pDepthBuffer[i] = 0.0f;
            }

            // Draw each mesh
            foreach (Mesh mesh in meshes)
            {
                // Store triangles for rastering later
                List<Triangle> vecTrianglesToRaster = new List<Triangle>();

                // List<Triangle> newTris = new List<Triangle>(mesh.quads.Count * 2);
                // Prepare each triangle for drawing
                foreach (Quadrilateral quad in mesh.quads)
                {
                    Triangle tri1 = new Triangle(quad.p[0], quad.p[1], quad.p[2], quad.t[0], quad.t[1], quad.t[2]);
                    tri1.drawLine1_2 = false; 
                    mainView.RasterizeTriangle(worldMat, matView, tri1, ref vecTrianglesToRaster);
                    Triangle tri2 = new Triangle(quad.p[0], quad.p[2], quad.p[3], quad.t[0], quad.t[2], quad.t[3]);
                    tri2.drawLine0_1 = false;
                    mainView.RasterizeTriangle(worldMat, matView, tri2, ref vecTrianglesToRaster);
                }
                // Prepare each triangle for drawing
                foreach (Triangle tri in mesh.tris)
                {
                    mainView.RasterizeTriangle(worldMat, matView, tri, ref vecTrianglesToRaster);
                }

                // Sort triangles from back to front through approximating
                // the triangles' z positions.
                // Useful for transparency
                bool sortTriangles = false;
                if(sortTriangles)
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
                                    nTrisToAdd = mainView.Triangle_ClipAgainstPlane(new Vec3d(0.0f, 0.0f, 0.0f), new Vec3d(0.0f, 1.0f, 0.0f), ref test, ref clipped[0], ref clipped[1]);
                                    break;
                                case 1:
                                    nTrisToAdd = mainView.Triangle_ClipAgainstPlane(new Vec3d(0.0f, (float)mainView.screenHeight - 1, 0.0f), new Vec3d(0.0f, -1.0f, 0.0f), ref test, ref clipped[0], ref clipped[1]);
                                    break;
                                case 2:
                                    nTrisToAdd = mainView.Triangle_ClipAgainstPlane(new Vec3d(0.0f, 0.0f, 0.0f), new Vec3d(1.0f, 0.0f, 0.0f), ref test, ref clipped[0], ref clipped[1]);
                                    break;
                                case 3:
                                    nTrisToAdd = mainView.Triangle_ClipAgainstPlane(new Vec3d((float)mainView.screenWidth - 1, 0.0f, 0.0f), new Vec3d(-1.0f, 0.0f, 0.0f), ref test, ref clipped[0], ref clipped[1]);
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

                    // Draw the transformed, viewed, clipped, projected, sorted, clipped triangles to a bitmap
                    foreach (Triangle t in listTriangles)
                    {
                        bool fillMesh = true;
                        if(fillMesh)
                            mainView.DrawTriangle(t, mesh.Texture, mesh.hasTexture, true);
                        bool wireframeMode = false;
                        if (wireframeMode)
                        {
                            // Currently does not work properly when combined with a textured or solid mesh unless triangles are sorted first.
                            // Triangles clipped on the edges of the screen are shown
                            if (t.drawLine0_1)
                                mainView.DrawLine((int)t.p[0].x, (int)t.p[0].y, (int)t.p[1].x, (int)t.p[1].y, mainView.frame, Color.Black);
                            
                            if (t.drawLine1_2)
                                mainView.DrawLine((int)t.p[0].x, (int)t.p[0].y, (int)t.p[2].x, (int)t.p[2].y, mainView.frame, Color.Black);
                            
                            if (t.drawLine2_0)
                                mainView.DrawLine((int)t.p[1].x, (int)t.p[1].y, (int)t.p[2].x, (int)t.p[2].y, mainView.frame, Color.Black);
                        }
                    }
                }
            }

            // Draw the bitmap to the screen
            e.Graphics.DrawImage(mainView.frame.Bitmap, 0, 0, mainView.pixelWidth * mainView.screenWidth, mainView.pixelHeight * mainView.screenHeight);
        }

        // Sets the current state of any pressed key
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            keyPressed[e.KeyCode] = true;
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            keyPressed[e.KeyCode] = false;
        }

        // Basic implementation of opining obj files. Not fully complete
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
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
                    meshes.Clear();

                    string filePath = openFileDialog.FileName;
                    string[] subPaths = filePath.Split('\\');
                    string folderPath = String.Join("\\", subPaths.SkipLast(1).ToArray());
                    string materialName = "";
                    string texturePath = "";
                    bool hasMaterial = false;
                    bool hasTexture = false;
                    Mesh meshCube = new Mesh();
                    meshCube.LoadObjectFromFile(filePath, ref materialName, ref hasMaterial);
                    if (hasMaterial)
                    {
                        string materialPath = Path.Combine(folderPath, materialName);
                        meshCube.LoadMaterialFromFile(materialPath, ref texturePath, ref hasTexture);
                        if (hasTexture)
                        {
                            texturePath = texturePath.Replace("/", "\\");
                            meshCube.LoadTextureFromFile(texturePath);
                            meshCube.hasTexture = true;
                        }
                    }
                    else
                    {
                        meshCube.hasTexture = false;
                    }
                    meshes.Add(meshCube);
                }
            }
        }

        private void AddCube_Click(object sender, EventArgs e)
        {
            //// Creates a cube made from tris
            //Vec3d[] points = new Vec3d[8];
            //Mesh cube = new Mesh();
            //for (int i = 0; i < 8; i++)
            //{
            //    points[i] = new Vec3d(i / 4, (i / 2) % 2, i % 2);
            //}

            //List<Triangle> tris = new List<Triangle>
            //{
            //    new Triangle(points[1], points[3], points[2]),
            //     new Triangle(points[1], points[2], points[0]),
            //      new Triangle( points[0], points[2], points[6]),
            //      new Triangle( points[0], points[6], points[4]),
            //      new Triangle( points[1], points[0], points[4]),
            //      new Triangle( points[1], points[4], points[5]),
            //      new Triangle( points[4], points[6], points[7]),
            //      new Triangle( points[4], points[7], points[5]),
            //      new Triangle( points[2], points[3], points[7]),
            //      new Triangle( points[2], points[7], points[6]),
            //      new Triangle( points[5], points[7], points[3]),
            //      new Triangle( points[5], points[3], points[1])
            //};
            //cube.tris = tris;
            //meshes.Add(cube);

            // A cube made of quadrilaterals.
            Mesh cube = new Mesh();
            List<Quadrilateral> quads = new List<Quadrilateral>
            {
                // South
                new Quadrilateral(new Vec3d(0,0,0), new Vec3d(0,1,0), new Vec3d(1,1,0), new Vec3d(1,0,0)),
                // East
                new Quadrilateral(new Vec3d(1,0,0), new Vec3d(1,1,0), new Vec3d(1,1,1), new Vec3d(1,0,1)),
                // West
                new Quadrilateral(new Vec3d(0,0,1), new Vec3d(0,1,1), new Vec3d(0,1,0), new Vec3d(0,0,0)),
                // Bottom
                new Quadrilateral(new Vec3d(0,0,1), new Vec3d(0,0,0), new Vec3d(1,0,0), new Vec3d(1,0,1)),
                // Top
                new Quadrilateral(new Vec3d(0,1,0), new Vec3d(0,1,1), new Vec3d(1,1,1), new Vec3d(1,1,0)),
                // North
                new Quadrilateral(new Vec3d(1,0,1), new Vec3d(1,1,1), new Vec3d(0,1,1), new Vec3d(0,0,1)),
            };
            cube.quads = quads;
            meshes.Add(cube);
        }

        // Necessary for keydown event to trigger for arrow keys
        private void AddCube_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            e.IsInputKey = true;
        }

        // Basic obj file saving. Only saves vertex and face information atm
        private void saveASToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Can't save anything if there are no meshes
            if (meshes.Count != 0)
            {
                // Initialize SaveFileDialog
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "obj files (*.obj)|*.obj";
                saveFileDialog1.FilterIndex = 2;
                saveFileDialog1.RestoreDirectory = false;

                // If a valid filename was chosen...
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    // Automatically closes file once done writing
                    using (StreamWriter outputFile = new StreamWriter(saveFileDialog1.OpenFile()))
                    {
                        // Output comment
                        outputFile.WriteLine("# test object");
                        foreach (Mesh mesh in meshes)
                        {
                            // As we loop through each polygon, we store each vertex and
                            // its index into the pool
                            Dictionary<Vec3d, int> indexOfVertex = new Dictionary<Vec3d, int>();                         
                            // Stores all the faces
                            List<int[]> faces = new List<int[]>();
                            // Pool of vertices start with an index of 1
                            int index = 1; 
                            foreach (Triangle tri in mesh.tris)
                            {
                                // Keeps track of the indices for each vertex of the face.
                                // The face of a triangle consists of 3 vertices
                                int[] face = new int[3];
                                // For each vertex in the triangle...
                                for (int i = 0; i < 3; i++)
                                {
                                    // Check whether that vertex is already in our pool of
                                    // vertices
                                    if (indexOfVertex.ContainsKey(tri.p[i]))
                                    {
                                        // If it already exits in the pool, we get the index
                                        // of that vertex and add it to the face
                                        face[i] = indexOfVertex[tri.p[i]];
                                    }
                                    else
                                    {
                                        // If we don't already have that vertex stored, then
                                        // we store it and set its index
                                        indexOfVertex[tri.p[i]] = index;
                                        // We get that index of that vertex for the face and we
                                        // increment the index for the next new vertex
                                        face[i] = index++;
                                    }
                                }
                                // Add the face to our list of faces
                                faces.Add(face);
                            }
                            // Repeat above code for each quadrilateral
                            foreach (Quadrilateral quad in mesh.quads)
                            {
                                // Quadrilaterals consist of 4 vertices
                                int[] face = new int[4];

                                for (int i = 0; i < 4; i++)
                                {
                                    if (indexOfVertex.ContainsKey(quad.p[i]))
                                    {
                                        face[i] = indexOfVertex[quad.p[i]];
                                    }
                                    else
                                    {
                                        indexOfVertex[quad.p[i]] = index;
                                        face[i] = index++;
                                    }
                                }
                                faces.Add(face);
                            }
                            // Output the collection in standard obj format
                            foreach (KeyValuePair<Vec3d, int> vertex in indexOfVertex)
                            {
                                // Explicitly set 6 decimal places
                                string output = $"v {vertex.Key.x:f6} {vertex.Key.y:f6} {vertex.Key.z:f6}";
                                outputFile.WriteLine(output);
                            }
                            // Output the collection in standard obj format
                            foreach (int[] face in faces)
                            {
                                if (face.Length == 3)
                                {
                                    string output = $"f {face[0]} {face[1]} {face[2]}";
                                    outputFile.WriteLine(output);

                                }
                                else if (face.Length == 4)
                                {
                                    string output = $"f {face[0]} {face[1]} {face[2]} {face[3]}";
                                    outputFile.WriteLine(output);
                                }
                            }

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
    }
}
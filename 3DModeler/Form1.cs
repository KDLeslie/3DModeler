using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Security;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Windows.Forms;
using static _3DModeler.Operations;

namespace _3DModeler
{
    public partial class Form1 : Form
    {
        readonly Stopwatch stopWatch = new Stopwatch();
        public Form1()
        {
            InitializeComponent();
        }

        View world;
        int frames = 0; // Stores how many frames are rendered each second
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
                this.col = tri.col;
            }
            public Triangle(Vec3d[] p)
            {
                for (int i = 0; i < 3; i++)
                {
                    this.p[i] = p[i];
                }
            }
            public Triangle(Vec3d[] p, Vec2d[] t)
            {
                for (int i = 0; i < 3; i++)
                {
                    this.p[i] = p[i];
                    this.t[i] = t[i];
                }
            }

            public Vec3d[] p = new Vec3d[3];  // Stores vertex coordinates for each triangle
            public Vec2d[] t = new Vec2d[3]; // Stores texel coordinates for each triangle
            public Color col = new Color();  // Used for the shading of each triangle

        }
        struct Mesh
        {
            public Mesh() { }

            // Mesh supports only triangular faces
            public List<Triangle> tris = new List<Triangle>();

            public DirectBitmap Texture = new DirectBitmap();

            public bool hasTexture = false;
            // Loads an object from a file. Returns true if successful
            public bool LoadObjectFromFile(string sFilename, ref string materialName, ref bool hasMaterial)
            {
                // by default
                hasMaterial = false;
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using (StreamReader sr = new StreamReader(sFilename))
                {
                    // Stores the vertices to be indexed through 
                    List<Vec3d> verts = new List<Vec3d>();
                    List<Vec2d> texs = new List<Vec2d>();
                    string line;
                    // Read lines from the file until the end of the file
                    // is reached.
                    while ((line = sr.ReadLine()) != null)
                    {
                        // catches empty lines
                        if (line.Length == 0) continue;

                        if (line[0] == 'v')
                        {
                            // 0th coord is junk character
                            string[] coords = line.Split(' ');
                            if (line[1] == 't')
                            {
                                Vec2d v = new Vec2d
                                {
                                    u = float.Parse(coords[1]),
                                    v = float.Parse(coords[2])
                                };
                                // u, v coordinates have bottom left as (0,0) but sprites have top left has (0,0)
                                // Invert the v-coordinate to match our system.
                                v.v = 1.0f - v.v;
                                texs.Add(v);

                            }
                            else
                            {
                                Vec3d v = new Vec3d
                                {
                                    x = float.Parse(coords[1]),
                                    y = float.Parse(coords[2]),
                                    z = float.Parse(coords[3])
                                };
                                verts.Add(v);
                            }
                        }
                        if (line[0] == 'm')
                        {
                            string[] indices = line.Split(' ');
                            materialName = indices[1];
                            hasMaterial = true;
                        }
                        if (!hasMaterial)
                        {
                            if (line[0] == 'f')
                            {
                                int[] p = new int[3];
                                string[] indices = line.Split(' ');
                                p[0] = int.Parse(indices[1]);
                                p[1] = int.Parse(indices[2]);
                                p[2] = int.Parse(indices[3]);
                                Triangle triangle = new Triangle();
                                // Index through our pool of vertices to find the ones that
                                // correspond to this face
                                // obj files use 1 indexing so our indices are off by 1
                                triangle.p[0] = verts[p[0] - 1];
                                triangle.p[1] = verts[p[1] - 1];
                                triangle.p[2] = verts[p[2] - 1];
                                tris.Add(triangle);
                            }
                        }
                        else
                        {
                            if (line[0] == 'f')
                            {
                                string[] indexPairs = line.Split(' ');
                                int[] p = new int[3];
                                int[] t = new int[3];
                                for (int i = 0; i < 3; i++)
                                {
                                    string[] pt = indexPairs[i + 1].Split('/');
                                    p[i] = int.Parse(pt[0]);
                                    t[i] = int.Parse(pt[1]);
                                }
                                Triangle triangle = new Triangle();
                                for (int i = 0; i < 3; i++)
                                {
                                    triangle.p[i] = verts[p[i] - 1];
                                    triangle.t[i] = texs[t[i] - 1];
                                }
                                tris.Add(triangle);
                            }

                        }
                    }
                }
                return true;
            }

            public bool LoadMaterialFromFile(string sFilename, ref string texturePath, ref bool hasTexture)
            {
                hasTexture = false;
                using (StreamReader sr = new StreamReader(sFilename))
                {
                    string line;
                    // Read lines from the file until the end of the file
                    // is reached.
                    while ((line = sr.ReadLine()) != null)
                    {
                        // catches empty lines
                        if (line.Length == 0) continue;

                        if (line[0] == 'm')
                        {
                            string[] indices = line.Split(' ');
                            texturePath = indices[1];
                            hasTexture = true;
                        }
                    }
                }
                return true;
            }
            public bool LoadTextureFromFile(string sFilename)
            {
                Texture = new DirectBitmap(sFilename);
                return true;
            }
        }

        class View
        {
            public View()
            { 
            }
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
            public float fYaw { get; set; } // FPS style camera rotation about the y-axis
            public float fPitch { get; set; } // FPS style camera rotation about the x-axis
            public float fTheta { get; set; } // Used to rotate the world            
            public int screenWidth { get; set; }
            public int screenHeight { get; set; }
            public int pixelWidth { get; set; }
            public int pixelHeight { get; set; }
            public float[] pDepthBuffer { get; set; } = new float[1];
            public DirectBitmap screen = new DirectBitmap(1, 1); // A bitmap representing the frame drawn to the picturebox

            public int Triangle_ClipAgainstPlane(Vec3d plane_p, Vec3d plane_n, ref Triangle in_tri, ref Triangle out_tri1, ref Triangle out_tri2)
            {
                // Make sure plane normal is indeed normal
                plane_n = Vector_Normalize(ref plane_n);

                // Return signed shortest distance from point to plane, plane normal must be normalized
                // Note no longer referenced
                Func<Vec3d, float> dist = (Vec3d p) =>
                {
                    Vec3d n = Vector_Normalize(ref p);
                    //Note changed this line
                    return (Vector_DotProduct(ref plane_n, ref p) - Vector_DotProduct(ref plane_n, ref plane_p));
                };

                // Create two temporary storage arrays to classify points either side of plane
                // If distance sign is positive, point lies on "inside" of plane
                Vec3d[] inside_points = new Vec3d[3] { new Vec3d(), new Vec3d(), new Vec3d() }; int nInsidePointCount = 0;
                Vec3d[] outside_points = new Vec3d[3] { new Vec3d(), new Vec3d(), new Vec3d() }; int nOutsidePointCount = 0;
                Vec2d[] inside_tex = new Vec2d[3] { new Vec2d(), new Vec2d(), new Vec2d() }; int nInsideTexCount = 0;
                Vec2d[] outside_tex = new Vec2d[3] { new Vec2d(), new Vec2d(), new Vec2d() }; int nOutsideTexCount = 0;


                // Get signed distance of each point in triangle to plane
                float d0 = dist(in_tri.p[0]);
                float d1 = dist(in_tri.p[1]);
                float d2 = dist(in_tri.p[2]);

                if (d0 >= 0) { inside_points[nInsidePointCount++] = in_tri.p[0]; inside_tex[nInsideTexCount++] = in_tri.t[0]; }
                else
                {
                    outside_points[nOutsidePointCount++] = in_tri.p[0]; outside_tex[nOutsideTexCount++] = in_tri.t[0];
                }
                if (d1 >= 0)
                {
                    inside_points[nInsidePointCount++] = in_tri.p[1]; inside_tex[nInsideTexCount++] = in_tri.t[1];
                }
                else
                {
                    outside_points[nOutsidePointCount++] = in_tri.p[1]; outside_tex[nOutsideTexCount++] = in_tri.t[1];
                }
                if (d2 >= 0)
                {
                    inside_points[nInsidePointCount++] = in_tri.p[2]; inside_tex[nInsideTexCount++] = in_tri.t[2];
                }
                else
                {
                    outside_points[nOutsidePointCount++] = in_tri.p[2]; outside_tex[nOutsideTexCount++] = in_tri.t[2];
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

                if (nInsidePointCount == 3)
                {
                    // All points lie on the inside of plane, so do nothing
                    // and allow the triangle to simply pass through
                    out_tri1 = in_tri;

                    return 1; // Just the one returned original triangle is valid
                }

                if (nInsidePointCount == 1 && nOutsidePointCount == 2)
                {
                    // Triangle should be clipped. As two points lie outside
                    // the plane, the triangle simply becomes a smaller triangle

                    // Copy appearance info to new triangle
                    out_tri1.col = in_tri.col;

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

                if (nInsidePointCount == 2 && nOutsidePointCount == 1)
                {
                    // Triangle should be clipped. As two points lie inside the plane,
                    // the clipped triangle becomes a "quad". Fortunately, we can
                    // represent a quad with two new triangles

                    // Copy appearance info to new triangles
                    out_tri1.col = in_tri.col;

                    out_tri2.col = in_tri.col;

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
                return 0; // remove later
            }

            public Color GetColour(float lum)
            {
                // converts the luminance back to RGB values
                return Color.FromArgb(255, (int)(lum * 255), (int)(lum * 255), (int)(lum * 255));

            }

            public void ConstructBitmap(int x1, int y1, float u1, float v1, float w1,
                int x2, int y2, float u2, float v2, float w2,
                int x3, int y3, float u3, float v3, float w3, DirectBitmap texture,
                int colorARGB = -1, bool hasTexture = false)
            {
                // Swaps the variables so that y1 < y2 < y3
                // y1 appears at the top of the screen
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

                // The pixel domain is integers. Can't move half a pixel
                //Variables represent the top short side of triangle
                int dy1 = y2 - y1;
                int dx1 = x2 - x1;
                float dv1 = v2 - v1;
                float du1 = u2 - u1;
                float dw1 = w2 - w1;

                // Variables represent the long side of triangle
                int dy2 = y3 - y1;
                int dx2 = x3 - x1;
                float dv2 = v3 - v1;
                float du2 = u3 - u1;
                float dw2 = w3 - w1;

                // Per pixel texel coordinates
                float tex_u, tex_v, tex_w;

                // Depicts how much we step in that unit direction per row of pixels
                float dax_step = 0, dbx_step = 0,
                    du1_step = 0, dv1_step = 0,
                    du2_step = 0, dv2_step = 0,
                    dw1_step = 0, dw2_step = 0;

                // as long as the line is not horizontal, dy will not be zero
                // how much we move per x coordinate
                if (dy1 != 0) dax_step = dx1 / MathF.Abs(dy1);
                if (dy2 != 0) dbx_step = dx2 / MathF.Abs(dy2);

                if (dy1 != 0) du1_step = du1 / MathF.Abs(dy1);
                if (dy1 != 0) dv1_step = dv1 / MathF.Abs(dy1);
                if (dy1 != 0) dw1_step = dw1 / MathF.Abs(dy1);

                if (dy2 != 0) du2_step = du2 / MathF.Abs(dy2);
                if (dy2 != 0) dv2_step = dv2 / MathF.Abs(dy2);
                if (dy2 != 0) dw2_step = dw2 / MathF.Abs(dy2);

                // As long as the line isn't flat, draw top half of triangle
                if (dy1 != 0)
                {
                    // for each row of pixels
                    for (int i = y1; i <= y2; i++)
                    {
                        // get start and endpoints horizontally
                        int ax = (int)(x1 + (i - y1) * dax_step);
                        int bx = (int)(x1 + (i - y1) * dbx_step);

                        // get start and end points of texel coordinates
                        float tex_su = u1 + (i - y1) * du1_step;
                        float tex_sv = v1 + (i - y1) * dv1_step;
                        float tex_sw = w1 + (i - y1) * dw1_step;

                        float tex_eu = u1 + (i - y1) * du2_step;
                        float tex_ev = v1 + (i - y1) * dv2_step;
                        float tex_ew = w1 + (i - y1) * dw2_step;
                        // ensure ax and it's related components are the start
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

                        // for each pixel in the row
                        for (int j = ax; j < bx; j++)
                        {
                            // position in texel space for each pixel we cross
                            tex_u = (1.0f - t) * tex_su + t * tex_eu;
                            tex_v = (1.0f - t) * tex_sv + t * tex_ev;
                            tex_w = (1.0f - t) * tex_sw + t * tex_ew;

                            if (tex_w > pDepthBuffer[i * screenWidth + j])
                            {
                                // divide by tex_w to get un-normalized texel coordinate
                                // scale up texel coordinates to the height and width of the textures
                                int w = (int)((tex_u / tex_w) * texture.Width - 0.5);
                                int h = (int)((tex_v / tex_w) * texture.Height - 0.5);
                                if (!hasTexture)
                                {
                                    screen.SetPixel(j, i, Color.FromArgb(colorARGB));
                                }
                                else
                                {
                                    screen.SetPixel(j, i, texture.GetPixel(w, h));
                                }
                                pDepthBuffer[i * screenWidth + j] = tex_w;
                            }
                            // increase t each time we get to a new pixel
                            t += tstep;
                        }

                    }
                }
                // recalculate values to work with the bottom left short side of the triangle
                dy1 = y3 - y2;
                dx1 = x3 - x2;
                dv1 = v3 - v2;
                du1 = u3 - u2;
                dw1 = w3 - w2;

                if (dy1 != 0) dax_step = dx1 / MathF.Abs(dy1);
                if (dy2 != 0) dbx_step = dx2 / MathF.Abs(dy2);

                du1_step = 0; dv1_step = 0;
                if (dy1 != 0) du1_step = du1 / MathF.Abs(dy1);
                if (dy1 != 0) dv1_step = dv1 / MathF.Abs(dy1);
                if (dy1 != 0) dw1_step = dw1 / MathF.Abs(dy1);

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

                            if (tex_w > pDepthBuffer[i * screenWidth + j])
                            {
                                int w = (int)((tex_u / tex_w) * texture.Width - 0.5);
                                int h = (int)((tex_v / tex_w) * texture.Height - 0.5);
                                if (!hasTexture)
                                {
                                    screen.SetPixel(j, i, Color.FromArgb(colorARGB));
                                }
                                else
                                {
                                    screen.SetPixel(j, i, texture.GetPixel(w, h));
                                }
                                pDepthBuffer[i * screenWidth + j] = tex_w;
                            }
                            t += tstep;
                        }
                    }
                }
            }

        }

        // https://stackoverflow.com/questions/24701703/c-sharp-faster-alternatives-to-setpixel-and-getpixel-for-bitmaps-for-windows-f
        public class DirectBitmap : IDisposable
        {
            public Bitmap Bitmap { get; private set; }
            public Int32[] Bits { get; private set; }
            public bool Disposed { get; private set; }
            public int Height { get; private set; }
            public int Width { get; private set; }

            protected GCHandle BitsHandle { get; private set; }

            public DirectBitmap() { }

            public DirectBitmap(int width, int height)
            {
                Width = width;
                Height = height;
                Bits = new Int32[width * height];
                BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
                Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
            }

            public DirectBitmap(int width, int height, Color color)
            {
                Width = width;
                Height = height;
                Bits = new Int32[width * height];
                for (int i = 0; i < width * height; i++)
                {
                    Bits[i] = color.ToArgb();
                }
                BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
                Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
            }

            public DirectBitmap(string filePath)
            {
                Bitmap bitmap = new Bitmap(filePath);
                Width = bitmap.Width;
                Height = bitmap.Height;
                Bits = new Int32[Width * Height];
                BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
                Bitmap = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());

                for (int h = 0; h < Height; h++)
                {
                    for (int w = 0; w < Width; w++)
                    {
                        Bits[w + h * Width] = bitmap.GetPixel(w, h).ToArgb();
                    }
                }
            }

            public void SetPixel(int x, int y, Color color)
            {
                int index = x + (y * Width);
                int col = color.ToArgb();
                if (index >= Bits.Length)
                {
                    return;
                }
                Bits[index] = col;
            }

            public Color GetPixel(int x, int y)
            {
                int index = x + (y * Width);
                int color = Bits[index];
                Color result = Color.FromArgb(color);

                return result;
            }

            public void Dispose()
            {
                if (Disposed) return;
                Disposed = true;
                Bitmap.Dispose();
                BitsHandle.Free();
            }

        }



        private void Form1_Load(object sender, EventArgs e)
        {
            // this.DoubleBuffered = true; // may not be needed since picturebox is already double buffered
            stopWatch.Start();
            Clock.Interval = 20;
            Clock.Enabled = true;
            world = new View(Viewer.Width, Viewer.Height, 1, 1);
            // Setup Projection Matrix
            world.matProj = Matrix_MakeProjection(90.0f, (float)world.screenHeight / (float)world.screenWidth, 0.1f, 1000.0f);
        }

        private void Clock_Tick(object sender, EventArgs e)
        {
            // https://github.com/OneLoneCoder/olcPixelGameEngine/blob/147c25a018c917030e59048b5920c269ef583c50/olcPixelGameEngine.h#L3823
            // Runs paint event. Renders frame
            tock = (float)stopWatch.Elapsed.TotalSeconds;
            fElapsedTime = tock - tick;
            tick = tock;
            Viewer.Refresh();
            frames += 1;
            frameTime += fElapsedTime;
            if (frameTime >= 1.0f)
            {
                FPS.Text = $"FPS: {frames}";
                frames = 0;
                frameTime -= 1; // Possibly change to set to 0
            }
            label1.Text = $"Frame: {frames}";
            label2.Text = $"ELapsed time: {stopWatch.Elapsed.TotalSeconds}";
            // sw.Restart();
        }


        private void Viewer_Paint(object sender, PaintEventArgs e)
        {
            // called once per frame
            // e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            if (keyPressed[Keys.Up])
                world.vCamera.y += 8.0f * fElapsedTime;   // Travel along positive y-axis

            if (keyPressed[Keys.Down])
                world.vCamera.y -= 8.0f * fElapsedTime;  // Travel along negative y-axis

            if (keyPressed[Keys.Left])
                world.vCamera.x -= 8.0f * fElapsedTime;   // Travel along positive x-axis

            if (keyPressed[Keys.Right])
                world.vCamera.x += 8.0f * fElapsedTime;   // Travel along negative x-axis

            // A velocity vector used to control the forward movement of the camera
            Vec3d vVelocity = world.vLookDir * (8.0f * fElapsedTime);

            // Standard FPS Control scheme, but turn instead of strafe
            if (keyPressed[Keys.W])
                world.vCamera = world.vCamera + vVelocity;

            if (keyPressed[Keys.S])
                world.vCamera = world.vCamera - vVelocity;

            if (keyPressed[Keys.A])
                world.fYaw -= 2.0f * fElapsedTime;

            if (keyPressed[Keys.D])
                world.fYaw += 2.0f * fElapsedTime;

            if (keyPressed[Keys.R])
                world.fPitch -= 2.0f * fElapsedTime;

            if (keyPressed[Keys.F])
                world.fPitch += 2.0f * fElapsedTime;


            // Set up "World Transform"
            Mat4x4 matRotZ, matRotX;
            /*world.fTheta += 1.0f * fElapsedTime;*/  // Spins the world around
            matRotZ = Matrix_MakeRotationZ(world.fTheta * 0.5f);
            matRotX = Matrix_MakeRotationX(world.fTheta);

            Mat4x4 matTrans;
            // Offsets the world
            matTrans = Matrix_MakeTranslation(0.0f, 0.0f, 5.0f);

            // Transforms must be done with scaling first, then x, y, and z rotations, then 
            // translations

            Mat4x4 matWorld;
            matWorld = Matrix_MakeIdentity(); // Form World Matrix
            matWorld =  matRotZ * matRotX; // Transform by rotation
            matWorld = matWorld * matTrans; // Transform by translation

            // Create "Point At" Matrix for camera
            Vec3d vUp = new Vec3d(0, 1, 0); // Default up direction for the camera is along the positive y-axis
            Vec3d vForwardCam = new Vec3d(0, 0, 1); // Default forward direction for our camera is along the positive z-axis
            // Cap pitch from being able to rotate too far.
            // TODO: Fix this
            world.fPitch = world.fPitch > 0 ? MathF.Min(3.1415f / 2, world.fPitch) : MathF.Max(-3.1415f / 2, world.fPitch);
            Mat4x4 matCameraRot = Matrix_MakeIdentity();
            Mat4x4 matCameraRot_X = Matrix_MakeRotationX(world.fPitch);
            Mat4x4 matCameraRot_Y = Matrix_MakeRotationY(world.fYaw);
            matCameraRot = matCameraRot_X * matCameraRot_Y;
            // This rotated forward vector becomes the camera's look direction
            world.vLookDir = matCameraRot * vForwardCam;
            // Offset look direction to the camera location to get the target the camera points at
            Vec3d vTarget = world.vCamera + world.vLookDir;
            // Construct the "Point At" matrix
            Mat4x4 matCamera = Matrix_PointAt(ref world.vCamera, ref vTarget, ref vUp);

            // Construct the "Look At" matrix from the inverse
            Mat4x4 matView = Matrix_QuickInverse(ref matCamera);



            // Dispose of the screen bitmap each frame. Memory must be freed each time
            world.screen.Dispose();
            // clears the screen each frame
            world.screen = new DirectBitmap(world.screenWidth, world.screenHeight, Color.Cyan);

            // Clear Depth Buffer
            for (int i = 0; i < world.screenWidth * world.screenHeight; i++)
            {
                world.pDepthBuffer[i] = 0.0f;
            }

            // Draw Triangles
            // Note: tris are passed by reference here
            foreach (Mesh mesh in meshes)
            {
                // Store triangles for rastering later
                List<Triangle> vecTrianglesToRaster = new List<Triangle>();

                foreach (Triangle tri in mesh.tris)
                {
                    Triangle triTransformed = new Triangle();
                    // World Matrix Transform
                    triTransformed.p[0] = matWorld * tri.p[0];
                    triTransformed.p[1] = matWorld * tri.p[1];
                    triTransformed.p[2] = matWorld * tri.p[2];
                    // texture information stays the same
                    triTransformed.t[0] = tri.t[0];
                    triTransformed.t[1] = tri.t[1];
                    triTransformed.t[2] = tri.t[2];


                    // Calculate triangle's Normal 
                    Vec3d normal = new Vec3d(), line1 = new Vec3d(), line2 = new Vec3d();

                    // Get lines on either side of triangle
                    line1 = triTransformed.p[1] - triTransformed.p[0];
                    line2 = triTransformed.p[2] - triTransformed.p[0];

                    // Take the cross product of lines to get normal to triangle surface
                    normal = Vector_CrossProduct(ref line1, ref line2);
                    normal = Vector_Normalize(ref normal);

                    // Get Ray from camera to triangle
                    Vec3d vCameraRay = triTransformed.p[0] - world.vCamera;

                    // if ray is aligned with normal then triangle is visible
                    // if not it is culled
                    // right now this means triangles on the other side of an object we won't see
                    // so if we could see through one side of the object, we wouldn't the inside of the
                    // other side as it'd be culled. Note, Make toggleable
                    // if (Vector_DotProduct(ref normal, ref vCameraRay) < 0.0f)
                    {
                        // Illumination
                        Vec3d light_direction = new Vec3d(0.0f, 1.0f, -1.0f);
                        light_direction = Vector_Normalize(ref light_direction);

                        // How "aligned" are light direction and triangle surface normal?
                        float dp = MathF.Max(0.1f, Vector_DotProduct(ref light_direction, ref normal));

                        // Choose triangle colours as required
                        // The less the triangle normal and the light direction are aligned
                        // the dimmer the triangle
                        triTransformed.col = world.GetColour(dp);

                        // convert World Space --> View Space
                        Triangle triViewed = new Triangle();
                        triViewed.p[0] = matView * triTransformed.p[0];
                        triViewed.p[1] = matView * triTransformed.p[1];
                        triViewed.p[2] = matView * triTransformed.p[2];
                        triViewed.col = triTransformed.col;
                        // Texture information is still not updated
                        triViewed.t[0] = triTransformed.t[0];
                        triViewed.t[1] = triTransformed.t[1];
                        triViewed.t[2] = triTransformed.t[2];


                        // Clip the Viewed Triangle against near plane, this could form two additional
                        // triangles.
                        int nClippedTriangles = 0;
                        Triangle[] clipped = new Triangle[2] { new Triangle(), new Triangle() };
                        nClippedTriangles = world.Triangle_ClipAgainstPlane(new Vec3d(0.0f, 0.0f, 0.1f), new Vec3d(0.0f, 0.0f, 1.0f), ref triViewed, ref clipped[0], ref clipped[1]);

                        // We may end up with multiple triangles form the clip, so project as
                        // required
                        for (int n = 0; n < nClippedTriangles; n++)
                        {
                            // Project triangles from 3D --> 2D
                            // View space -> screen space
                            Triangle triProjected = new Triangle();
                            triProjected.p[0] = world.matProj * clipped[n].p[0];
                            triProjected.p[1] = world.matProj * clipped[n].p[1];
                            triProjected.p[2] = world.matProj * clipped[n].p[2];
                            triProjected.col = clipped[n].col;
                            triProjected.t[0] = clipped[n].t[0];
                            triProjected.t[1] = clipped[n].t[1];
                            triProjected.t[2] = clipped[n].t[2];

                            // divide the texture coordinates by z-component to add perspective
                            triProjected.t[0].u = triProjected.t[0].u / triProjected.p[0].w;
                            triProjected.t[1].u = triProjected.t[1].u / triProjected.p[1].w;
                            triProjected.t[2].u = triProjected.t[2].u / triProjected.p[2].w;

                            triProjected.t[0].v = triProjected.t[0].v / triProjected.p[0].w;
                            triProjected.t[1].v = triProjected.t[1].v / triProjected.p[1].w;
                            triProjected.t[2].v = triProjected.t[2].v / triProjected.p[2].w;

                            // set texel depth to be inverse so we can get the un-normalized coordinates 
                            // back
                            triProjected.t[0].w = 1.0f / triProjected.p[0].w;
                            triProjected.t[1].w = 1.0f / triProjected.p[1].w;
                            triProjected.t[2].w = 1.0f / triProjected.p[2].w;

                            // each vertex is divided by the z-component to add perspective
                            triProjected.p[0] = triProjected.p[0] / triProjected.p[0].w;
                            triProjected.p[1] = triProjected.p[1] / triProjected.p[1].w;
                            triProjected.p[2] = triProjected.p[2] / triProjected.p[2].w;

                            // We must invert X because our system uses a left-hand coordinate system	
                            triProjected.p[0].x *= -1.0f;
                            triProjected.p[1].x *= -1.0f;
                            triProjected.p[2].x *= -1.0f;
                            // We must invert Y because pixels are drawn top down
                            triProjected.p[0].y *= -1.0f;
                            triProjected.p[1].y *= -1.0f;
                            triProjected.p[2].y *= -1.0f;

                            // Projection Matrix gives results from -1 to +1 through dividing by Z
                            // So we offset vertices to fit on our screen that goes from 0 to height or width
                            Vec3d vOffsetView = new Vec3d(1, 1, 0);
                            triProjected.p[0] = triProjected.p[0] + vOffsetView;
                            triProjected.p[1] = triProjected.p[1] + vOffsetView;
                            triProjected.p[2] = triProjected.p[2] + vOffsetView;

                            // vertices are now between 0 and 2 so we scale into viewable
                            // screen height and width
                            triProjected.p[0].x *= 0.5f * world.screenWidth;
                            triProjected.p[0].y *= 0.5f * world.screenHeight;
                            triProjected.p[1].x *= 0.5f * world.screenWidth;
                            triProjected.p[1].y *= 0.5f * world.screenHeight;
                            triProjected.p[2].x *= 0.5f * world.screenWidth;
                            triProjected.p[2].y *= 0.5f * world.screenHeight;

                            // Store triangle for sorting
                            vecTrianglesToRaster.Add(triProjected);


                        }
                    }
                }

                //Sort triangles from back to front
                // An approximation of a triangle's Z position
                // Causes problems if each vertex is far in terms of Z position
                // Useful for transparency
                //vecTrianglesToRaster.Sort((Triangle t1, Triangle t2) =>
                //{
                //    float z1 = (t1.p[0].z + t1.p[1].z + t1.p[2].z) / 3.0f;
                //    float z2 = (t2.p[0].z + t2.p[1].z + t2.p[2].z) / 3.0f;
                //    if (z2 - z1 > 0)
                //    {
                //        return 1;
                //    }
                //    else if (z1 - z2 == 0)
                //    {
                //        return 0;
                //    }
                //    else
                //    {
                //        return -1;
                //    }
                //});


                // Loop through all transformed, viewed, projected, and sorted triangles
                foreach (Triangle triToRaster in vecTrianglesToRaster)
                {
                    // Clip triangles against all four screen edges, this could yield
                    // a bunch of triangles, so create a queue that we traverse to 
                    //  ensure we only test new triangles generated against planes
                    Triangle[] clipped = new Triangle[2] { new Triangle(), new Triangle() };
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
                            switch (p)
                            {
                                case 0:
                                    nTrisToAdd = world.Triangle_ClipAgainstPlane(new Vec3d(0.0f, 0.0f, 0.0f), new Vec3d(0.0f, 1.0f, 0.0f), ref test, ref clipped[0], ref clipped[1]);
                                    break;
                                case 1:
                                    nTrisToAdd = world.Triangle_ClipAgainstPlane(new Vec3d(0.0f, (float)world.screenHeight - 1, 0.0f), new Vec3d(0.0f, -1.0f, 0.0f), ref test, ref clipped[0], ref clipped[1]);
                                    break;
                                case 2:
                                    nTrisToAdd = world.Triangle_ClipAgainstPlane(new Vec3d(0.0f, 0.0f, 0.0f), new Vec3d(1.0f, 0.0f, 0.0f), ref test, ref clipped[0], ref clipped[1]);
                                    break;
                                case 3:
                                    nTrisToAdd = world.Triangle_ClipAgainstPlane(new Vec3d((float)world.screenWidth - 1, 0.0f, 0.0f), new Vec3d(-1.0f, 0.0f, 0.0f), ref test, ref clipped[0], ref clipped[1]);
                                    break;
                            }

                            // Clipping may yield a variable number of triangles, so
                            // add these new ones to the back of the queue for subsequent
                            // clipping against next planes
                            for (int w = 0; w < nTrisToAdd; w++)
                                listTriangles.Enqueue(new Triangle(ref clipped[w]));
                        }
                        nNewTriangles = listTriangles.Count();
                    }


                    // Draw the transformed, viewed, clipped, projected, sorted, clipped triangles to a bitmap
                    foreach (Triangle t in listTriangles)
                    {
                        world.ConstructBitmap((int)t.p[0].x, (int)t.p[0].y, t.t[0].u, t.t[0].v, t.t[0].w,
                            (int)t.p[1].x, (int)t.p[1].y, t.t[1].u, t.t[1].v, t.t[1].w,
                            (int)t.p[2].x, (int)t.p[2].y, t.t[2].u, t.t[2].v, t.t[2].w, mesh.Texture, t.col.ToArgb(), mesh.hasTexture);


                        //// Handles wireframe. No depth buffer currently emulated for wireframe so you can see
                        //// it through triangles
                        //DrawLine((int)t.p[0].x, (int)t.p[0].y, (int)t.p[1].x, (int)t.p[1].y, world.screen, Color.Black);
                        //DrawLine((int)t.p[0].x, (int)t.p[0].y, (int)t.p[2].x, (int)t.p[2].y, world.screen, Color.Black);
                        //DrawLine((int)t.p[1].x, (int)t.p[1].y, (int)t.p[2].x, (int)t.p[2].y, world.screen, Color.Black);

                    }
                }
            }


            // Draw the bitmap to the screen
            e.Graphics.DrawImage(world.screen.Bitmap, 0, 0, world.pixelWidth * world.screenWidth, world.pixelHeight * world.screenHeight);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            keyPressed[e.KeyCode] = true;
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            keyPressed[e.KeyCode] = false;
        }

        //https://rosettacode.org/wiki/Bitmap/Bresenham%27s_line_algorithm#C#
        private void DrawLine(int x0, int y0, int x1, int y1, DirectBitmap bitmap, Color color)
        {
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = (dx > dy ? dx : -dy) / 2, e2;
            for (; ; )
            {
                bitmap.SetPixel(x0, y0, color);
                if (x0 == x1 && y0 == y1) break;
                e2 = err;
                if (e2 > -dx) { err -= dy; x0 += sx; }
                if (e2 < dy) { err += dx; y0 += sy; }
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "All files (*.*)|*.*|obj files (*.obj)|*.obj";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = false;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    meshes.Clear();

                    //Get the path of specified file
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
            // creates the cube
            Vec3d[] points = new Vec3d[8];
            Mesh cube = new Mesh();
            for (int i = 0; i < 8; i++)
            {
                points[i] = new Vec3d(i / 4, (i / 2) % 2, i % 2);
            }

            List<Triangle> tris = new List<Triangle>
            {
                new Triangle(new Vec3d[] { points[1], points[3], points[2] }),
                 new Triangle(new Vec3d[] { points[1], points[2], points[0] }),
                  new Triangle(new Vec3d[] { points[0], points[2], points[6] }),
                  new Triangle(new Vec3d[] { points[0], points[6], points[4] }),
                  new Triangle(new Vec3d[] { points[1], points[0], points[4] }),
                  new Triangle(new Vec3d[] { points[1], points[4], points[5] }),
                  new Triangle(new Vec3d[] { points[4], points[6], points[7] }),
                  new Triangle(new Vec3d[] { points[4], points[7], points[5] }),
                  new Triangle(new Vec3d[] { points[2], points[3], points[7] }),
                  new Triangle(new Vec3d[] { points[2], points[7], points[6] }),
                  new Triangle(new Vec3d[] { points[5], points[7], points[3] }),
                  new Triangle(new Vec3d[] { points[5], points[3], points[1] }),
            };
            cube.tris = tris;
            meshes.Add(cube);
        }

        // Necessary for keydown event to trigger for arrow keys
        private void AddCube_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            e.IsInputKey = true;
        }

        private void saveASToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(meshes.Count != 0) 
            {
                Stream myStream;
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                saveFileDialog1.Filter = "obj files (*.obj)|*.obj|All files (*.*)|*.*";
                saveFileDialog1.FilterIndex = 2;
                saveFileDialog1.RestoreDirectory = false;


                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter outputFile = new StreamWriter(saveFileDialog1.OpenFile()))
                    {
                        outputFile.WriteLine("# test");
                        foreach (Mesh mesh in meshes)
                        {
                            Dictionary<Vec3d, int> vertices = new Dictionary<Vec3d, int>();
                            List<int[]> faces = new List<int[]>();
                            int index = 1;

                            foreach(Triangle tri in mesh.tris)
                            {
                                int[] face = new int[3]; 

                                for (int i = 0; i < 3; i++)
                                {
                                    if (vertices.ContainsKey(tri.p[i]))
                                    {
                                        face[i] = vertices[tri.p[i]];
                                    }
                                    else
                                    {
                                        vertices[tri.p[i]] = index;
                                        face[i] = index++;
                                    }
                                }
                                faces.Add(face);                                
                            }
                            foreach (KeyValuePair<Vec3d, int> vertex in vertices)
                            {
                                string output = $"v {vertex.Key.x:f6} {vertex.Key.y:f6} {vertex.Key.z:f6}";
                                outputFile.WriteLine(output);
                            }
                            foreach (int[] face in faces)
                            {
                                string output = $"f {face[0]} {face[1]} {face[2]}";
                                outputFile.WriteLine(output);
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
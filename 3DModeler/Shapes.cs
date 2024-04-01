using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static _3DModeler.Operations;

namespace _3DModeler
{
    // A 2D structure to hold texture coordinates
    // Perhaps replace with one from System.Numerics
    public struct Vec2D
    {
        public Vec2D() { }

        public Vec2D(float u, float v)
        {
            this.u = u;
            this.v = v;
        }
        public Vec2D(Vec2D vec2D)
        {
            this.u = vec2D.u;
            this.v = vec2D.v;
            this.w = vec2D.w;
        }

        public float u = 0;
        public float v = 0;
        public float w = 1; // Keeps track of the depth of each texture coordinate

        public bool Equals(Vec2D p)
        {
            return this.u == p.u && this.v == p.v;
        }
    }

    // A 3D structure to hold vertex coordinates
    public struct Vec3D
    {
        public Vec3D() { }

        public Vec3D(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public Vec3D(Vec3D vec3D)
        {
            this.w = vec3D.w;
            this.x = vec3D.x;
            this.y = vec3D.y;
            this.z = vec3D.z;
        }
        public static Vec3D operator +(Vec3D lhs, Vec3D rhs)
        {
            Vec3D vec3D = new Vec3D
            {
                x = lhs.x + rhs.x,
                y = lhs.y + rhs.y,
                z = lhs.z + rhs.z
            };
            return vec3D;
        }
        public static Vec3D operator +(Vec3D lhs, float rhs)
        {
            Vec3D vec3D = new Vec3D
            {
                x = lhs.x + rhs,
                y = lhs.y + rhs,
                z = lhs.z + rhs
            };
            return vec3D;
        }
        public static Vec3D operator -(Vec3D vec)
        {
            Vec3D vec3D = new Vec3D
            {
                x = -vec.x,
                y = -vec.y,
                z = -vec.z
            };
            return vec3D;
        }
        public static Vec3D operator -(Vec3D lhs, Vec3D rhs)
        {
            Vec3D vec3D = new Vec3D
            {
                x = lhs.x - rhs.x,
                y = lhs.y - rhs.y,
                z = lhs.z - rhs.z
            };
            return vec3D;
        }
        public static Vec3D operator -(Vec3D lhs, float rhs)
        {
            Vec3D vec3D = new Vec3D
            {
                x = lhs.x - rhs,
                y = lhs.y - rhs,
                z = lhs.z - rhs
            };
            return vec3D;
        }
        public static Vec3D operator *(Vec3D lhs, float rhs)
        {
            Vec3D vec3D = new Vec3D
            {
                x = lhs.x * rhs,
                y = lhs.y * rhs,
                z = lhs.z * rhs
            };
            return vec3D;
        }
        public static Vec3D operator /(Vec3D lhs, float rhs)
        {
            Vec3D vec3D = new Vec3D
            {
                x = lhs.x / rhs,
                y = lhs.y / rhs,
                z = lhs.z / rhs
            };
            return vec3D;
        }

        public float x = 0;
        public float y = 0;
        public float z = 0;
        public float w = 1; // 4th term is needed for vector multiplication

        public bool Equals(Vec3D p)
        {
            return this.x == p.x && this.y == p.y && this.z == p.z;
        }
    }

    // A matrix structure used for many vector transformations
    public struct Mat4x4
    {
        public Mat4x4() { }
        public static Vec3D operator *(Vec3D lhs, Mat4x4 rhs)
        {
            Vec3D v = new Vec3D
            {
                x = lhs.x * rhs.m[0, 0] + lhs.y * rhs.m[1, 0] + lhs.z * rhs.m[2, 0] + lhs.w * rhs.m[3, 0],
                y = lhs.x * rhs.m[0, 1] + lhs.y * rhs.m[1, 1] + lhs.z * rhs.m[2, 1] + lhs.w * rhs.m[3, 1],
                z = lhs.x * rhs.m[0, 2] + lhs.y * rhs.m[1, 2] + lhs.z * rhs.m[2, 2] + lhs.w * rhs.m[3, 2],
                w = lhs.x * rhs.m[0, 3] + lhs.y * rhs.m[1, 3] + lhs.z * rhs.m[2, 3] + lhs.w * rhs.m[3, 3]
            };
            return v;
        }
        public static Mat4x4 operator *(Mat4x4 lhs, Mat4x4 rhs)
        {
            Mat4x4 matrix = new Mat4x4();
            for (int c = 0; c < 4; c++)
                for (int r = 0; r < 4; r++)
                    matrix.m[r, c] = lhs.m[r, 0] * rhs.m[0, c] + lhs.m[r, 1] * rhs.m[1, c] + lhs.m[r, 2] * rhs.m[2, c] + lhs.m[r, 3] * rhs.m[3, c];
            return matrix;
        }

        public float[,] m = new float[4, 4];
    }

    // A structure used for storing face information for a 3 vertex polygon
    public struct Triangle
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
    public struct Quadrilateral
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
    public struct Mesh
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
    public struct Material
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
}

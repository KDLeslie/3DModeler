using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static _3DModeler.Operations;


namespace _3DModeler
{
    internal static class Operations
    {
        // A 2D structure to hold texture coordinates
        public struct Vec2d
        {
            public Vec2d() { }

            public Vec2d(float u, float v) 
            { 
                this.u = u;
                this.v = v;
            }
            public Vec2d(Vec2d vec2D)
            {
                this.u = vec2D.u;
                this.v = vec2D.v;
                this.w = vec2D.w;
            }
            public bool Equals(Vec2d p)
            {
                // Return true if the fields match.
                // Note that the base class is not invoked because it is
                // System.Object, which defines Equals as reference equality.
                return this.u == p.u && this.v == p.v;
            }
            public float u = 0;
            public float v = 0;
            public float w = 1; // Keeps track of the depth of each texture coordinate

        }

        // A 3D structure to hold vertex coordinates
        public struct Vec3d
        {
            public Vec3d() { }

            public Vec3d(float x, float y, float z)
            {
                this.x = x;
                this.y = y; 
                this.z = z;
            }
            public Vec3d(Vec3d vec3D)
            {
                this.w = vec3D.w;
                this.x = vec3D.x;
                this.y = vec3D.y;
                this.z = vec3D.z;
            }
            public static Vec3d operator +(Vec3d lhs, Vec3d rhs)
            {
                Vec3d vec3D = new Vec3d
                {
                    x = lhs.x + rhs.x,
                    y = lhs.y + rhs.y,
                    z = lhs.z + rhs.z
                };
                return vec3D;
            }
            public static Vec3d operator +(Vec3d lhs, float rhs)
            {
                Vec3d vec3D = new Vec3d
                {
                    x = lhs.x + rhs,
                    y = lhs.y + rhs,
                    z = lhs.z + rhs
                };
                return vec3D;
            }
            public static Vec3d operator -(Vec3d lhs, Vec3d rhs)
            {
                Vec3d vec3D = new Vec3d
                {
                    x = lhs.x - rhs.x,
                    y = lhs.y - rhs.y,
                    z = lhs.z - rhs.z
                };
                return vec3D;
            }
            public static Vec3d operator -(Vec3d lhs, float rhs)
            {
                Vec3d vec3D = new Vec3d
                {
                    x = lhs.x - rhs,
                    y = lhs.y - rhs,
                    z = lhs.z - rhs
                };
                return vec3D;
            }
            public static Vec3d operator *(Vec3d lhs, float rhs)
            {
                Vec3d vec3D = new Vec3d
                {
                    x = lhs.x * rhs,
                    y = lhs.y * rhs,
                    z = lhs.z * rhs
                };
                return vec3D;
            }
            public static Vec3d operator /(Vec3d lhs, float rhs)
            {
                Vec3d vec3D = new Vec3d
                {
                    x = lhs.x / rhs,
                    y = lhs.y / rhs,
                    z = lhs.z / rhs
                };
                return vec3D;
            }
            
            public bool Equals(Vec3d p)
            {
                // Return true if the fields match.
                // Note that the base class is not invoked because it is
                // System.Object, which defines Equals as reference equality.
                return this.x == p.x && this.y == p.y && this.z == p.z;
            }

            public float x = 0;
            public float y = 0;
            public float z = 0;
            public float w = 1; // 4th term is needed for vector multiplication
        }

        public struct Mat4x4
        {
            public Mat4x4() { }
            public static Vec3d operator *(Mat4x4 lhs, Vec3d rhs)
            {
                Vec3d v = new Vec3d
                {
                    x = rhs.x * lhs.m[0, 0] + rhs.y * lhs.m[1, 0] + rhs.z * lhs.m[2, 0] + rhs.w * lhs.m[3, 0],
                    y = rhs.x * lhs.m[0, 1] + rhs.y * lhs.m[1, 1] + rhs.z * lhs.m[2, 1] + rhs.w * lhs.m[3, 1],
                    z = rhs.x * lhs.m[0, 2] + rhs.y * lhs.m[1, 2] + rhs.z * lhs.m[2, 2] + rhs.w * lhs.m[3, 2],
                    w = rhs.x * lhs.m[0, 3] + rhs.y * lhs.m[1, 3] + rhs.z * lhs.m[2, 3] + rhs.w * lhs.m[3, 3]
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

        public static Mat4x4 Matrix_MakeIdentity()
        {
            Mat4x4 matrix = new Mat4x4();
            matrix.m[0,0] = 1.0f;
            matrix.m[1,1] = 1.0f;
            matrix.m[2,2] = 1.0f;
            matrix.m[3,3] = 1.0f;
            return matrix;
        }

        public static Mat4x4 Matrix_MakeRotationX(float fAngleRad)
        {
            Mat4x4 matrix = new Mat4x4();
            matrix.m[0,0] = 1.0f;
            matrix.m[1,1] = MathF.Cos(fAngleRad);
            matrix.m[1,2] = MathF.Sin(fAngleRad);
            matrix.m[2,1] = -MathF.Sin(fAngleRad);
            matrix.m[2,2] = MathF.Cos(fAngleRad);
            matrix.m[3,3] = 1.0f;
            return matrix;
        }

        public static Mat4x4 Matrix_MakeRotationY(float fAngleRad)
        {
            Mat4x4 matrix = new Mat4x4();
            matrix.m[0,0] = MathF.Cos(fAngleRad);
            matrix.m[0,2] = MathF.Sin(fAngleRad);
            matrix.m[2,0] = -MathF.Sin(fAngleRad);
            matrix.m[1,1] = 1.0f;
            matrix.m[2,2] = MathF.Cos(fAngleRad);
            matrix.m[3,3] = 1.0f;
            return matrix;
        }

        public static Mat4x4 Matrix_MakeRotationZ(float fAngleRad)
        {
            Mat4x4 matrix = new Mat4x4();
            matrix.m[0,0] = MathF.Cos(fAngleRad);
            matrix.m[0,1] = MathF.Sin(fAngleRad);
            matrix.m[1,0] = -MathF.Sin(fAngleRad);
            matrix.m[1,1] = MathF.Cos(fAngleRad);
            matrix.m[2,2] = 1.0f;
            matrix.m[3,3] = 1.0f;
            return matrix;
        }

        public static Mat4x4 Matrix_MakeTranslation(float x, float y, float z)
        {
            Mat4x4 matrix = new Mat4x4();
            matrix.m[0,0] = 1.0f;
            matrix.m[1,1] = 1.0f;
            matrix.m[2,2] = 1.0f;
            matrix.m[3,3] = 1.0f;
            matrix.m[3,0] = x;
            matrix.m[3,1] = y;
            matrix.m[3,2] = z;
            return matrix;
        }

        public static Mat4x4 Matrix_MakeProjection(float fFovDegrees, float fAspectRatio, float fNear, float fFar)
        {
            // Aspect ratio prevents stretching when screen height and width vary
            // Field of view effectively zooms in or out when decreased or increased respectively
            // Near and Far planes represent the top and bottom of the viewing frustrum in positive z direction
            float fFovRad = 1.0f / MathF.Tan(fFovDegrees * 0.5f / 180.0f * MathF.PI);
            Mat4x4 matrix = new Mat4x4();
            matrix.m[0,0] = fAspectRatio * fFovRad;
            matrix.m[1,1] = fFovRad;
            matrix.m[2,2] = fFar / (fFar - fNear);
            matrix.m[3,2] = (-fFar * fNear) / (fFar - fNear);
            matrix.m[2,3] = 1.0f;
            matrix.m[3,3] = 0.0f;
            return matrix;
        }

        public static Mat4x4 Matrix_PointAt(ref Vec3d pos, ref Vec3d target, ref Vec3d up)
        {
            // calculate new forward direction
            Vec3d newForward = target - pos;
            newForward = Vector_Normalize(ref newForward);

            // Calculate new Up direction incase foward vector has a y-component
            Vec3d a = newForward * Vector_DotProduct(ref up, ref newForward);
            Vec3d newUp = up - a;
            newUp = Vector_Normalize(ref newUp);

            // New Right direction is easy, its just cross product
            Vec3d newRight = Vector_CrossProduct(ref newUp, ref newForward);

            // Construct Dimensioning and Translation Matrix	
            Mat4x4 matrix = new Mat4x4();
            matrix.m[0,0] = newRight.x; matrix.m[0,1] = newRight.y; matrix.m[0,2] = newRight.z; matrix.m[0,3] = 0.0f;
            matrix.m[1,0] = newUp.x; matrix.m[1,1] = newUp.y; matrix.m[1,2] = newUp.z; matrix.m[1,3] = 0.0f;
            matrix.m[2,0] = newForward.x; matrix.m[2,1] = newForward.y; matrix.m[2,2] = newForward.z; matrix.m[2,3] = 0.0f;
            matrix.m[3,0] = pos.x; matrix.m[3,1] = pos.y; matrix.m[3,2] = pos.z; matrix.m[3,3] = 1.0f;
            return matrix;
        }

        public static Mat4x4 Matrix_QuickInverse(ref Mat4x4 m) // Only for Rotation/Translation Matrices
        {
            Mat4x4 matrix = new Mat4x4();
            matrix.m[0,0] = m.m[0,0]; matrix.m[0,1] = m.m[1,0]; matrix.m[0,2] = m.m[2,0]; matrix.m[0,3] = 0.0f;
            matrix.m[1,0] = m.m[0,1]; matrix.m[1,1] = m.m[1,1]; matrix.m[1,2] = m.m[2,1]; matrix.m[1,3] = 0.0f;
            matrix.m[2,0] = m.m[0,2]; matrix.m[2,1] = m.m[1,2]; matrix.m[2,2] = m.m[2,2]; matrix.m[2,3] = 0.0f;
            matrix.m[3,0] = -(m.m[3,0] * matrix.m[0,0] + m.m[3,1] * matrix.m[1,0] + m.m[3,2] * matrix.m[2,0]);
            matrix.m[3,1] = -(m.m[3,0] * matrix.m[0,1] + m.m[3,1] * matrix.m[1,1] + m.m[3,2] * matrix.m[2,1]);
            matrix.m[3,2] = -(m.m[3,0] * matrix.m[0,2] + m.m[3,1] * matrix.m[1,2] + m.m[3,2] * matrix.m[2,2]);
            matrix.m[3,3] = 1.0f;
            return matrix;
        }
        public static float Vector_DotProduct(ref Vec3d v1, ref Vec3d v2)
        {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }

        public static float Vector_Length(ref Vec3d v)
        {
            return MathF.Sqrt(Vector_DotProduct(ref v, ref v));
        }

        public static Vec3d Vector_Normalize(ref Vec3d v)
        {
            float l = Vector_Length(ref v);
            Vec3d vec3D = new Vec3d
            {
                x = v.x / l,
                y = v.y / l,
                z = v.z / l
            };
            return vec3D;
        }

        public static Vec3d Vector_CrossProduct(ref Vec3d v1, ref Vec3d v2)
        {
            Vec3d v = new Vec3d
            {
                x = v1.y * v2.z - v1.z * v2.y,
                y = v1.z * v2.x - v1.x * v2.z,
                z = v1.x * v2.y - v1.y * v2.x
            };          
            return v;
        }
        public static Vec3d Vector_IntersectPlane(ref Vec3d plane_p, ref Vec3d plane_n, ref Vec3d lineStart, ref Vec3d lineEnd, ref float t)
        {
            plane_n = Vector_Normalize(ref plane_n);
            float plane_d = -Vector_DotProduct(ref plane_n, ref plane_p);
            float ad = Vector_DotProduct(ref lineStart, ref plane_n);
            float bd = Vector_DotProduct(ref lineEnd, ref plane_n);
            t = (-plane_d - ad) / (bd - ad);
            Vec3d lineStartToEnd = lineEnd - lineStart;
            Vec3d lineToIntersect = lineStartToEnd * t;
            return lineStart + lineToIntersect;
        }

    }
}

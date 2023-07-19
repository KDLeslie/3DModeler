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
            public float x = 0;
            public float y = 0;
            public float z = 0;
            public float w = 1; // 4th term is needed for vector multiplication
        }

        public struct Mat4x4
        {
            public Mat4x4()
            {
            }

            public float[,] m = new float[4, 4];
        }

        public static Vec3d Matrix_MultiplyVector(ref Mat4x4 m, ref Vec3d i)
        {
            Vec3d v = new Vec3d
            {
                x = i.x * m.m[0, 0] + i.y * m.m[1, 0] + i.z * m.m[2, 0] + i.w * m.m[3, 0],
                y = i.x * m.m[0, 1] + i.y * m.m[1, 1] + i.z * m.m[2, 1] + i.w * m.m[3, 1],
                z = i.x * m.m[0, 2] + i.y * m.m[1, 2] + i.z * m.m[2, 2] + i.w * m.m[3, 2],
                w = i.x * m.m[0, 3] + i.y * m.m[1, 3] + i.z * m.m[2, 3] + i.w * m.m[3, 3]
            };
            return v;
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

        public static Mat4x4 Matrix_MultiplyMatrix(ref Mat4x4 m1, ref Mat4x4 m2)
        {
            Mat4x4 matrix = new Mat4x4();
            for (int c = 0; c < 4; c++)
                for (int r = 0; r < 4; r++)
                    matrix.m[r,c] = m1.m[r,0] * m2.m[0,c] + m1.m[r,1] * m2.m[1,c] + m1.m[r,2] * m2.m[2,c] + m1.m[r,3] * m2.m[3,c];
            return matrix;
        }

        public static Mat4x4 Matrix_PointAt(ref Vec3d pos, ref Vec3d target, ref Vec3d up)
        {
            // calculate new forward direction
            Vec3d newForward = Vector_Sub(ref target, ref pos);
            newForward = Vector_Normalize(ref newForward);

            // Calculate new Up direction incase foward vector has a y-component
            Vec3d a = Vector_Mul(ref newForward, Vector_DotProduct(ref up, ref newForward));
            Vec3d newUp = Vector_Sub(ref up, ref a);
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


        public static Vec3d Vector_Add(ref Vec3d v1, ref Vec3d v2)
        {
            Vec3d vec3D = new Vec3d
            {
                x = v1.x + v2.x,
                y = v1.y + v2.y,
                z = v1.z + v2.z
            };
            return vec3D;
        }

        public static Vec3d Vector_Sub(ref Vec3d v1, ref Vec3d v2)
        {
            Vec3d vec3D = new Vec3d
            {
                x = v1.x - v2.x,
                y = v1.y - v2.y,
                z = v1.z - v2.z
            };
            return vec3D;
        }

        public static  Vec3d Vector_Mul(ref Vec3d v1, float k)
        {
            Vec3d vec3D = new Vec3d
            {
                x = v1.x * k,
                y = v1.y * k,
                z = v1.z * k
            };
            return vec3D;
        }

        public static Vec3d Vector_Div(ref Vec3d v1, float k)
        {
            Vec3d vec3D = new Vec3d
            {
                x = v1.x / k, 
                y = v1.y / k, 
                z = v1.z / k 
            };
            return vec3D;
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
            Vec3d lineStartToEnd = Vector_Sub(ref lineEnd, ref lineStart);
            Vec3d lineToIntersect = Vector_Mul(ref lineStartToEnd, t);
            return Vector_Add(ref lineStart, ref lineToIntersect);
        }

    }
}

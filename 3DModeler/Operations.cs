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
            public Vec2d() 
            { 
                this.u = 0;
                this.v = 0;
                this.w = 1;
            }

            public Vec2d(float u, float v) 
            { 
                this.u = u;
                this.v = v;
                this.w = 1;
            }
            public Vec2d(Vec2d vec2D)
            {
                this.u = vec2D.u;
                this.v = vec2D.v;
                this.w = vec2D.w;
            }
            public float u { get; set; } 
            public float v { get; set; } 
            public float w { get; set; } // Keeps track of the depth of each texture coordinate

        }

        // A 3D structure to hold vertex coordinates
        public struct Vec3d
        {
            public Vec3d() 
            { 
                this.x = 0;
                this.y = 0;
                this.z = 0;
                this.w = 1;
            }
            public Vec3d(float x, float y, float z)
            {
                this.x = x;
                this.y = y; 
                this.z = z;
                this.w = 1;
            }
            public Vec3d(Vec3d vec3D)
            {
                this.w = vec3D.w;
                this.x = vec3D.x;
                this.y = vec3D.y;
                this.z = vec3D.z;
            }
            public float x { get; set; }
            public float y { get; set; }
            public float z { get; set; }
            public float w { get; set; } = 1; // 4th term is needed for vector multiplication
        }

        public struct Mat4x4
        {
            public Mat4x4()
            {
            }

            public float[,] m { get; set; } = new float[4, 4];
        }

        public static Vec3d Matrix_MultiplyVector(Mat4x4 m, Vec3d i)
        {
            Vec3d v = new Vec3d();
            v.x = i.x * m.m[0,0] + i.y * m.m[1,0] + i.z * m.m[2,0] + i.w * m.m[3,0];
            v.y = i.x * m.m[0,1] + i.y * m.m[1,1] + i.z * m.m[2,1] + i.w * m.m[3,1];
            v.z = i.x * m.m[0,2] + i.y * m.m[1,2] + i.z * m.m[2,2] + i.w * m.m[3,2];
            v.w = i.x * m.m[0,3] + i.y * m.m[1,3] + i.z * m.m[2,3] + i.w * m.m[3,3];
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

        public static Mat4x4 Matrix_MultiplyMatrix(Mat4x4 m1, Mat4x4 m2)
        {
            Mat4x4 matrix = new Mat4x4();
            for (int c = 0; c < 4; c++)
                for (int r = 0; r < 4; r++)
                    matrix.m[r,c] = m1.m[r,0] * m2.m[0,c] + m1.m[r,1] * m2.m[1,c] + m1.m[r,2] * m2.m[2,c] + m1.m[r,3] * m2.m[3,c];
            return matrix;
        }

        public static Mat4x4 Matrix_PointAt(Vec3d pos, Vec3d target, Vec3d up)
        {
            // calculate new forward direction
            Vec3d newForward = Vector_Sub(target, pos);
            newForward = Vector_Normalize(newForward);

            // Calculate new Up direction incase foward vector has a y-component
            Vec3d a = Vector_Mul(newForward, Vector_DotProduct(up, newForward));
            Vec3d newUp = Vector_Sub(up, a);
            newUp = Vector_Normalize(newUp);

            // New Right direction is easy, its just cross product
            Vec3d newRight = Vector_CrossProduct(newUp, newForward);

            // Construct Dimensioning and Translation Matrix	
            Mat4x4 matrix = new Mat4x4();
            matrix.m[0,0] = newRight.x; matrix.m[0,1] = newRight.y; matrix.m[0,2] = newRight.z; matrix.m[0,3] = 0.0f;
            matrix.m[1,0] = newUp.x; matrix.m[1,1] = newUp.y; matrix.m[1,2] = newUp.z; matrix.m[1,3] = 0.0f;
            matrix.m[2,0] = newForward.x; matrix.m[2,1] = newForward.y; matrix.m[2,2] = newForward.z; matrix.m[2,3] = 0.0f;
            matrix.m[3,0] = pos.x; matrix.m[3,1] = pos.y; matrix.m[3,2] = pos.z; matrix.m[3,3] = 1.0f;
            return matrix;
        }

        public static Mat4x4 Matrix_QuickInverse(Mat4x4 m) // Only for Rotation/Translation Matrices
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


        public static Vec3d Vector_Add(Vec3d v1, Vec3d v2)
        {
            Vec3d vec3D = new Vec3d();
            vec3D.x = v1.x + v2.x;
            vec3D.y = v1.y + v2.y;
            vec3D.z = v1.z + v2.z;
            return vec3D;
        }

        public static Vec3d Vector_Sub(Vec3d v1, Vec3d v2)
        {
            Vec3d vec3D = new Vec3d();
            vec3D.x = v1.x - v2.x;
            vec3D.y = v1.y - v2.y;
            vec3D.z = v1.z - v2.z;
            return vec3D;
        }

        public static  Vec3d Vector_Mul(Vec3d v1, float k)
        {
            Vec3d vec3D = new Vec3d();
            vec3D.x = v1.x * k;
            vec3D.y = v1.y * k;
            vec3D.z = v1.z * k;
            return vec3D;
        }

        public static Vec3d Vector_Div(Vec3d v1, float k)
        {
            Vec3d vec3D = new Vec3d();
            vec3D.x = v1.x / k;
            vec3D.y = v1.y / k;
            vec3D.z = v1.z / k;
            return vec3D;
        }

        public static float Vector_DotProduct(Vec3d v1, Vec3d v2)
        {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }

        public static float Vector_Length(Vec3d v)
        {
            return MathF.Sqrt(Vector_DotProduct(v, v));
        }

        public static Vec3d Vector_Normalize(Vec3d v)
        {
            float l = Vector_Length(v);
            Vec3d vec3D = new Vec3d();
            vec3D.x = v.x / l;
            vec3D.y = v.y / l;
            vec3D.z = v.z / l;
            return vec3D;
        }

        public static Vec3d Vector_CrossProduct(Vec3d v1, Vec3d v2)
        {
            Vec3d v = new Vec3d();
            v.x = v1.y * v2.z - v1.z * v2.y;
            v.y = v1.z * v2.x - v1.x * v2.z;
            v.z = v1.x * v2.y - v1.y * v2.x;
            return v;
        }
        public static Vec3d Vector_IntersectPlane(Vec3d plane_p, Vec3d plane_n, Vec3d lineStart, Vec3d lineEnd, ref float t)
        {
            plane_n = Vector_Normalize(plane_n);
            float plane_d = -Vector_DotProduct(plane_n, plane_p);
            float ad = Vector_DotProduct(lineStart, plane_n);
            float bd = Vector_DotProduct(lineEnd, plane_n);
            t = (-plane_d - ad) / (bd - ad);
            Vec3d lineStartToEnd = Vector_Sub(lineEnd, lineStart);
            Vec3d lineToIntersect = Vector_Mul(lineStartToEnd, t);
            return Vector_Add(lineStart, lineToIntersect);
        }

    }
}

using static System.MathF;


namespace _3DModeler
{
    public static class Operations
    {
        // Returns an identity matrix
        public static Mat4x4 MakeIdentity()
        {
            Mat4x4 matrix = new Mat4x4();
            matrix.m[0,0] = 1.0f;
            matrix.m[1,1] = 1.0f;
            matrix.m[2,2] = 1.0f;
            matrix.m[3,3] = 1.0f;
            return matrix;
        }

        // Returns a rotation matrix
        public static Mat4x4 MakeRotation(float x = 0, float y = 0, float z = 0, bool degrees = false)
        {
            Mat4x4 matrix = new Mat4x4();
            if (degrees)
            {
                x = x * PI / 180;
                y = y * PI / 180;
                z = z * PI / 180;
            }
            matrix.m[0, 0] = Cos(y) * Cos(z);
            matrix.m[0, 1] = Cos(y) * Sin(z);
            matrix.m[0, 2] = Sin(y);
            matrix.m[0, 3] = 0;
            matrix.m[1, 0] = -Sin(x) * Sin(y) * Cos(z) - Cos(x) * Sin(z);
            matrix.m[1, 1] = Cos(x) * Cos(z) - Sin(x) * Sin(y) * Sin(z);
            matrix.m[1, 2] = Sin(x) * Cos(y);
            matrix.m[1, 3] = 0;
            matrix.m[2, 0] = Sin(x) * Sin(z) - Cos(x) * Sin(y) * Cos(z);
            matrix.m[2, 1] = -Sin(x) * Cos(z) - Cos(x) * Sin(y) * Sin(z);
            matrix.m[2, 2] = Cos(x) * Cos(y);
            matrix.m[2, 3] = 0;
            matrix.m[3, 0] = 0;
            matrix.m[3, 1] = 0;
            matrix.m[3, 2] = 0;
            matrix.m[3, 3] = 1.0f;
            return matrix;
        }

        // Returns a translation matrix
        public static Mat4x4 MakeTranslation(float x = 0, float y = 0, float z = 0)
        {
            Mat4x4 matrix = MakeIdentity();
            matrix.m[3,0] = x;
            matrix.m[3,1] = y;
            matrix.m[3,2] = z;
            return matrix;
        }

        // Returns a scale matrix
        public static Mat4x4 MakeScale(float x = 1, float y = 1, float z = 1)
        {
            Mat4x4 matrix = MakeIdentity();
            matrix.m[0, 0] = x;
            matrix.m[1, 1] = y;
            matrix.m[2, 2] = z;
            return matrix;
        }

        // Returns a projection matrix. Requires the field of view,
        // aspect ratio, and near and far z planes
        public static Mat4x4 MakeProjection(float fovDegrees, float aspectRatio, float zNear, float zFar)
        {
            float fFovRad = 1.0f / Tan(fovDegrees * 0.5f / 180.0f * PI);
            Mat4x4 matrix = new Mat4x4();
            matrix.m[0,0] = aspectRatio * fFovRad;
            matrix.m[1,1] = fFovRad;
            matrix.m[2,2] = zFar / (zFar - zNear);
            matrix.m[3,2] = (-zFar * zNear) / (zFar - zNear);
            matrix.m[2,3] = 1.0f;
            matrix.m[3,3] = 0.0f;
            return matrix;
        }

        // Returns a "Point At" matrix
        public static Mat4x4 MakePointAt(ref Vec3D pos, ref Vec3D target, ref Vec3D up)
        {
            // Calculate new Forward direction
            Vec3D newForward = target - pos;
            newForward = Normalize(ref newForward);

            // Calculate new Up direction incase forward vector has a y-component
            Vec3D a = newForward * DotProduct(ref up, ref newForward);
            Vec3D newUp = up - a;
            newUp = Normalize(ref newUp);

            // New Right direction is just cross product
            Vec3D newRight = CrossProduct(ref newUp, ref newForward);

            // Construct Dimensioning and Translation Matrix	
            Mat4x4 matrix = new Mat4x4();
            matrix.m[0,0] = newRight.x; 
            matrix.m[0,1] = newRight.y; 
            matrix.m[0,2] = newRight.z; 
            matrix.m[0,3] = 0.0f;
            matrix.m[1,0] = newUp.x; 
            matrix.m[1,1] = newUp.y; 
            matrix.m[1,2] = newUp.z; 
            matrix.m[1,3] = 0.0f;
            matrix.m[2,0] = newForward.x; 
            matrix.m[2,1] = newForward.y; 
            matrix.m[2,2] = newForward.z; 
            matrix.m[2,3] = 0.0f;
            matrix.m[3,0] = pos.x; 
            matrix.m[3,1] = pos.y; 
            matrix.m[3,2] = pos.z; 
            matrix.m[3,3] = 1.0f;
            return matrix;
        }

        // Returns the inverse of rotation or translation matrices
        public static Mat4x4 QuickInverse(ref Mat4x4 m) 
        {
            Mat4x4 matrix = new Mat4x4();
            matrix.m[0,0] = m.m[0,0]; 
            matrix.m[0,1] = m.m[1,0]; 
            matrix.m[0,2] = m.m[2,0]; 
            matrix.m[0,3] = 0.0f;
            matrix.m[1,0] = m.m[0,1]; 
            matrix.m[1,1] = m.m[1,1]; 
            matrix.m[1,2] = m.m[2,1]; 
            matrix.m[1,3] = 0.0f;
            matrix.m[2,0] = m.m[0,2]; 
            matrix.m[2,1] = m.m[1,2]; 
            matrix.m[2,2] = m.m[2,2]; 
            matrix.m[2,3] = 0.0f;
            matrix.m[3,0] = -(m.m[3,0] * matrix.m[0,0] + m.m[3,1] * matrix.m[1,0] + m.m[3,2] * matrix.m[2,0]);
            matrix.m[3,1] = -(m.m[3,0] * matrix.m[0,1] + m.m[3,1] * matrix.m[1,1] + m.m[3,2] * matrix.m[2,1]);
            matrix.m[3,2] = -(m.m[3,0] * matrix.m[0,2] + m.m[3,1] * matrix.m[1,2] + m.m[3,2] * matrix.m[2,2]);
            matrix.m[3,3] = 1.0f;
            return matrix;
        }

        // Returns the dot product of two vectors
        public static float DotProduct(ref Vec3D v1, ref Vec3D v2)
        {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }

        // Returns the length of a vector
        public static float Length(ref Vec3D v)
        {
            return Sqrt(DotProduct(ref v, ref v));
        }

        // Returns the normalized vector of a vector
        public static Vec3D Normalize(ref Vec3D v)
        {
            float l = Length(ref v);
            Vec3D vec3D = new Vec3D
            {
                x = v.x / l,
                y = v.y / l,
                z = v.z / l
            };
            return vec3D;
        }

        // Returns the cross product of two vectors
        public static Vec3D CrossProduct(ref Vec3D v1, ref Vec3D v2)
        {
            Vec3D v = new Vec3D
            {
                x = v1.y * v2.z - v1.z * v2.y,
                y = v1.z * v2.x - v1.x * v2.z,
                z = v1.x * v2.y - v1.y * v2.x
            };          
            return v;
        }

        // Returns the point of intersection between a line and a plane.
        // The scale parameter t is passed out via reference
        public static Vec3D IntersectPlane(ref Vec3D plane_p, ref Vec3D plane_n, ref Vec3D lineStart, ref Vec3D lineEnd, ref float t)
        {
            plane_n = Normalize(ref plane_n);
            float plane_d = -DotProduct(ref plane_n, ref plane_p);
            float ad = DotProduct(ref lineStart, ref plane_n);
            float bd = DotProduct(ref lineEnd, ref plane_n);
            t = (-plane_d - ad) / (bd - ad);
            Vec3D lineStartToEnd = lineEnd - lineStart;
            Vec3D lineToIntersect = lineStartToEnd * t;
            return lineStart + lineToIntersect;
        }

        // Gets a transformation matrix to transform each tri according to its own
        // specific transformation data. Unlike world transforms, these have permanent
        // affects on the triangle
        public static Mat4x4 GetTriTransformationMatrix(Mesh mesh)
        {
            Mat4x4 matTranslate = MakeTranslation(mesh.translation[0], mesh.translation[1], mesh.translation[2]);
            Mat4x4 matScale = MakeScale(mesh.scale[0], mesh.scale[1], mesh.scale[2]);
            Mat4x4 matRotate = MakeRotation(mesh.rotation[0], degrees: true) * MakeRotation(y: mesh.rotation[1], degrees: true)
                * MakeRotation(z: mesh.rotation[2], degrees: true);
            return matScale * matRotate * matTranslate;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static _3DModeler.Operations;

namespace _3DModeler
{
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
        public float[] Thetas { get; set; } = { 0, 0, 0 }; // World rotation around x, y, and z axes (only changes the view)
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

}

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

        // Reads and loads in objects from an obj file. Returns false if
        // no objects could be loaded. TODO: Add support for obj files that
        // contain normal information
        private bool LoadFromOBJFile(string filename, ref string materialFilename)
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
        private bool LoadFromMTLFile(string filename, string folderPath, Dictionary<string, Material> object_Material)
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
                                // Remove the alpha as transparent textures are not yet supported
                                material.texture.RemoveAlpha();
                                material.hasTexture = true;

                            }
                            catch (ArgumentException)
                            {
                                try
                                {
                                    // Look for the image through an absolute path
                                    material.texture = new DirectBitmap(material.texturePath);
                                    // Remove the alpha as transparent textures are not yet supported
                                    material.texture.RemoveAlpha();
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


            // Clear pixels of the previous frame
            // By default, the bitmap produced is entirely transparent
            if (MainView.Frame != null)
                MainView.Frame.Clear();
            // Create a new background color for the frame
            ViewWindow.BackColor = Color.Cyan;
            // Clear depth buffer each frame
            Array.Clear(MainView.DepthBuffer);

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
                MainView.Frame = new DirectBitmap(MainView.ScreenWidth, MainView.ScreenHeight);
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
                    if (LoadFromOBJFile(filePath, ref materialName))
                    {
                        // If the obj file references a material file... 
                        if (materialName != "")
                        {
                            string materialPath = Path.Combine(folderPath, materialName);
                            Dictionary<string, Material> materialMap = new Dictionary<string, Material>();
                            // If the mtl file was read successfully...
                            if (LoadFromMTLFile(materialPath, folderPath, materialMap))
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
            cube.name = "Mesh" + Meshes.Count;
            cube.materialName = "Mesh" + Meshes.Count;
            Meshes.Add(cube);
            ObjectList.Items.Add(cube.name);
        }
        // Generated by ChatGPT
        private void SphereToolStripMenuItem_Click(object sender, EventArgs e)
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
            sphere.name = "Mesh" + Meshes.Count;
            sphere.materialName = "Mesh" + Meshes.Count;
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

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void MainForm_Load(object sender, EventArgs e)
        {
            MainView.Frame = new DirectBitmap(MainView.ScreenWidth, MainView.ScreenHeight);
        }
    }
}
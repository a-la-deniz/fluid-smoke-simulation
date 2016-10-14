using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace FluidSym
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Matrix world = Matrix.Identity;
        Matrix view = Matrix.Identity;
        Matrix rotation = Matrix.Identity;
        Matrix projection;
        Vector3 camPos = new Vector3(0, 0, 50);
        Vector3 camCenter = Vector3.UnitZ;
        Vector3 up = Vector3.UnitY;
        Vector3 forward = Vector3.UnitZ;
        Vector3 left = Vector3.UnitX;
        MouseState originalMouseState;
        MouseState previousMouseState;
        Matrix camera = Matrix.Identity;
        solver2d Solver2D;
        solver3d Solver3D;
        KeyboardState prevKeybState;
        bool asdf = true;

        float xDifference = 90;
        float yDifference = 90;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = 512;
            graphics.PreferredBackBufferWidth = 512;
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45),
            graphics.PreferredBackBufferWidth / graphics.PreferredBackBufferHeight,
            0.1f, 1000.0f);
            camCenter = camPos + forward * 50;
            Matrix.CreateLookAt(ref camPos, ref camCenter, ref up, out view);
            Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            this.IsMouseVisible = true;
            originalMouseState = Mouse.GetState();
            previousMouseState = originalMouseState;
            Solver3D = new solver3d(15);
            Solver2D = new solver2d(64);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            ProcessInputs();
            // TODO: Add your update logic here
            //bzz.solve(0.1f);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            camera = Matrix.CreateRotationY(MathHelper.ToRadians(90 - xDifference));
            camera = Matrix.CreateRotationX(MathHelper.ToRadians(90 - yDifference)) * camera;
            forward = camera.Forward;
            camCenter = camPos + forward;
            Matrix.CreateLookAt(ref camPos, ref camCenter, ref up, out view);
            DrawTerrain();
            if (asdf)
            {
                Solver2D.solve(1f);
                drawSquare();
            }
            else
            {
                Solver3D.solve(1f);
                drawSmoke();
            }

            base.Draw(gameTime);
        }
        private void ProcessInputs()
        {
            KeyboardState keybState = Keyboard.GetState();
            MouseState mseState = Mouse.GetState();
            int i = (int)((mseState.X / (float)512) * Solver2D.dim + 1);
            int j = (int)(((512 - mseState.Y) / (float)512) * Solver2D.dim + 1);
            i = (int)MathHelper.Clamp(i, 0, Solver2D.dim + 1);
            j = (int)MathHelper.Clamp(j, 0, Solver2D.dim + 1);
            int k = (int)((mseState.X / (float)512) * Solver3D.dim + 1);
            int l = (int)(((512 - mseState.Y) / (float)512) * Solver3D.dim + 1);
            k = (int)MathHelper.Clamp(k, 0, Solver3D.dim + 1);
            l = (int)MathHelper.Clamp(l, 0, Solver3D.dim + 1);
            if (mseState != previousMouseState)
            {
                if (mseState.MiddleButton == ButtonState.Pressed)
                {
                    xDifference += ((mseState.X - previousMouseState.X) / 5.0f);
                    yDifference += ((mseState.Y - previousMouseState.Y) / 5.0f);
                }
                //rotation = Matrix.CreateRotationY(MathHelper.ToRadians(xDifference)) * rotation;
                //rotation = Matrix.CreateRotationX(MathHelper.ToRadians(yDifference)) * rotation;
                //camCenter.X += xDifference / 5;
                //camCenter.Y -= yDifference / 5;
                //Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            }
            if (keybState.IsKeyDown(Keys.LeftControl))
            {
                //bzz.vel_prev[i, j].X = 5 * (mseState.X - previousMouseState.X);
                //bzz.vel_prev[i, j].Y = -5 * (mseState.Y - previousMouseState.Y);

                //bzz.col_prev[8, 1] = 100.0f;
                //bzz.vel_prev[8, 1].X = 0;
                //bzz.vel_prev[8, 1].Y = -3;
                Solver2D.vel_prev[i, j].X = 0;
                Solver2D.vel_prev[i, j].Y = 10;
                Solver3D.vel_prev[k, l, Solver3D.dim / 2].X = 0;
                if (l > Solver3D.dim / 2)
                    Solver3D.vel_prev[k, l, Solver3D.dim / 2].Y = -10;
                else
                    Solver3D.vel_prev[k, l, Solver3D.dim / 2].Y = 10;
                Solver3D.vel_prev[k, l, Solver3D.dim / 2].Z = 0;

                
            }
            if (mseState.RightButton == ButtonState.Pressed)
            {
                Solver2D.col_prev[i, j] = 100;
                Solver3D.col_prev[k, l, Solver3D.dim / 2] = new Vector4(0.4f, 0.2f, 0.0f, 0.2f);

                //bzz.col_prev[8, 1] = 100.0f;
                //bzz.vel_prev[8, 1].X = 0;
                //bzz.vel_prev[8, 1].Y = -3;

            }
            if (keybState.IsKeyDown(Keys.Space))
            {
                camPos.Y += 1f;
            }
            if (keybState.IsKeyDown(Keys.LeftShift))
            {
                camPos.Y -= 1f;
            }
            if (keybState.IsKeyDown(Keys.A))
            {
                camPos -= camera.Right;
            }
            if (keybState.IsKeyDown(Keys.D))
            {
                camPos += camera.Right;
            }
            if (keybState.IsKeyDown(Keys.W))
            {
                camPos += camera.Forward;
            }
            if (keybState.IsKeyDown(Keys.S))
            {
                camPos -= camera.Forward;
            }
            if (keybState.IsKeyDown(Keys.Escape))
            {
                Solver3D = new solver3d(Solver3D.dim);
                Solver2D = new solver2d(Solver2D.dim);
            }
            if (keybState.IsKeyUp(Keys.Up) && keybState.IsKeyUp(Keys.Down))
            {
                
            }
            if (keybState.IsKeyUp(Keys.Right))
            {
                
            }
            if (keybState.IsKeyUp(Keys.Left))
            {
                if(prevKeybState.IsKeyDown(Keys.Left))
                {
                    asdf = !asdf;
                }
            }
            prevKeybState = keybState;
            previousMouseState = mseState;
        }

        protected void drawSquare()
        {
            camPos.X = 32;
            camPos.Y = 32;
            camPos.Z = 78;
            xDifference = 90;
            yDifference = 90;
            VertexBuffer vertexBuffer;

            BasicEffect basicEffect;
            basicEffect = new BasicEffect(GraphicsDevice);

            basicEffect.World = world;
            basicEffect.View = view;
            basicEffect.Projection = projection;
            basicEffect.VertexColorEnabled = true;
            //basicEffect.LightingEnabled = true;
            //basicEffect.AmbientLightColor = new Vector3(0.2f, 0.2f, 0.2f);

            RasterizerState rasterizerState = new RasterizerState();
            //rasterizerState.CullMode = CullMode.None;

            GraphicsDevice.RasterizerState = rasterizerState;
            int dim1 = Solver2D.dim + 1;
            VertexPositionColor[] vertices = new VertexPositionColor[2 * 3 * dim1 * dim1];

            int total = 0;
            float h = 1.0f;
            float edge = h;
            float side = edge / 2;
            float x, y, d00, d01, d10, d11;
            for (int i = 0; i <= Solver2D.dim; i++)
            {
                x = (i - 0.5f) * h;
                for (int j = 0; j <= Solver2D.dim; j++)
                {
                    y = (j - 0.5f) * h;

                    d00 = Solver2D.col[i, j];
                    d01 = Solver2D.col[i, j + 1];
                    d10 = Solver2D.col[i + 1, j];
                    d11 = Solver2D.col[i + 1, j + 1];

                    //d00 = 1;
                    //d01 = 1;
                    //d10 = 1;
                    //d11 = 1;

                    vertices[total].Position.X = x;
                    vertices[total].Position.Y = y;
                    vertices[total].Position.Z = 0;
                    vertices[total].Color = new Color(d00, d00, d00);
                    vertices[total++].Color.A = 128;
                    vertices[total].Position.X = x;
                    vertices[total].Position.Y = y + edge;
                    vertices[total].Position.Z = 0;
                    vertices[total].Color = new Color(d01, d01, d01);
                    vertices[total++].Color.A = 128;
                    vertices[total].Position.X = x + edge;
                    vertices[total].Position.Y = y;
                    vertices[total].Position.Z = 0;
                    vertices[total].Color = new Color(d10, d10, d10);
                    vertices[total++].Color.A = 128;

                    vertices[total].Position.X = x + edge;
                    vertices[total].Position.Y = y + edge;
                    vertices[total].Position.Z = 0;
                    vertices[total].Color = new Color(d11, d11, d11);
                    vertices[total++].Color.A = 128;
                    vertices[total].Position.X = x + edge;
                    vertices[total].Position.Y = y;
                    vertices[total].Position.Z = 0;
                    vertices[total].Color = new Color(d10, d10, d10);
                    vertices[total++].Color.A = 128;
                    vertices[total].Position.X = x;
                    vertices[total].Position.Y = y + edge;
                    vertices[total].Position.Z = 0;
                    vertices[total].Color = new Color(d01, d01, d01);
                    vertices[total++].Color.A = 128;
                    //front
                }
            }
            vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), 12 * 3 * dim1 * dim1, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionColor>(vertices);
            GraphicsDevice.SetVertexBuffer(vertexBuffer);
            //GraphicsDevice.BlendState = BlendState.AlphaBlend;
            //GraphicsDevice.BlendState = BlendState.Additive;
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 12 * dim1 * dim1);
                //GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 12 * 3 * 8000, 0, 12 * 8000);
            }
            GraphicsDevice.BlendState = BlendState.Opaque;
        }

        protected void drawSmoke()
        {
            VertexBuffer vertexBuffer;

            BasicEffect basicEffect;
            basicEffect = new BasicEffect(GraphicsDevice);

            //basicEffect.World = world;
            basicEffect.World = Matrix.CreateTranslation(-Solver3D.dim / 2, 10, 0);
            basicEffect.View = view;
            basicEffect.Projection = projection;
            basicEffect.VertexColorEnabled = true;
            //basicEffect.LightingEnabled = true;
            //basicEffect.AmbientLightColor = new Vector3(0.2f, 0.2f, 0.2f);

            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;

            GraphicsDevice.RasterizerState = rasterizerState;
            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            int dim1 = Solver3D.dim + 1;
            VertexPositionColor[] vertices = new VertexPositionColor[12 * 3 * dim1 * dim1 * dim1];
            int total = 0;
            float h = 1.0f;
            float edge = h;
            float side = edge / 2;
            float x, y, z;
            Vector4 d000, d100, d010, d001,
                           d110, d101, d011, d111;

            for (int i = 0; i <= Solver3D.dim; i++)
            {
                x = (i - 0.5f) * h;

                for (int j = 0; j <= Solver3D.dim; j++)
                {
                    y = (j - 0.5f) * h;

                    for (int k = 0; k <= Solver3D.dim; k++)
                    {
                        z = (k - 0.5f) * h;

                        d000 = Solver3D.col[i, j, k];
                        d000.W = 0.01f;
                        d100 = Solver3D.col[i + 1, j, k];
                        d100.W = 0.01f;
                        d010 = Solver3D.col[i, j + 1, k];
                        d010.W = 0.01f;
                        d001 = Solver3D.col[i, j, k + 1];
                        d001.W = 0.01f;
                        d110 = Solver3D.col[i + 1, j + 1, k];
                        d110.W = 0.01f;
                        d011 = Solver3D.col[i, j + 1, k + 1];
                        d011.W = 0.01f;
                        d101 = Solver3D.col[i + 1, j, k + 1];
                        d101.W = 0.01f;
                        d111 = Solver3D.col[i + 1, j + 1, k + 1];
                        d111.W = 0.01f;

                        vertices[total].Position.X = x;
                        vertices[total].Position.Y = y;
                        vertices[total].Position.Z = z;
                        vertices[total++].Color = new Color(d000);
                        vertices[total].Position.X = x;
                        vertices[total].Position.Y = y + edge;
                        vertices[total].Position.Z = z;
                        vertices[total++].Color = new Color(d010);
                        vertices[total].Position.X = x + edge;
                        vertices[total].Position.Y = y;
                        vertices[total].Position.Z = z;
                        vertices[total++].Color = new Color(d100);

                        vertices[total].Position.X = x + edge;
                        vertices[total].Position.Y = y + edge;
                        vertices[total].Position.Z = z;
                        vertices[total++].Color = new Color(d110);
                        vertices[total].Position.X = x + edge;
                        vertices[total].Position.Y = y;
                        vertices[total].Position.Z = z;
                        vertices[total++].Color = new Color(d100);
                        vertices[total].Position.X = x;
                        vertices[total].Position.Y = y + edge;
                        vertices[total].Position.Z = z;
                        vertices[total++].Color = new Color(d010);
                        //front
                        vertices[total].Position.X = x + edge;
                        vertices[total].Position.Y = y;
                        vertices[total].Position.Z = z;
                        vertices[total++].Color = new Color(d100);
                        vertices[total].Position.X = x + edge;
                        vertices[total].Position.Y = y + edge;
                        vertices[total].Position.Z = z;
                        vertices[total++].Color = new Color(d110);
                        vertices[total].Position.X = x + edge;
                        vertices[total].Position.Y = y;
                        vertices[total].Position.Z = z - edge;
                        vertices[total++].Color = new Color(d101);

                        vertices[total].Position.X = x + edge;
                        vertices[total].Position.Y = y + edge;
                        vertices[total].Position.Z = z - edge;
                        vertices[total++].Color = new Color(d111);
                        vertices[total].Position.X = x + edge;
                        vertices[total].Position.Y = y;
                        vertices[total].Position.Z = z - edge;
                        vertices[total++].Color = new Color(d101);
                        vertices[total].Position.X = x + edge;
                        vertices[total].Position.Y = y + edge;
                        vertices[total].Position.Z = z;
                        vertices[total++].Color = new Color(d110);
                        //right
                        vertices[total].Position.X = x;
                        vertices[total].Position.Y = y;
                        vertices[total].Position.Z = z - edge;
                        vertices[total++].Color = new Color(d001);
                        vertices[total].Position.X = x;
                        vertices[total].Position.Y = y;
                        vertices[total].Position.Z = z;
                        vertices[total++].Color = new Color(d000);
                        vertices[total].Position.X = x + edge;
                        vertices[total].Position.Y = y;
                        vertices[total].Position.Z = z - edge;
                        vertices[total++].Color = new Color(d101);

                        vertices[total].Position.X = x + edge;
                        vertices[total].Position.Y = y;
                        vertices[total].Position.Z = z;
                        vertices[total++].Color = new Color(d100);
                        vertices[total].Position.X = x + edge;
                        vertices[total].Position.Y = y;
                        vertices[total].Position.Z = z - edge;
                        vertices[total++].Color = new Color(d101);
                        vertices[total].Position.X = x;
                        vertices[total].Position.Y = y;
                        vertices[total].Position.Z = z;
                        vertices[total++].Color = new Color(d000);
                        //bottom
                        vertices[total].Position.X = x;
                        vertices[total].Position.Y = y;
                        vertices[total].Position.Z = z - edge;
                        vertices[total++].Color = new Color(d001);
                        vertices[total].Position.X = x;
                        vertices[total].Position.Y = y + edge;
                        vertices[total].Position.Z = z - edge;
                        vertices[total++].Color = new Color(d011);
                        vertices[total].Position.X = x;
                        vertices[total].Position.Y = y;
                        vertices[total].Position.Z = z;
                        vertices[total++].Color = new Color(d000);

                        vertices[total].Position.X = x;
                        vertices[total].Position.Y = y + edge;
                        vertices[total].Position.Z = z;
                        vertices[total++].Color = new Color(d010);
                        vertices[total].Position.X = x;
                        vertices[total].Position.Y = y;
                        vertices[total].Position.Z = z;
                        vertices[total++].Color = new Color(d000);
                        vertices[total].Position.X = x;
                        vertices[total].Position.Y = y + edge;
                        vertices[total].Position.Z = z - edge;
                        vertices[total++].Color = new Color(d011);
                        //left
                        vertices[total].Position.X = x + edge;
                        vertices[total].Position.Y = y;
                        vertices[total].Position.Z = z - edge;
                        vertices[total++].Color = new Color(d101);
                        vertices[total].Position.X = x + edge;
                        vertices[total].Position.Y = y + edge;
                        vertices[total].Position.Z = z - edge;
                        vertices[total++].Color = new Color(d111);
                        vertices[total].Position.X = x;
                        vertices[total].Position.Y = y;
                        vertices[total].Position.Z = z - edge;
                        vertices[total++].Color = new Color(d001);

                        vertices[total].Position.X = x;
                        vertices[total].Position.Y = y + edge;
                        vertices[total].Position.Z = z - edge;
                        vertices[total++].Color = new Color(d011);
                        vertices[total].Position.X = x;
                        vertices[total].Position.Y = y;
                        vertices[total].Position.Z = z - edge;
                        vertices[total++].Color = new Color(d001);
                        vertices[total].Position.X = x + edge;
                        vertices[total].Position.Y = y + edge;
                        vertices[total].Position.Z = z - edge;
                        vertices[total++].Color = new Color(d111);
                        //back
                        vertices[total].Position.X = x;
                        vertices[total].Position.Y = y + edge;
                        vertices[total].Position.Z = z;
                        vertices[total++].Color = new Color(d010);
                        vertices[total].Position.X = x;
                        vertices[total].Position.Y = y + edge;
                        vertices[total].Position.Z = z - edge;
                        vertices[total++].Color = new Color(d011);
                        vertices[total].Position.X = x + edge;
                        vertices[total].Position.Y = y + edge;
                        vertices[total].Position.Z = z;
                        vertices[total++].Color = new Color(d110);

                        vertices[total].Position.X = x + edge;
                        vertices[total].Position.Y = y + edge;
                        vertices[total].Position.Z = z - edge;
                        vertices[total++].Color = new Color(d111);
                        vertices[total].Position.X = x + edge;
                        vertices[total].Position.Y = y + edge;
                        vertices[total].Position.Z = z;
                        vertices[total++].Color = new Color(d110);
                        vertices[total].Position.X = x;
                        vertices[total].Position.Y = y + edge;
                        vertices[total].Position.Z = z - edge;
                        vertices[total++].Color = new Color(d011);
                        //top
                    }
                }
            }
            vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), 12 * 3 * dim1 * dim1 * dim1, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionColor>(vertices);
            GraphicsDevice.SetVertexBuffer(vertexBuffer);
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            //GraphicsDevice.BlendState = BlendState.Additive;
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 12 * dim1 * dim1 * dim1);
                //GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 12 * 3 * 8000, 0, 12 * 8000);
            }
            GraphicsDevice.BlendState = BlendState.Opaque;
        }

        protected void drawCube()
        {
            VertexBuffer vertexBuffer;

            BasicEffect basicEffect;
            basicEffect = new BasicEffect(GraphicsDevice);

            //basicEffect.World = world;
            basicEffect.World = Matrix.CreateScale (0.1f) * Matrix.CreateTranslation(0, 0, 0);

            basicEffect.View = view;
            basicEffect.Projection = projection;
            basicEffect.VertexColorEnabled = true;
            //basicEffect.LightingEnabled = true;
            //basicEffect.AmbientLightColor = new Vector3(0.2f, 0.2f, 0.2f);

            RasterizerState rasterizerState = new RasterizerState();
            //rasterizerState.CullMode = CullMode.None;

            GraphicsDevice.RasterizerState = rasterizerState;
            int dim1 = 8;
            VertexPositionColor[] vertices = new VertexPositionColor[12 * 3 * dim1 * dim1 * dim1 * 8];

            int total = 0;
            float h = 1.0f;
            float edge = h;
            float side = edge / 2;
            for (int i = -dim1; i < dim1; i++)
            {
                for (int j = -dim1; j < dim1; j++)
                {
                    for (int k = -dim1; k < dim1; k++)
                    {
                        vertices[total].Position.X = i * h;
                        vertices[total].Position.Y = j * h;
                        vertices[total].Position.Z = k * h;
                        vertices[total].Color = Color.Blue;
                        vertices[total++].Color.A = 128;
                        vertices[total].Position.X = i * h;
                        vertices[total].Position.Y = j * h + edge;
                        vertices[total].Position.Z = k * h;
                        vertices[total].Color = Color.Blue;
                        vertices[total++].Color.A = 128;
                        vertices[total].Position.X = i * h + edge;
                        vertices[total].Position.Y = j * h;
                        vertices[total].Position.Z = k * h;
                        vertices[total].Color = Color.Blue;
                        vertices[total++].Color.A = 128;

                        vertices[total].Position.X = i * h + edge;
                        vertices[total].Position.Y = j * h + edge;
                        vertices[total].Position.Z = k * h;
                        vertices[total].Color = Color.Blue;
                        vertices[total++].Color.A = 128;
                        vertices[total].Position.X = i * h + edge;
                        vertices[total].Position.Y = j * h;
                        vertices[total].Position.Z = k * h;
                        vertices[total].Color = Color.Blue;
                        vertices[total++].Color.A = 128;
                        vertices[total].Position.X = i * h;
                        vertices[total].Position.Y = j * h + edge;
                        vertices[total].Position.Z = k * h;
                        vertices[total].Color = Color.Blue;
                        vertices[total++].Color.A = 128;
                        //front
                        vertices[total].Position.X = i * h + edge;
                        vertices[total].Position.Y = j * h;
                        vertices[total].Position.Z = k * h;
                        vertices[total].Color = Color.Red;
                        vertices[total++].Color.A = 128;
                        vertices[total].Position.X = i * h + edge;
                        vertices[total].Position.Y = j * h + edge;
                        vertices[total].Position.Z = k * h;
                        vertices[total].Color = Color.Red;
                        vertices[total++].Color.A = 128;
                        vertices[total].Position.X = i * h + edge;
                        vertices[total].Position.Y = j * h;
                        vertices[total].Position.Z = k * h - edge;
                        vertices[total].Color = Color.Red;
                        vertices[total++].Color.A = 128;
                        
                        vertices[total].Position.X = i * h + edge;
                        vertices[total].Position.Y = j * h + edge;
                        vertices[total].Position.Z = k * h - edge;
                        vertices[total].Color = Color.Red;
                        vertices[total++].Color.A = 128;
                        vertices[total].Position.X = i * h + edge;
                        vertices[total].Position.Y = j * h;
                        vertices[total].Position.Z = k * h - edge;
                        vertices[total].Color = Color.Red;
                        vertices[total++].Color.A = 128;
                        vertices[total].Position.X = i * h + edge;
                        vertices[total].Position.Y = j * h + edge;
                        vertices[total].Position.Z = k * h;
                        vertices[total].Color = Color.Red;
                        vertices[total++].Color.A = 128;
                        //right
                        vertices[total].Position.X = i * h;
                        vertices[total].Position.Y = j * h;
                        vertices[total].Position.Z = k * h - edge;
                        vertices[total].Color = Color.Green;
                        vertices[total++].Color.A = 128;
                        vertices[total].Position.X = i * h;
                        vertices[total].Position.Y = j * h;
                        vertices[total].Position.Z = k * h;
                        vertices[total].Color = Color.Green;
                        vertices[total++].Color.A = 128;
                        vertices[total].Position.X = i * h + edge;
                        vertices[total].Position.Y = j * h;
                        vertices[total].Position.Z = k * h - edge;
                        vertices[total].Color = Color.Green;
                        vertices[total++].Color.A = 128;

                        vertices[total].Position.X = i * h + edge;
                        vertices[total].Position.Y = j * h;
                        vertices[total].Position.Z = k * h;
                        vertices[total].Color = Color.Green;
                        vertices[total++].Color.A = 128;
                        vertices[total].Position.X = i * h + edge;
                        vertices[total].Position.Y = j * h;
                        vertices[total].Position.Z = k * h - edge;
                        vertices[total].Color = Color.Green;
                        vertices[total++].Color.A = 128;
                        vertices[total].Position.X = i * h;
                        vertices[total].Position.Y = j * h;
                        vertices[total].Position.Z = k * h;
                        vertices[total].Color = Color.Green;
                        vertices[total++].Color.A = 128;
                        //bottom
                        vertices[total].Position.X = i * h;
                        vertices[total].Position.Y = j * h;
                        vertices[total].Position.Z = k * h - edge;
                        vertices[total].Color = Color.Magenta;
                        vertices[total++].Color.A = 128;
                        vertices[total].Position.X = i * h;
                        vertices[total].Position.Y = j * h + edge;
                        vertices[total].Position.Z = k * h - edge;
                        vertices[total].Color = Color.Magenta;
                        vertices[total++].Color.A = 128;
                        vertices[total].Position.X = i * h;
                        vertices[total].Position.Y = j * h;
                        vertices[total].Position.Z = k * h;
                        vertices[total].Color = Color.Magenta;
                        vertices[total++].Color.A = 128;

                        vertices[total].Position.X = i * h;
                        vertices[total].Position.Y = j * h + edge;
                        vertices[total].Position.Z = k * h;
                        vertices[total].Color = Color.Magenta;
                        vertices[total++].Color.A = 128;
                        vertices[total].Position.X = i * h;
                        vertices[total].Position.Y = j * h;
                        vertices[total].Position.Z = k * h;
                        vertices[total].Color = Color.Magenta;
                        vertices[total++].Color.A = 128;
                        vertices[total].Position.X = i * h;
                        vertices[total].Position.Y = j * h + edge;
                        vertices[total].Position.Z = k * h - edge;
                        vertices[total].Color = Color.Magenta;
                        vertices[total++].Color.A = 128;
                        //left
                        vertices[total].Position.X = i * h + edge;
                        vertices[total].Position.Y = j * h;
                        vertices[total].Position.Z = k * h - edge;
                        vertices[total].Color = Color.Cyan;
                        vertices[total++].Color.A = 128;
                        vertices[total].Position.X = i * h + edge;
                        vertices[total].Position.Y = j * h + edge;
                        vertices[total].Position.Z = k * h - edge;
                        vertices[total].Color = Color.Cyan;
                        vertices[total++].Color.A = 128;
                        vertices[total].Position.X = i * h;
                        vertices[total].Position.Y = j * h;
                        vertices[total].Position.Z = k * h - edge;
                        vertices[total].Color = Color.Cyan;
                        vertices[total++].Color.A = 128;

                        vertices[total].Position.X = i * h;
                        vertices[total].Position.Y = j * h + edge;
                        vertices[total].Position.Z = k * h - edge;
                        vertices[total].Color = Color.Cyan;
                        vertices[total++].Color.A = 128;
                        vertices[total].Position.X = i * h;
                        vertices[total].Position.Y = j * h;
                        vertices[total].Position.Z = k * h - edge;
                        vertices[total].Color = Color.Cyan;
                        vertices[total++].Color.A = 128;
                        vertices[total].Position.X = i * h + edge;
                        vertices[total].Position.Y = j * h + edge;
                        vertices[total].Position.Z = k * h - edge;
                        vertices[total].Color = Color.Cyan;
                        vertices[total++].Color.A = 128;
                        //back
                        vertices[total].Position.X = i * h;
                        vertices[total].Position.Y = j * h + edge;
                        vertices[total].Position.Z = k * h;
                        vertices[total].Color = Color.Yellow;
                        vertices[total++].Color.A = 128;
                        vertices[total].Position.X = i * h;
                        vertices[total].Position.Y = j * h + edge;
                        vertices[total].Position.Z = k * h - edge;
                        vertices[total].Color = Color.Yellow;
                        vertices[total++].Color.A = 128;
                        vertices[total].Position.X = i * h + edge;
                        vertices[total].Position.Y = j * h + edge;
                        vertices[total].Position.Z = k * h;
                        vertices[total].Color = Color.Yellow;
                        vertices[total++].Color.A = 128;

                        vertices[total].Position.X = i * h + edge;
                        vertices[total].Position.Y = j * h + edge;
                        vertices[total].Position.Z = k * h - edge;
                        vertices[total].Color = Color.Yellow;
                        vertices[total++].Color.A = 128;
                        vertices[total].Position.X = i * h + edge;
                        vertices[total].Position.Y = j * h + edge;
                        vertices[total].Position.Z = k * h;
                        vertices[total].Color = Color.Yellow;
                        vertices[total++].Color.A = 128;
                        vertices[total].Position.X = i * h;
                        vertices[total].Position.Y = j * h + edge;
                        vertices[total].Position.Z = k * h - edge;
                        vertices[total].Color = Color.Yellow;
                        vertices[total++].Color.A = 128;
                        //top
                    }
                }
            }
            vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), 12 * 3 * dim1 * dim1 * dim1 * 8, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionColor>(vertices);
            GraphicsDevice.SetVertexBuffer(vertexBuffer);
            //GraphicsDevice.BlendState = BlendState.AlphaBlend;
            //GraphicsDevice.BlendState = BlendState.Additive;
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 12 * dim1 * dim1 * dim1 * 8);
                //GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 12 * 3 * 8000, 0, 12 * 8000);
            }
            GraphicsDevice.BlendState = BlendState.Opaque;
        }

        protected void DrawTerrain()
        {
            VertexBuffer vertexBuffer;

            BasicEffect basicEffect;
            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.World = Matrix.Identity;
            basicEffect.View = view;
            basicEffect.Projection = projection;
            basicEffect.VertexColorEnabled = true;

            //RasterizerState rasterizerState = new RasterizerState();
            //rasterizerState.CullMode = CullMode.None;

            //GraphicsDevice.RasterizerState = rasterizerState;
            //VertexPositionColor[] vertices = new VertexPositionColor[12 * 8000];

            //int total = 0;
            //float h = 1.0f;
            //float height = 1.665f * h;
            //float top = height * 2 / 3;
            //float bottom = height / 3;
            //float edge = 2.04f * h;
            //float side = edge / 2;
            //for (int i = -10; i < 10; i++)
            //{
            //    for (int j = -10; j < 10; j++)
            //    {
            //        for (int k = -10; k < 10; k++)
            //        {
            //            vertices[total].Position.X = i * h;
            //            vertices[total].Position.Y = j * h + top;
            //            vertices[total].Position.Z = k * h;
            //            vertices[total++].Color = Color.Gray;
            //            vertices[total].Position.X = i * h + side;
            //            vertices[total].Position.Y = j * h - bottom;
            //            vertices[total].Position.Z = k * h + side;
            //            vertices[total++].Color = Color.Gray;
            //            vertices[total].Position.X = i * h - side;
            //            vertices[total].Position.Y = j * h - bottom;
            //            vertices[total].Position.Z = k * h + side;
            //            vertices[total++].Color = Color.Gray;

            //            vertices[total].Position.X = i * h;
            //            vertices[total].Position.Y = j * h + top;
            //            vertices[total].Position.Z = k * h;
            //            vertices[total++].Color = Color.Blue;
            //            vertices[total].Position.X = i * h;
            //            vertices[total].Position.Y = j * h - bottom;
            //            vertices[total].Position.Z = k * h - side;
            //            vertices[total++].Color = Color.Blue;
            //            vertices[total].Position.X = i * h + side;
            //            vertices[total].Position.Y = j * h - bottom;
            //            vertices[total].Position.Z = k * h + side;
            //            vertices[total++].Color = Color.Blue;

            //            vertices[total].Position.X = i * h;
            //            vertices[total].Position.Y = j * h + top;
            //            vertices[total].Position.Z = k * h;
            //            vertices[total++].Color = Color.Red;
            //            vertices[total].Position.X = i * h - side;
            //            vertices[total].Position.Y = j * h - bottom;
            //            vertices[total].Position.Z = k * h + side;
            //            vertices[total++].Color = Color.Red;
            //            vertices[total].Position.X = i * h;
            //            vertices[total].Position.Y = j * h - bottom;
            //            vertices[total].Position.Z = k * h - side;
            //            vertices[total++].Color = Color.Red;

            //            vertices[total].Position.X = i * h - side;
            //            vertices[total].Position.Y = j * h - bottom;
            //            vertices[total].Position.Z = k * h + side;
            //            vertices[total++].Color = Color.Green;
            //            vertices[total].Position.X = i * h + side;
            //            vertices[total].Position.Y = j * h - bottom;
            //            vertices[total].Position.Z = k * h + side;
            //            vertices[total++].Color = Color.Green;
            //            vertices[total].Position.X = i * h;
            //            vertices[total].Position.Y = j * h - bottom;
            //            vertices[total].Position.Z = k * h - side;
            //            vertices[total++].Color = Color.Green;

            //        }
            //    }
            //}
            //vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), 96000, BufferUsage.WriteOnly);
            //vertexBuffer.SetData<VertexPositionColor>(vertices);
            //GraphicsDevice.SetVertexBuffer(vertexBuffer);

            //foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            //{
            //    pass.Apply();
            //    GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 32000);
            //}
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            GraphicsDevice.RasterizerState = rasterizerState;
            VertexPositionColor[] vertices = new VertexPositionColor[4];
            vertices[0] = new VertexPositionColor(new Vector3(0, 0, 0), Color.Gray);
            vertices[1] = new VertexPositionColor(new Vector3(0, 0, 1), Color.Gray);
            vertices[2] = new VertexPositionColor(new Vector3(1, 0, 0), Color.Gray);
            vertices[3] = new VertexPositionColor(new Vector3(1, 0, 1), Color.Gray);
            vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), 4, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionColor>(vertices);
            GraphicsDevice.SetVertexBuffer(vertexBuffer);

            for (int i = -50; i < 50; i++)
            {
                for (int j = -50; j < 50; j++)
                {
                    basicEffect.World = Matrix.CreateTranslation(i, 0, j);
                    foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                    }
                }
            }
        }
    }
}


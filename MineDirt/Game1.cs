using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MineDirt;
public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    Camera3D camera; 

    #region Stuff   

    VertexPositionColor[] vertices =
    [
        // Front face
        new VertexPositionColor(new Vector3(-1, 1, -1), Color.Red),
        new VertexPositionColor(new Vector3(1, 1, -1), Color.Green),
        new VertexPositionColor(new Vector3(-1, -1, -1), Color.Blue),
        new VertexPositionColor(new Vector3(1, -1, -1), Color.Yellow),

        // Back face
        new VertexPositionColor(new Vector3(-1, 1, 1), Color.Purple),
        new VertexPositionColor(new Vector3(1, 1, 1), Color.Orange),
        new VertexPositionColor(new Vector3(-1, -1, 1), Color.Cyan),
        new VertexPositionColor(new Vector3(1, -1, 1), Color.Magenta),
    ];

    short[] indices =
    [
        // Front face
        0, 1, 2, 2, 1, 3,
        // Back face
        4, 6, 5, 5, 6, 7,
        // Left face
        0, 2, 4, 4, 2, 6,
        // Right face
        1, 5, 3, 3, 5, 7,
        // Top face
        0, 4, 1, 1, 4, 5,
        // Bottom face
        2, 3, 6, 6, 3, 7
    ];

    VertexBuffer vertexBuffer;
    IndexBuffer indexBuffer;
    BasicEffect effect;

    #endregion

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        IsMouseVisible = false; // Hide the mouse cursor
        camera = new Camera3D(new Vector3(0, 0, 5), GraphicsDevice.Viewport.AspectRatio);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Create and set vertex buffer
        vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), vertices.Length, BufferUsage.WriteOnly);
        vertexBuffer.SetData(vertices);

        // Create and set index buffer
        indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
        indexBuffer.SetData(indices);

        // Initialize BasicEffect
        effect = new BasicEffect(GraphicsDevice)
        {
            VertexColorEnabled = true,
            View = Matrix.CreateLookAt(
                new Vector3(0, 0, 5),  // Camera position
                Vector3.Zero,          // Look at
                Vector3.Up),           // Up direction
            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,
                GraphicsDevice.Viewport.AspectRatio,
                0.1f, 100f)
        };
        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        var keyboardState = Keyboard.GetState();
        var mouseState = Mouse.GetState();

        // Update the camera with the current input and GraphicsDevice for mouse centering
        camera.Update(gameTime, keyboardState, mouseState, GraphicsDevice);

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        effect.View = camera.View;
        effect.Projection = camera.Projection;

        // Bind vertex and index buffers
        GraphicsDevice.SetVertexBuffer(vertexBuffer);
        GraphicsDevice.Indices = indexBuffer;

        // Apply effect and draw
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indices.Length / 3);
}

        base.Draw(gameTime);
    }
}

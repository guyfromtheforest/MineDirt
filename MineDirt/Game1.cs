using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MineDirt;
public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    Camera3D camera;
    Texture2D blockTextures;

    #region Stuff   

    SamplerState pointSampler = new SamplerState
{
    Filter = TextureFilter.Point, // This ensures point filtering is used
    AddressU = TextureAddressMode.Wrap, // This controls how textures behave at the horizontal edges (Wrap, Clamp, etc.)
    AddressV = TextureAddressMode.Wrap, // Same as above, controls behavior at vertical edges
};


    // Define the vertices with texture coordinates (UV mapping)
VertexPositionTexture[] vertices =
[
    // Front face (using the side texture)
    new VertexPositionTexture(new Vector3(-1, 1, -1), new Vector2(0.0625f, 0f)), // top-left
    new VertexPositionTexture(new Vector3(1, 1, -1), new Vector2(0.125f, 0f)),  // top-right
    new VertexPositionTexture(new Vector3(-1, -1, -1), new Vector2(0.0625f, 0.0625f)), // bottom-left
    new VertexPositionTexture(new Vector3(1, -1, -1), new Vector2(0.125f, 0.0625f)),  // bottom-right

    // Back face (using the side texture)
    new VertexPositionTexture(new Vector3(-1, 1, 1), new Vector2(0.0625f, 0f)),  // top-left
    new VertexPositionTexture(new Vector3(1, 1, 1), new Vector2(0.125f, 0f)),   // top-right
    new VertexPositionTexture(new Vector3(-1, -1, 1), new Vector2(0.0625f, 0.0625f)),  // bottom-left
    new VertexPositionTexture(new Vector3(1, -1, 1), new Vector2(0.125f, 0.0625f)),   // bottom-right

    // Left face (using the side texture)
    new VertexPositionTexture(new Vector3(-1, 1, -1), new Vector2(0.0625f, 0f)),  // top-left
    new VertexPositionTexture(new Vector3(-1, 1, 1), new Vector2(0.125f, 0f)),   // top-right
    new VertexPositionTexture(new Vector3(-1, -1, -1), new Vector2(0.0625f, 0.0625f)),  // bottom-left
    new VertexPositionTexture(new Vector3(-1, -1, 1), new Vector2(0.125f, 0.0625f)),   // bottom-right

    // Right face (using the side texture)
    new VertexPositionTexture(new Vector3(1, 1, -1), new Vector2(0.0625f, 0f)),  // top-left
    new VertexPositionTexture(new Vector3(1, 1, 1), new Vector2(0.125f, 0f)),   // top-right
    new VertexPositionTexture(new Vector3(1, -1, -1), new Vector2(0.0625f, 0.0625f)),  // bottom-left
    new VertexPositionTexture(new Vector3(1, -1, 1), new Vector2(0.125f, 0.0625f)),   // bottom-right

    // Top face (using the top texture)
    new VertexPositionTexture(new Vector3(-1, 1, -1), new Vector2(0f, 0f)),  // top-left
    new VertexPositionTexture(new Vector3(1, 1, -1), new Vector2(0.0625f, 0f)),   // top-right
    new VertexPositionTexture(new Vector3(-1, 1, 1), new Vector2(0f, 0.0625f)),   // bottom-left
    new VertexPositionTexture(new Vector3(1, 1, 1), new Vector2(0.0625f, 0.0625f)),    // bottom-right

    // Bottom face (using the bottom texture)
    new VertexPositionTexture(new Vector3(-1, -1, -1), new Vector2(0.125f, 0f)), // top-left
    new VertexPositionTexture(new Vector3(1, -1, -1), new Vector2(0.1875f, 0f)),  // top-right
    new VertexPositionTexture(new Vector3(-1, -1, 1), new Vector2(0.125f, 0.0625f)),  // bottom-left
    new VertexPositionTexture(new Vector3(1, -1, 1), new Vector2(0.1875f, 0.0625f))    // bottom-right
];

    short[] indices =
    [
        // Front face
        0, 1, 2, 2, 1, 3,
        // Back face
        4, 6, 5, 5, 6, 7,
        // Left face
        8, 9, 10, 10, 9, 11,
        // Right face
        12, 13, 14, 14, 13, 15,
        // Top face
        16, 17, 18, 18, 17, 19,
        // Bottom face
        20, 21, 22, 22, 21, 23
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

        // Set to fullscreen
        _graphics.IsFullScreen = true;

        // Set the resolution for fullscreen mode
        _graphics.PreferredBackBufferWidth = 1920;  // Set your preferred width
        _graphics.PreferredBackBufferHeight = 1080; // Set your preferred height
        _graphics.ApplyChanges();
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

        blockTextures = Content.Load<Texture2D>("Textures/Blocks");

        // Create the vertex buffer
        vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTexture), vertices.Length, BufferUsage.WriteOnly);
        vertexBuffer.SetData(vertices);

        // Create the index buffer
        indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
        indexBuffer.SetData(indices);

        // Initialize the BasicEffect
        effect = new BasicEffect(GraphicsDevice)
        {
            VertexColorEnabled = false, // Disable color shading
            TextureEnabled = true,      // Enable texture mapping
            Texture = blockTextures,     // Set the loaded texture
            View = Matrix.CreateLookAt(new Vector3(0, 0, 5), Vector3.Zero, Vector3.Up),
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.1f, 100f)
        };
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
        GraphicsDevice.RasterizerState = RasterizerState.CullNone;

        // Set the texture and effect matrices
        effect.Texture = blockTextures;
        effect.TextureEnabled = true;
        effect.View = camera.View;
        effect.Projection = camera.Projection;

        // Set the vertex buffer and index buffer
        GraphicsDevice.SamplerStates[0] = pointSampler;
        GraphicsDevice.SetVertexBuffer(vertexBuffer);
        GraphicsDevice.Indices = indexBuffer;

        // Draw the cube
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indices.Length / 3);
        }

        base.Draw(gameTime);
    }
}

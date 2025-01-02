using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MineDirt.Src.Blocks;

namespace MineDirt;
public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    Camera3D camera;
    Texture2D blockTextures;
    private VertexBuffer grassVertexBuffer;
    GrassBlock grassBlock = new(Vector3.Zero);
    Cobblestone cobblestone = new(new(0, -1, 0));

    #region Stuff   

    SamplerState pointSampler = new SamplerState
    {
        Filter = TextureFilter.Point, // This ensures point filtering is used
        AddressU = TextureAddressMode.Wrap, // This controls how textures behave at the horizontal edges (Wrap, Clamp, etc.)
        AddressV = TextureAddressMode.Wrap, // Same as above, controls behavior at vertical edges
    };

    VertexBuffer vertexBuffer;
    IndexBuffer indexBuffer;
    BasicEffect effect;
    private IndexBuffer grassIndexBuffer;
    private VertexBuffer cobblestoneVertexBuffer;
    private IndexBuffer cobblestoneIndexBuffer;

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
        // Create the vertex buffer for the GrassBlock
        grassVertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTexture), grassBlock.vertices.Length, BufferUsage.WriteOnly);
        grassVertexBuffer.SetData(grassBlock.vertices);

        // Create the index buffer for the GrassBlock
        grassIndexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, grassBlock.indices.Length, BufferUsage.WriteOnly);
        grassIndexBuffer.SetData(grassBlock.indices);

        // Create the vertex buffer for the Cobblestone
        cobblestoneVertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTexture), cobblestone.vertices.Length, BufferUsage.WriteOnly);
        cobblestoneVertexBuffer.SetData(cobblestone.vertices);

        // Create the index buffer for the Cobblestone
        cobblestoneIndexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, cobblestone.indices.Length, BufferUsage.WriteOnly);
        cobblestoneIndexBuffer.SetData(cobblestone.indices);

        // Initialize the BasicEffect
        effect = new BasicEffect(GraphicsDevice)
        {
            VertexColorEnabled = false,  // Disable color shading
            TextureEnabled = true,      // Enable texture mapping
            Texture = blockTextures,    // Set the loaded texture
            View = Matrix.CreateLookAt(new Vector3(0, 0, 5), Vector3.Zero, Vector3.Up),
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.1f, 100f)
        };

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
        // Set texture sampling to Point to avoid blurriness (pixelated textures)
        GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

        // Set the effect matrices for the camera
        effect.View = camera.View;
        effect.Projection = camera.Projection;

        // Set the texture for each block and render them
        effect.Texture = blockTextures; // Assuming the same texture for both, change as needed.

        // Draw the GrassBlock
        GraphicsDevice.SetVertexBuffer(grassVertexBuffer);
        GraphicsDevice.Indices = grassIndexBuffer;

        // Apply effect and draw the GrassBlock
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, grassBlock.indices.Length / 3);
        }

        // Draw the Cobblestone Block
        GraphicsDevice.SetVertexBuffer(cobblestoneVertexBuffer);
        GraphicsDevice.Indices = cobblestoneIndexBuffer;

        // Apply effect and draw the Cobblestone Block
        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, cobblestone.indices.Length / 3);
        }

        base.Draw(gameTime);
    }
}

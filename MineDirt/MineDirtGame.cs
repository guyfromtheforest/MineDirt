using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MineDirt.Src.Blocks;

namespace MineDirt;
public class MineDirtGame : Game
{
    public static GraphicsDeviceManager Graphics;
    private SpriteBatch _spriteBatch;

    Camera3D camera;
    public static Texture2D BlockTextures;

    BasicEffect effect;
    Chunk chunk;

    public MineDirtGame()
    {
        Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // Set to fullscreen
        Graphics.IsFullScreen = true;

        // Set the resolution for fullscreen mode
        Graphics.PreferredBackBufferWidth = 1920;  // Set your preferred width
        Graphics.PreferredBackBufferHeight = 1080; // Set your preferred height
        Graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        IsMouseVisible = false; // Hide the mouse cursor
        camera = new Camera3D(new Vector3(0, 10, 0), GraphicsDevice.Viewport.AspectRatio);

        chunk = new(Vector3.Zero);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        BlockTextures = Content.Load<Texture2D>("Textures/Blocks");

        //// Initialize the BasicEffect
        effect = new BasicEffect(GraphicsDevice)
        {
            VertexColorEnabled = false, // Disable color shading
            TextureEnabled = true,      // Enable texture mapping
            Texture = BlockTextures,     // Set the loaded texture
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

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        GraphicsDevice.RasterizerState = RasterizerState.CullNone;
        // Set texture sampling to Point to avoid blurriness (pixelated textures)
        GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

        //// Set the effect matrices for the camera
        effect.View = camera.View;
        effect.Projection = camera.Projection;


        chunk.Draw(effect); 

        base.Draw(gameTime);
    }
}

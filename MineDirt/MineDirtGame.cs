using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using MineDirt.Src.Blocks;
using System.Diagnostics;

namespace MineDirt;
public class MineDirtGame : Game
{
#if DEBUG
    MineDirt.Src.Debug debug = new();
#endif

    public static MineDirtGame Instance;
    public static GraphicsDeviceManager Graphics;
    public static bool IsMouseCursorVisible = false;

    public static Camera3D Camera;
    public static Texture2D BlockTextures;

    private SpriteBatch _spriteBatch;

    BasicEffect effect;
    Chunk chunk;

    private const int TargetUPS = 30;        // Updates per second
    private const int TargetFPS = 0;         // Frames per second (0 for uncapped)
    private double UpdateInterval => 1.0 / TargetUPS;
    private double FrameInterval => TargetFPS > 0 ? 1.0 / TargetFPS : 0.0;

    private double elapsedUpdateTime = 0;
    private double elapsedFrameTime = 0;

    private Stopwatch stopwatch = new Stopwatch();

    public MineDirtGame()
    {
        Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = IsMouseCursorVisible;

        // Uncap FPS
        Graphics.SynchronizeWithVerticalRetrace = false;
        IsFixedTimeStep = false;

        // Set updates per second to 30
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 64.0);

        // Set to fullscreen
        Graphics.IsFullScreen = true;

        // Set the resolution for fullscreen mode
        Graphics.PreferredBackBufferWidth = 1920;  // Set your preferred width
        Graphics.PreferredBackBufferHeight = 1080; // Set your preferred height
        Graphics.ApplyChanges();

        Instance = this;
    }

    protected override void Initialize()
    {
        Camera = new Camera3D(new Vector3(0, 10, 0), GraphicsDevice.Viewport.AspectRatio);

        chunk = new(Vector3.Zero);


#if DEBUG
        debug.Initialize();
#endif
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        BlockTextures = Content.Load<Texture2D>("Textures/Blocks");

        // Initialize the BasicEffect
        effect = new BasicEffect(GraphicsDevice)
        {
            VertexColorEnabled = false, // Disable color shading
            TextureEnabled = true,      // Enable texture mapping
            Texture = BlockTextures,     // Set the loaded texture
            View = Matrix.CreateLookAt(new Vector3(0, 0, 0), Vector3.Zero, Vector3.Up),
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.1f, 100f)
        };

#if DEBUG
        debug.LoadContent();
#endif
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        var keyboardState = Keyboard.GetState();
        var mouseState = Mouse.GetState();

        // Update the camera with the current input and GraphicsDevice for mouse centering
        Camera.Update(gameTime, keyboardState, mouseState, GraphicsDevice);

        IsMouseVisible = IsMouseCursorVisible;


#if DEBUG
        debug.Update(gameTime);
#endif
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
#if DEBUG
        debug.BeginDraw(gameTime);
#endif

        GraphicsDevice.Clear(Color.CornflowerBlue);
        GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        //// Set the effect matrices for the camera
        effect.View = Camera.View;
        effect.Projection = Camera.Projection;

        chunk.Draw(effect);

        base.Draw(gameTime);

#if DEBUG
        debug.EndDraw(gameTime);
#endif
    }
}

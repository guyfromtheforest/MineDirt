using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using MineDirt.Src;

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

    public static Noise Noise = new Noise(1234);

    BasicEffect effect;
    Chunk chunk; 
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

        chunk = new(Vector3.Zero);

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

        effect.View = Camera.View;
        effect.Projection = Camera.Projection;

        chunk.Draw(effect);

        base.Draw(gameTime);

#if DEBUG
        debug.EndDraw(gameTime);
#endif
    }
}

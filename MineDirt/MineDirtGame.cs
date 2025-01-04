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
    public static Texture2D TextureAtlas;

    private SpriteBatch _spriteBatch;

    public static FastNoiseLite Noise = new(1234);

    BasicEffect effect;
    Effect blockShader;
    
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
        Camera = new Camera3D(new Vector3(0, 100, 0), GraphicsDevice.Viewport.AspectRatio);
        World.Initialize();

        // Set noise parameters
        Noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        Noise.SetFrequency(0.02f);
        Noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        Noise.SetFractalOctaves(5);
        Noise.SetFractalLacunarity(2.0f);
        Noise.SetFractalGain(0.1f);
        Noise.SetFractalWeightedStrength(0.2f);

#if DEBUG
        debug.Initialize();
#endif
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        blockShader = Content.Load<Effect>("Shaders/BlockShader");
        TextureAtlas = Content.Load<Texture2D>("Textures/Blocks");

        // Initialize the BasicEffect
        effect = new BasicEffect(GraphicsDevice)
        {
            VertexColorEnabled = false, // Disable color shading
            TextureEnabled = true,      // Enable texture mapping
            Texture = TextureAtlas,     // Set the loaded texture
            View = Matrix.CreateLookAt(new Vector3(0, 0, 0), Vector3.Zero, Vector3.Up),
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.1f, 100f)
        };

        blockShader.Parameters["WorldViewProjection"].SetValue(Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.1f, 100f));
        // vertexShader.Parameters["PositionScale"].SetValue(Vector3.One);
        // vertexShader.Parameters["UVScale"].SetValue(Vector2.One);
        blockShader.Parameters["TextureAtlas"].SetValue(TextureAtlas);

        // Load the block textures
        Block.Load(BlockType.Dirt, [2]);
        Block.Load(BlockType.Grass, [1, 0, 2]);
        Block.Load(BlockType.Cobblestone, [3]);
        Block.Load(BlockType.Bedrock, [4]);
        Block.Load(BlockType.Stone, [5]);

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

        World.UpdateChunks();
        // chunk = new Chunk(new Vector3((float)Math.Floor(Camera.Position.X), 0, (float)Math.Floor(Camera.Position.Z)));

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

        blockShader.Parameters["WorldViewProjection"].SetValue(Camera.View * Camera.Projection);

        World.DrawChunks(blockShader);
        //chunk.Draw(effect);

        base.Draw(gameTime);

#if DEBUG
        debug.EndDraw(gameTime);
#endif
    }
}

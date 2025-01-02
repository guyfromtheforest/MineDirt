#if DEBUG
using MonoGame.ImGuiNet;
#endif

using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using MineDirt.Src.Blocks;

namespace MineDirt;
public class MineDirtGame : Game
{
    public static GraphicsDeviceManager Graphics;
    private SpriteBatch _spriteBatch;

    public static bool IsMouseCursorVisible = false;

    Camera3D camera;
    public static Texture2D BlockTextures;

    BasicEffect effect;
    Chunk chunk;

    private RasterizerState defaultRasterizeState = new()
    {
        FillMode = FillMode.Solid,
        CullMode = CullMode.None,
    };

#if DEBUG
    public static ImGuiRenderer GuiRenderer;

    private float fps = 0f;
    private float ups = 0f;
    private float fpsTimer = 0f;
    private float upsTimer = 0f;
    private int frameCount = 0;
    private int updateCount = 0;

    private bool RenderWireframes = false;

    private RasterizerState wireFrameRasterizeState = new()
    {
        FillMode = FillMode.WireFrame,
    };

#endif

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
    }

    protected override void Initialize()
    {
#if DEBUG
        GuiRenderer = new ImGuiRenderer(this);
#endif

        camera = new Camera3D(new Vector3(0, 10, 0), GraphicsDevice.Viewport.AspectRatio);

        chunk = new(Vector3.Zero);

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
        GuiRenderer.RebuildFontAtlas();
#endif
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        var keyboardState = Keyboard.GetState();
        var mouseState = Mouse.GetState();

        // Update the camera with the current input and GraphicsDevice for mouse centering
        camera.Update(gameTime, keyboardState, mouseState, GraphicsDevice);

        IsMouseVisible = IsMouseCursorVisible;

        base.Update(gameTime);

#if DEBUG
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Update UPS (Updates Per Second)
        upsTimer += deltaTime;
        updateCount++;
        if (upsTimer >= 1f) // Update every second
        {
            ups = updateCount;
            upsTimer -= 1f;  // Reset the timer
            updateCount = 0;
        }
#endif
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        GraphicsDevice.RasterizerState = defaultRasterizeState;
        // Set texture sampling to Point to avoid blurriness (pixelated textures)
        GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        if (RenderWireframes)
            GraphicsDevice.RasterizerState = wireFrameRasterizeState;

        //// Set the effect matrices for the camera
        effect.View = camera.View;
        effect.Projection = camera.Projection;

        chunk.Draw(effect);

        base.Draw(gameTime);

#if DEBUG
        GuiRenderer.BeginLayout(gameTime);

        // Calculate FPS (Frames Per Second)
        fpsTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        frameCount++;
        if (fpsTimer >= 1f) // Update every second
        {
            fps = frameCount;
            fpsTimer -= 1f; // Reset the timer
            frameCount = 0;
        }

        // Create an ImGui window for camera coordinates
        if (ImGui.Begin("Debug"))
        {
            ImGui.SetWindowSize(new System.Numerics.Vector2(320, 100));

            // Display the camera's position in the window
            ImGui.Text($"X: {camera.Position.X}, Y: {camera.Position.Y}, Z: {camera.Position.Z}");

            // Display FPS and UPS
            ImGui.Text($"FPS: {fps}");
            ImGui.Text($"UPS: {ups}");

            // Checkbox 
            ImGui.Checkbox("Render Wireframes", ref RenderWireframes);
        }
        ImGui.End();

        GuiRenderer.EndLayout();
#endif
    }
}

#if DEBUG
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.ImGuiNet;
using System;
namespace MineDirt.Src;

public class Debug
{
    public ImGuiRenderer GuiRenderer;

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

    private RasterizerState defaultRasterizeState = new()
    {
        FillMode = FillMode.Solid,
        CullMode = CullMode.None,
    };

    public void Initialize()
    {
        GuiRenderer = new ImGuiRenderer(MineDirtGame.Instance);
    }

    public void LoadContent()
    {
        GuiRenderer.RebuildFontAtlas();
    }

    public void Update(GameTime gameTime)
    {
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
    }

    public void EndDraw(GameTime gameTime)
    {
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
            ImGui.Text($"X: {MineDirtGame.Camera.Position.X}, Y: {MineDirtGame.Camera.Position.Y}, Z: {MineDirtGame.Camera.Position.Z}");

            // Display FPS and UPS
            ImGui.Text($"FPS: {fps}");
            ImGui.Text($"UPS: {ups}");

            // Checkbox 
            ImGui.Checkbox("Render Wireframes", ref RenderWireframes);
        }
        ImGui.End();

        GuiRenderer.EndLayout();
    }

    public void BeginDraw(GameTime gameTime)
    {
        if (RenderWireframes)
            MineDirtGame.Graphics.GraphicsDevice.RasterizerState = wireFrameRasterizeState;
        else
            MineDirtGame.Graphics.GraphicsDevice.RasterizerState = defaultRasterizeState;
    }
}

#endif
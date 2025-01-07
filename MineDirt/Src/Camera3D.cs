using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MineDirt.Src;

public class Camera3D
{
    public Vector3 Position { get; set; }
    public Vector3 Forward { get; private set; } = Vector3.Forward;
    public Vector3 Up { get; private set; } = Vector3.Up;

    public Matrix View { get; private set; }
    public Matrix Projection { get; private set; }

    private float yaw;
    private float pitch;

    public float ViewDistance { get; set; } = 1000.0f;
    public float FieldOfView { get; set; } = MathHelper.PiOver4;

    public static float MovementSpeed { get; set; } = 20.0f;
    public static float MaxMovementSpeed { get; } = 50.0f;

    private bool WasSprinting = false;

    public static float ReachDistance { get; set; } = 20.0f;
    public Block PointedBlock { get; set; }
    public Vector3 PointedBlockPosition;

    public Camera3D(Vector3 position, float aspectRatio)
    {
        Position = position;
        Projection = Matrix.CreatePerspectiveFieldOfView(
            FieldOfView,
            aspectRatio,
            0.1f,
            ViewDistance
        );
        UpdateViewMatrix();
    }

    private bool wasMenuMode = false; // To debounce the P key press
    private bool isMouseControlEnabled = true; // Flag to track if mouse control is enabled
    private bool isMouseCentered = true; // Flag to check if the mouse is centered

    public void Update(
        GameTime gameTime,
        KeyboardState keyboardState,
        MouseState mouseState,
        GraphicsDevice graphicsDevice
    )
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Movement and rotation speeds
        float rotationSpeed = 0.005f; // Smaller values for finer control
        float movementSpeed = MovementSpeed;

        if (keyboardState.IsKeyDown(Keys.LeftControl))
            movementSpeed = MaxMovementSpeed;

        // Center of the screen
        int centerX = graphicsDevice.Viewport.Width / 2;
        int centerY = graphicsDevice.Viewport.Height / 2;

        if (mouseState.Position.X == centerX && mouseState.Position.Y == centerY)
            isMouseCentered = true;

        if (mouseState.RightButton == ButtonState.Pressed && !wasMenuMode)
        {
            isMouseControlEnabled = !isMouseControlEnabled; // Toggle mouse control
            wasMenuMode = true;

            if (!isMouseControlEnabled)
            {
                // Stop the camera from jumping if the mouse is not centered
                if (!isMouseCentered)
                {
                    Mouse.SetPosition(centerX, centerY); // Only set if the mouse is not centered
                    isMouseCentered = true;
                }
                MineDirtGame.IsMouseCursorVisible = true; // Show the mouse cursor for interaction
            }
            else
            {
                // If returning to mouse-controlled mode, hide the cursor and center the mouse
                Mouse.SetPosition(centerX, centerY); // Ensure the mouse is centered
                isMouseCentered = false;
                MineDirtGame.IsMouseCursorVisible = false; // Hide the mouse cursor
            }
        }

        // Flag to handle debouncing key press (avoids rapid toggling)
        if (mouseState.RightButton == ButtonState.Released)
            wasMenuMode = false;

        // Camera follows the mouse if it's enabled
        if (isMouseControlEnabled)
        {
            // Calculate mouse movement (delta) only if the mouse is centered
            if (isMouseCentered)
            {
                int deltaX = mouseState.X - centerX;
                int deltaY = mouseState.Y - centerY;

                // Apply mouse delta to yaw and pitch
                yaw -= deltaX * rotationSpeed;
                pitch -= deltaY * rotationSpeed;

                // Clamp pitch to avoid flipping
                pitch = MathHelper.Clamp(
                    pitch,
                    -MathHelper.PiOver2 + 0.01f,
                    MathHelper.PiOver2 - 0.01f
                );

                // Reset the mouse position to the center of the screen
                Mouse.SetPosition(centerX, centerY);
            }
        }

        // Update forward and right vectors (camera movement directions)
        Matrix rotationMatrix = Matrix.CreateFromYawPitchRoll(yaw, pitch, 0);
        Forward = Vector3.Transform(Vector3.Forward, rotationMatrix);
        Vector3 right = Vector3.Cross(Forward, Up);

        // Normalize the movement vectors to prevent faster movement when moving diagonally
        Forward = Vector3.Normalize(Forward);
        right = Vector3.Normalize(right);

        // Movement with keyboard (WASD/Space/Shift for up/down)
        if (keyboardState.IsKeyDown(Keys.W))
        {
            // Prevent movement along the Y-axis
            Vector3 forwardMovement = Forward;
            forwardMovement.Y = 0; // Ignore Y-axis movement
            forwardMovement = Vector3.Normalize(forwardMovement);
            Position += forwardMovement * movementSpeed * deltaTime;
        }
        if (keyboardState.IsKeyDown(Keys.S))
        {
            // Prevent movement along the Y-axis
            Vector3 forwardMovement = Forward;
            forwardMovement.Y = 0; // Ignore Y-axis movement
            forwardMovement = Vector3.Normalize(forwardMovement);
            Position -= forwardMovement * movementSpeed * deltaTime;
        }
        if (keyboardState.IsKeyDown(Keys.A))
            Position -= right * movementSpeed * deltaTime;
        if (keyboardState.IsKeyDown(Keys.D))
            Position += right * movementSpeed * deltaTime;
        if (keyboardState.IsKeyDown(Keys.Space))
            Position += Up * movementSpeed * deltaTime;
        if (keyboardState.IsKeyDown(Keys.LeftShift))
            Position -= Up * movementSpeed * deltaTime;

        // Update the view matrix
        UpdateViewMatrix();

        // Update the pointed block
        UpdatePointedBlock();
    }

    public void UpdatePointedBlock()
    {
        Vector3 rayPosition = Position;
        Vector3 rayDirection = Vector3.Normalize(Forward);

        for (float t = 0; t < ReachDistance; t += 0.2f) // Adjust step size for precision/performance
        {
            rayPosition += rayDirection * 0.2f;
            Vector3 blockPos = Vector3.Floor(rayPosition); // Round down to get block coordinates

            if (World.TryGetBlock(blockPos, out Block block)) // Check if a block exists
            {
                if (block.Type == BlockType.Air)
                    continue;

                PointedBlockPosition = blockPos; // Set the pointed block coordinates
                PointedBlock = block; // Set the pointed block
                return;
            }
        }

        PointedBlockPosition = default;
        PointedBlock = default;
    }

    public void DrawBoundingBox(
        BoundingBox box,
        GraphicsDevice graphicsDevice,
        BasicEffect effect,
        float offset = 0.001f
    )
    {
        // Enable depth testing
        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        box.Min -= new Vector3(offset);
        box.Max += new Vector3(offset);

        // Define the corners of the bounding box
        Vector3[] corners = box.GetCorners();

        // Create a list to hold the offset corners
        Vector3[] offsetCorners = new Vector3[corners.Length];

        // Apply the offset to each corner
        for (int i = 0; i < corners.Length; i++)
        {
            // Offset each corner outward along the normal
            // We're using a simple approach to offset each vertex along the positive/negative X, Y, Z axes
            offsetCorners[i] = corners[i];
        }

        // Define the edges of the bounding box
        int[] indices =
        [
            0, 1, 1, 2, 2, 3, 3, 0, // Bottom face
            4, 5, 5, 6, 6, 7, 7, 4, // Top face
            0, 4, 1, 5, 2, 6, 3, 7, // Vertical edges
        ];

        // Set up vertices for the edges
        VertexPositionColor[] vertices = new VertexPositionColor[offsetCorners.Length];
        for (int i = 0; i < offsetCorners.Length; i++)
            vertices[i] = new VertexPositionColor(offsetCorners[i], Color.Black);

        // Apply the basic effect
        effect.VertexColorEnabled = true;
        effect.CurrentTechnique.Passes[0].Apply();

        // Draw the lines
        graphicsDevice.DrawUserIndexedPrimitives(
            PrimitiveType.LineList,
            vertices,
            0,
            vertices.Length,
            indices,
            0,
            indices.Length / 2
        );
    }

    private void UpdateViewMatrix() => View = Matrix.CreateLookAt(Position, Position + Forward, Up);
}
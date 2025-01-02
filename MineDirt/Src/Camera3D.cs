using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MineDirt;

public class Camera3D
{
    public Vector3 Position { get; private set; }
    public Vector3 Forward { get; private set; } = Vector3.Forward;
    public Vector3 Up { get; private set; } = Vector3.Up;

    public Matrix View { get; private set; }
    public Matrix Projection { get; private set; }

    private float yaw;
    private float pitch;

    public static float MovementSpeed { get; set; } = 20.0f;

    public Camera3D(Vector3 position, float aspectRatio, float fieldOfView = MathHelper.PiOver4, float nearPlane = 0.1f, float farPlane = 100f)
    {
        Position = position;
        Projection = Matrix.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearPlane, farPlane);
        UpdateViewMatrix();
    }

    private bool wasPKeyDown = false; // To debounce the P key press
    private bool isMouseControlEnabled = true; // Flag to track if mouse control is enabled
    private bool isMouseCentered = true; // Flag to check if the mouse is centered

    public void Update(GameTime gameTime, KeyboardState keyboardState, MouseState mouseState, GraphicsDevice graphicsDevice)
    {

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Movement and rotation speeds
        float rotationSpeed = 0.005f; // Smaller values for finer control
        float movementSpeed = MovementSpeed;

        // Center of the screen
        int centerX = graphicsDevice.Viewport.Width / 2;
        int centerY = graphicsDevice.Viewport.Height / 2;

        if(mouseState.Position.X == centerX && mouseState.Position.Y == centerY)
          isMouseCentered = true;  

        // Toggle camera mode with the 'P' key
        if (keyboardState.IsKeyDown(Keys.P) && !wasPKeyDown)  // Debounced press of 'P' key
        {
            isMouseControlEnabled = !isMouseControlEnabled; // Toggle mouse control
            wasPKeyDown = true;

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
        if (keyboardState.IsKeyUp(Keys.P))
            wasPKeyDown = false;

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
                pitch = MathHelper.Clamp(pitch, -MathHelper.PiOver2 + 0.01f, MathHelper.PiOver2 - 0.01f);

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
            Position += Forward * movementSpeed * deltaTime;
        if (keyboardState.IsKeyDown(Keys.S))
            Position -= Forward * movementSpeed * deltaTime;
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
    }

    private void UpdateViewMatrix()
    {
        View = Matrix.CreateLookAt(Position, Position + Forward, Up);
    }
}

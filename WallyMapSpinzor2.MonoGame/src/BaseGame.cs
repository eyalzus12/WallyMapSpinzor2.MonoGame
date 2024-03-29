using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WallyMapSpinzor2.MonoGame;

public class BaseGame : Game
{
    public const double MIN_ZOOM = 0.1;
    public const double MAX_ZOOM = 7;
    public const double ZOOM_PER_MSEC = 0.00005;
    public const double SPEED_MULT = 1;

    private GraphicsDeviceManager _graphics;
    public string BrawlPath{get; set;}
    public MonoGameCanvas? Canvas{get; set;} = null;
    public IDrawable ToDraw{get; set;}
    public Camera? Cam{get; set;}
    private double _windowScale = 1;

    public BaseGame(string brawlPath, IDrawable toDraw)
    {
        _graphics = new(this);
        ToDraw = toDraw;
        BrawlPath = brawlPath;
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        Window.Title = "WallyMapSpinzor2.MonoGame";
        Window.ClientSizeChanged += OnWindowResize;
    }

    private void OnWindowResize(object? sender, EventArgs e)
    {
        Canvas?.ResizeLineShader();
    }

    protected override void Update(GameTime gameTime)
    {
        Input.Update();

        if(Cam is not null) 
        {
            Cam.Zoom += ZOOM_PER_MSEC * Input.GetScrollWheelDelta() * gameTime.ElapsedGameTime.TotalMilliseconds;
            Cam.Zoom = Math.Clamp(Cam.Zoom, MIN_ZOOM, MAX_ZOOM);

            if(Input.IsMouseDown(MouseButtonEnum.Right))
            {
                (int x, int y) = Input.GetMouseDelta();
                Cam.X += x / (Cam.Zoom * _windowScale);
                Cam.Y += y / (Cam.Zoom * _windowScale);
            }
        }

        base.Update(gameTime);
    }

    private readonly RenderConfig _config = new()
    {
        //ShowNavNode = true
    };

    protected override void Draw(GameTime gameTime)
    {
        Canvas ??= new(GraphicsDevice, BrawlPath);
        GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);

        Cam ??= CreateCamera();

        double viewportW = GraphicsDevice.Viewport.Width;
        double viewportH = GraphicsDevice.Viewport.Height;

        Transform camTrans =
            Transform.CreateTranslate(viewportW / 2, viewportH / 2) * // set center to x=0, y=0 for scaling
            Transform.CreateScale(_windowScale, _windowScale) *
            Cam?.ToTransform() ?? Transform.IDENTITY;
        
        Canvas.CameraTransform = camTrans;
        ToDraw.DrawOn(Canvas, _config, Transform.IDENTITY, gameTime.TotalGameTime, new RenderData());
        Canvas.FinalizeDraw();
        base.Draw(gameTime);
    }

    private Camera? CreateCamera()
    {
        CameraBounds? bounds = ToDraw switch
        {
            LevelDesc ld => ld.CameraBounds,
            Level l => l.Desc.CameraBounds,
            _ => null
        };

        if(bounds is null) return null;

        double viewportW = GraphicsDevice.Viewport.Width;
        double viewportH = GraphicsDevice.Viewport.Height;
        _windowScale = Math.Min(viewportW / bounds.W, viewportH / bounds.H);

        return new(-bounds.X - viewportW / (2 * _windowScale), -bounds.Y - viewportH / (2 * _windowScale));
    }
}

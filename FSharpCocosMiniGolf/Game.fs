open CocosSharp
open System.Diagnostics

type PaddleState = 
    | MovingRight
    | MovingLeft 
    | Stopped

type GameLayer() as this = 
    inherit CCLayer()
    let paddle = new CCSprite("paddle")
    let ball = new CCSprite("ball")
    let hole = new CCSprite("hole")
    let score = new CCLabel("Score: 0", "arial", 22.f, CCLabelFormat.SpriteFont)
    let mutable ballYVelocity = 10.f
    let mutable ballXVelocity = 10.f
    let mutable paddleState = Stopped
    let mutable scoreValue = 0
    do 
        // Add paddle 
        paddle.PositionX <- 500.f
        paddle.PositionY <- 400.f
        base.AddChild(paddle)
        // Add ball
        ball.PositionX <- 300.f
        ball.PositionY <- 600.f
        base.AddChild(ball)
        // Add hole
        hole.PositionX <- 200.f
        hole.PositionY <- 200.f
        base.AddChild(hole)
        // Add label
        score.PositionX <- 100.f
        score.PositionY <- 700.f
        base.AddChild(score)
        base.Schedule(this.updateGame)
    
    member this.updateGame (frameTimeInSeconds) = 
        ball.PositionX <- ball.PositionX + ballXVelocity
        ball.PositionY <- ball.PositionY + ballYVelocity
        let ballRight = ball.BoundingBoxTransformedToParent.MaxX
        let ballLeft = ball.BoundingBoxTransformedToParent.MinX
        let ballTop = ball.BoundingBoxTransformedToParent.MaxY
        let ballDown = ball.BoundingBoxTransformedToParent.MinY
        let screenRight = base.VisibleBoundsWorldspace.MaxX
        let screenLeft = base.VisibleBoundsWorldspace.MinX
        let screenTop = base.VisibleBoundsWorldspace.MaxY
        let screenDown = base.VisibleBoundsWorldspace.MinY
        let shouldReflectXVelocity = 
            (ballRight > screenRight && ballXVelocity > 0.f) || (ballLeft < screenLeft && ballXVelocity < 0.f) 
            || (ballTop > screenTop && ballYVelocity > 0.f) || (ballDown < screenDown && ballYVelocity < 0.f)
        let doesBallOverlapPaddle = 
            ball.BoundingBoxTransformedToParent.IntersectsRect(paddle.BoundingBoxTransformedToParent)
        if shouldReflectXVelocity || doesBallOverlapPaddle then 
            if ballRight > screenRight || ballLeft < screenLeft  then
                ballXVelocity <- ballXVelocity * (-1.f)
            else
                ballYVelocity <- ballYVelocity * (-1.f)

        Debug.WriteLine(sprintf "ballXvelocity %A ballYVelocity %A" ballXVelocity ballYVelocity)
        // Check score
        if hole.BoundingBoxTransformedToParent.IntersectsRect(ball.BoundingBoxTransformedToParent) then 
            scoreValue <- scoreValue + 1
            score.Text <- sprintf "Score: %d" scoreValue

        // Process keyboard
        let refreshPaddlePositionSpeed = 0.2f
        match paddleState with
        | MovingLeft when paddle.PositionX >= 0.f -> 
            paddle.PositionX <- paddle.PositionX - 10.f
        | MovingRight when paddle.PositionX <= screenRight  -> 
            paddle.PositionX <- paddle.PositionX + 10.f
        | _ -> ()
    
    override this.AddedToScene() = 
        base.AddedToScene()
        let bounds = this.VisibleBoundsWorldspace
        let touchListener = new CCEventListenerKeyboard()
        let mutable keyPressed = true
        touchListener.OnKeyPressed <- fun ccKeyboardEvent -> 
            match ccKeyboardEvent.Keys with
            | CCKeys.Left -> paddleState <- MovingLeft
            | CCKeys.Right -> paddleState <- MovingRight
            | _ -> ()
        touchListener.OnKeyReleased <- fun ccKeyboardEvent -> 
            match ccKeyboardEvent.Keys with
            | CCKeys.Left | CCKeys.Right 
                when not(ccKeyboardEvent.KeyboardState.IsKeyDown(CCKeys.Right) 
                    || ccKeyboardEvent.KeyboardState.IsKeyDown(CCKeys.Left)) 
                -> paddleState <- Stopped
            | _ -> ()
        this.AddEventListener(touchListener, this)

type appDelegate() = 
    inherit CCApplicationDelegate()
    override x.ApplicationDidFinishLaunching((application : CCApplication), (mainWindow : CCWindow)) = 
        application.ContentRootDirectory <- "Content"
        let defaultResolution = 
            new CCSize(application.MainWindow.WindowSizeInPixels.Width, application.MainWindow.WindowSizeInPixels.Height)
        let scene = new CCScene(mainWindow)
        let gameLayer = new GameLayer()
        scene.AddChild(gameLayer)
        mainWindow.RunWithScene(scene)

module Program = 
    [<EntryPoint>]
    let Main(args) = 
        // main entry point return
        let application = new CCApplication(false, new System.Nullable<CCSize>(new CCSize(1024.f, 768.f)))
        application.ApplicationDelegate <- appDelegate()
        application.StartGame()
        0

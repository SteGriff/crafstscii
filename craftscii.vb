Option Explicit On
Option Strict On

Module craftscii

    Function Traversable(ByRef aBlock As BlockType) As Boolean

        Dim TraversableBlocks() As BlockType = {BlockType.Air, BlockType.Cave, BlockType.Water}
        Return TraversableBlocks.Contains(aBlock)

    End Function

    Sub BlockUI(ByVal World As Map, ByVal BlockGraphics() As BlockGraphic)

        PutCursor(0, World.Height + 1)
        For This As Byte = 1 To 7
            Console.ForegroundColor = BlockGraphics(This).ForeColour
            Console.BackgroundColor = BlockGraphics(This).BackColour
            Console.Write(This)
        Next

    End Sub

    Enum StateOfPlay
        Menu
        Game
        Sandbox
        Options
        Tutorial
    End Enum

    Enum DrawMode
        Fast
        Slow
    End Enum

    Function Tab(ByVal aString As String) As String
        Return vbTab + aString
    End Function

    Function UserShort(ByVal Wanted As String, ByVal GreaterThanThis As Integer, ByVal LessThanThis As Integer) As Short

        Dim Acceptable As Boolean = False
        Dim Result As Short = 0

        Do Until Acceptable
            Console.Write(Wanted & "?: ")
            Dim Input As Short = SafeShort(Console.ReadLine)
            If Input > GreaterThanThis And Input < LessThanThis Then
                Result = Input
                Acceptable = True
            Else
                Console.WriteLine(Wanted & " must be greater than " & GreaterThanThis & " and less than " & LessThanThis)
            End If
        Loop

        Return Result

    End Function

    Function ApplicationName(ByVal name As String, ByVal version As String) As String
        Return name + " " + version
    End Function

    Sub Main()

        Dim ProgramName As String = My.Application.Info.AssemblyName
        Dim ProgramVersion As String = "0.11"

        Randomize()
        Console.Title = ApplicationName(ProgramName, ProgramVersion)

        'Initial parameters for terrain generation
        Dim Start As Short = 10
        Dim Width As Short = 100
        Dim Height As Short = 22
        Dim WaterLevel As Short = 2

        Dim RequisiteWater As Short = 40
        Dim RequisiteCaves As Short = 20
        Dim RequisiteTrees As Short = 5
        Dim RequisiteDrySoil As Short = 64

        Do

            'Prepare Menu
            Dim Mode As StateOfPlay = StateOfPlay.Menu
            Dim SelectionAcceptable As Boolean = False

            Do Until SelectionAcceptable

                Console.Clear()

                Console.ForegroundColor = ConsoleColor.Green
                Console.WriteLine(vbNewLine & vbTab & ApplicationName(ProgramName, ProgramVersion) & vbNewLine)
                Console.WriteLine(Tab("SteGriff.co.uk"))
                Console.ForegroundColor = ConsoleColor.Cyan
                Console.WriteLine(Tab("Press R in-game to quit or restart. Check the Tutorial first!") & vbNewLine)

                Console.ResetColor()
                Console.WriteLine(Tab("1. Game"))
                Console.WriteLine(Tab("2. Sandbox"))
                Console.WriteLine(Tab("3. Tutorial"))
                Console.WriteLine(Tab("4. Quit"))
                Console.Write(vbNewLine & Tab("Type number, press enter: "))

                Dim Selection As Integer = SafeShort(Console.ReadLine)
                Select Case Selection
                    Case 1
                        Mode = StateOfPlay.Game
                        SelectionAcceptable = True

                    Case 2
                        Console.WriteLine(Tab("World Settings:"))

                        Width = UserShort("Width", 0, 32766)
                        Height = UserShort("Height", 19, 80)
                        WaterLevel = UserShort("Water level", 0, Height)

                        If Width < 20 Then
                            RequisiteWater = 0
                            RequisiteCaves = 0
                            RequisiteTrees = 0
                            RequisiteDrySoil = 1
                        Else
                            If WaterLevel > 1 Then
                                RequisiteWater = 8
                            Else
                                RequisiteWater = 1
                            End If

                            RequisiteCaves = 8
                            RequisiteTrees = 2
                            RequisiteDrySoil = 16
                        End If

                        Mode = StateOfPlay.Sandbox
                        SelectionAcceptable = True

                    Case 3
                        Mode = StateOfPlay.Tutorial
                        SelectionAcceptable = True

                    Case 4
                        End

                    Case Else
                        Console.WriteLine("I don't think that was an option bro.")
                        Console.ReadLine()

                End Select

            Loop


            'If window preparation goes awry, slow draw mode will become necessary
            Dim DrawMode As DrawMode = craftscii.DrawMode.Fast

            'Prepare window
            'Height
            Dim HeightCandidate As Short = CShort(Height + 1)
            If HeightCandidate < Console.BufferHeight Then
                'Don't shrink; not allowed.
            Else
                Console.BufferHeight = HeightCandidate
                Console.WindowHeight = HeightCandidate
            End If

            'Width
            Dim WidthCandidate As Short = CShort(Width + 1)
            If Width < Console.BufferWidth Then
                'Don't shrink; not allowed.
                DrawMode = DrawMode.Slow
                'We have to slow draw instead.
            Else
                Console.BufferWidth = WidthCandidate
                If WidthCandidate > 160 Then
                    Console.WindowWidth = 160
                Else
                    Console.WindowWidth = WidthCandidate
                End If

            End If

            Do Until Mode = StateOfPlay.Menu

                'Prepare World
                Dim World As New Map(Start, Height, Width, WaterLevel)
                World.GenerateRichWorld(RequisiteWater, RequisiteCaves, RequisiteTrees, RequisiteDrySoil)

                'Player
                Dim Player As New Player("Ste", "@", 0, 0)
                Player.Fall(World)

                'Monsters
                Dim NumberOfMonstersInPlay As Short = 0
                Dim Monsters(0) As Entity

                'Block graphics
                Dim BlockGraphics() As BlockGraphic
                BlockGraphics = CreateGraphics(Player)

                'Draw initial screen
                Console.Clear()
                If DrawMode = craftscii.DrawMode.Fast Then
                    DrawWorld(World.Width, World.Height, World.Blocks, BlockGraphics)
                Else
                    DrawWorldPart(World.Width, World.Height, World.Blocks, BlockGraphics)
                End If

                'Put player in world
                Player.Move(0, World, BlockGraphics)

                Select Case Mode
                    Case StateOfPlay.Game
                        Game(Mode, World, BlockGraphics, Player, Monsters, NumberOfMonstersInPlay)
                    Case StateOfPlay.Sandbox
                        Sandbox(Mode, World, BlockGraphics, Player)
                    Case StateOfPlay.Tutorial
                        Tutorial(Mode, World, BlockGraphics, Player, Monsters, NumberOfMonstersInPlay)
                End Select

            Loop

            'Jumps back to menu here.

        Loop

    End Sub

    Sub Game(ByRef Mode As StateOfPlay, ByVal World As Map, ByVal BlockGraphics() As BlockGraphic, ByVal Player As Player, ByVal Monsters() As Entity, ByVal NumberOfMonstersInPlay As Short)

        Dim SelectedBlock As BlockType = BlockType.Land

        Do Until Player.HP <= 0S

            'New Monsters!
            If NumberOfMonstersInPlay < 5 And Int(Rnd() * 20) = 0 Then
                AddMonster(Monsters, NumberOfMonstersInPlay, MonsterType.Zombie, World)
            End If

            Console.ResetColor()
            Player.DisplayInventory(World, BlockGraphics)

            PutCursor(Player.X + 1, Player.Y)
            Console.ResetColor()

            Dim Key As ConsoleKeyInfo
            Key = Console.ReadKey

            'Overwrite keypress
            DrawBlock(Player.X + 1S, Player.Y, World, BlockGraphics)

            'Key controls
            Dim xChange As Short = 0
            Dim X, Y As Short
            Dim SanitizedKey As String = Right(Key.KeyChar.ToString, 1)
            Dim TurnSpent As Boolean = True 'The only case in which the turn is not spent is a block selection.

            If IsNumeric(SanitizedKey) Then
                SelectedBlock = CType(SanitizedKey, BlockType)
                TurnSpent = False
            Else
                Select Case Key.KeyChar.ToString.ToLower

                    'Quit or restart
                    Case "r"
                        Player.HP = 0

                        'Move left and right
                    Case "m"
                        xChange = 1
                    Case "n"
                        xChange = -1

                        'Build and destroy
                    Case "w"
                        X = Player.X
                        Y = (Player.Y - 1S)
                    Case "a"
                        X = Player.X - 1S
                        Y = Player.Y
                    Case "s"
                        X = Player.X
                        Y = (Player.Y + 1S)
                    Case "d"
                        X = Player.X + 1S
                        Y = Player.Y

                End Select
            End If


            If "mn".Contains(Key.KeyChar.ToString.ToLower) Then
                Player.Move(xChange, World, BlockGraphics)
            End If


            Dim AntiY As Short = World.Height - Y

            If "wasd".Contains(Key.KeyChar.ToString) Then

                Player.Gain(World.Blocks(X, AntiY), 1)

                If AnyAdjacentBlockIs(BlockType.Water, X, AntiY, World) Then
                    World.Blocks(X, AntiY) = BlockType.Water
                Else
                    World.Blocks(X, AntiY) = BlockType.Air
                End If

                DrawBlock(X, Y, World, BlockGraphics)

                World.FloodAround(X, Player.Y, BlockGraphics)
                World.UpdateLighting()

                Player.Move(0, World, BlockGraphics)

            End If

            If "WASD".Contains(Key.KeyChar.ToString) Then

                Dim AllowBlockPlacement As Boolean = False

                Dim Accessible As Boolean
                Try
                    Accessible = Traversable(World.Blocks(X, AntiY))
                Catch
                    Accessible = False
                End Try

                If Accessible Then
                    AllowBlockPlacement = True
                Else
                    'To the top, left, or right, we shall not allow this.
                    If Key.Key = ConsoleKey.S Then 'If its the block below you, step up on top of placed block
                        Y = Player.Y
                        AntiY = World.Height - Y
                        If Traversable(World.Blocks(X, AntiY)) Then
                            AllowBlockPlacement = True
                        End If
                    End If
                End If

                If AllowBlockPlacement And Player.Has(SelectedBlock, 1) Then

                    AntiY = World.Height - Y 'Recalculate for any changes.

                    World.Blocks(X, AntiY) = SelectedBlock
                    Player.Spend(World.Blocks(X, AntiY), 1)

                    DrawBlock(X, Y, World, BlockGraphics)
                    Player.Move(0, World, BlockGraphics)

                End If

            End If

            'Move Monsters only if we've spent the turn usefully.
            If NumberOfMonstersInPlay > 0 And TurnSpent Then
                For Each Enemy As Entity In Monsters

                    'Move first
                    If Int(Rnd() * 3) <> 0 Then

                        Dim EnemyXChange As Short = 0
                        If Player.X > Enemy.X Then
                            EnemyXChange = 1
                        ElseIf Player.X < Enemy.X Then
                            EnemyXChange = -1
                        Else
                            EnemyXChange = 0
                        End If
                        Enemy.Move(EnemyXChange, World, BlockGraphics)

                        'Mauling you?
                        If Enemy.X = Player.X And Enemy.Y = Player.Y Then
                            If Player.HP > 0 Then
                                Player.Damage(1)
                            End If
                        End If

                    End If

                    'Then, if it can attack, do so.

                Next
            End If

            PutCursor(Player.X + 1, Player.Y)
            'Console.ReadLine()

        Loop

        PutCursor(0, 0)
        Console.ResetColor()
        Console.WriteLine("You're dead. You had 2HP at the start... did I mention that?")
        Console.WriteLine("Press Enter to regenerate, or Q followed by Enter to quit.")

        Dim Selection As String = Console.ReadLine()
        If Selection.ToLower = "q" Then
            Mode = StateOfPlay.Menu
        End If

    End Sub

    Sub Sandbox(ByRef Mode As StateOfPlay, ByVal World As Map, ByVal BlockGraphics() As BlockGraphic, ByVal Player As Player)

        'Monster- and Inventory-free!

        Dim SelectedBlock As BlockType = BlockType.Rock

        Console.ResetColor()
        BlockUI(World, BlockGraphics)

        Do Until Player.HP <= 0

            PutCursor(Player.X + 1, Player.Y)
            Console.ResetColor()

            Dim Key As ConsoleKeyInfo
            Key = Console.ReadKey

            'Overwrite keypress
            DrawBlock(Player.X + 1S, Player.Y, World, BlockGraphics)

            'Key controls
            Dim xChange As Short = 0
            Dim X, Y As Short
            Dim SanitizedKey As String = Right(Key.KeyChar.ToString, 1)

            If IsNumeric(SanitizedKey) Then
                SelectedBlock = CType(SanitizedKey, BlockType)
            Else
                Select Case Key.KeyChar.ToString.ToLower

                    'Quit or restart
                    Case "r"
                        Player.HP = 0

                        'Move left and right
                    Case "m"
                        xChange = 1
                    Case "n"
                        xChange = -1

                        'Build and destroy
                    Case "w"
                        X = Player.X
                        Y = (Player.Y - 1S)
                    Case "a"
                        X = Player.X - 1S
                        Y = Player.Y
                    Case "s"
                        X = Player.X
                        Y = (Player.Y + 1S)
                    Case "d"
                        X = Player.X + 1S
                        Y = Player.Y

                End Select
            End If


            If "mn".Contains(Key.KeyChar.ToString.ToLower) Then
                Player.Move(xChange, World, BlockGraphics)
            End If


            Dim AntiY As Short = World.Height - Y
            If AntiY >= 0 Then

                If "wasd".Contains(Key.KeyChar.ToString) Then

                    Player.Gain(World.Blocks(X, AntiY), 1)

                    If AnyAdjacentBlockIs(BlockType.Water, X, AntiY, World) Then
                        World.Blocks(X, AntiY) = BlockType.Water
                    Else
                        World.Blocks(X, AntiY) = BlockType.Air
                    End If

                    DrawBlock(X, Y, World, BlockGraphics)

                    World.FloodAround(X, Player.Y, BlockGraphics)
                    World.UpdateLighting()

                    Player.Move(0, World, BlockGraphics)

                End If

                If "WASD".Contains(Key.KeyChar.ToString) Then

                    Dim AllowBlockPlacement As Boolean = False

                    Dim Accessible As Boolean
                    Try
                        Accessible = Traversable(World.Blocks(X, AntiY))
                    Catch
                        Accessible = False
                    End Try

                    If Accessible Then
                        AllowBlockPlacement = True
                    Else
                        'To the top, left, or right, we shall not allow this.
                        If Key.Key = ConsoleKey.S Then 'If its the block below you, step up on top of placed block
                            Y = Player.Y
                            AntiY = World.Height - Y
                            If Traversable(World.Blocks(X, AntiY)) Then
                                AllowBlockPlacement = True
                            End If
                        End If
                    End If

                    If AllowBlockPlacement Then

                        AntiY = World.Height - Y 'Recalculate for any changes.
                        World.Blocks(X, AntiY) = SelectedBlock

                        DrawBlock(X, Y, World, BlockGraphics)
                        Player.Move(0, World, BlockGraphics)

                    End If

                End If


            End If

            PutCursor(Player.X + 1, Player.Y)

        Loop

        PutCursor(0, 0)
        Console.ResetColor()
        Console.WriteLine("You have ended the game.")
        Console.WriteLine("Press Enter to regenerate, or Q followed by Enter to quit.")

        Dim Selection As String = Console.ReadLine()
        If Selection.ToLower = "q" Then
            Mode = StateOfPlay.Menu
        End If

    End Sub

    Function NextStage(ByVal currentStage As TutorialStages) As TutorialStages
        Return CType(currentStage + 1, TutorialStages)
    End Function

    Enum TutorialStages
        Movement = 0
        Climbing = 1
        Destruction = 2
        BlockSelection = 3
        Addition = 4
        Monsters = 5
    End Enum

    Sub Tutorial(ByRef Mode As StateOfPlay, ByVal World As Map, ByVal BlockGraphics() As BlockGraphic, ByVal Player As Player, ByVal Monsters() As Entity, ByVal NumberOfMonstersInPlay As Short)

        Dim SelectedBlock As BlockType = BlockType.Land

        Dim Objective As TutorialStages = 0
        Dim ObjectivePoints As Short = 0

        Do Until Player.HP <= 0

            Select Case Objective
                Case TutorialStages.Movement
                    Instruction("Welcome! To move left and right, use the m and n keys.{0}Climbing is automatic, and takes a little getting used to. Move around now.", Player)
                    If ObjectivePoints >= 20 Then
                        ObjectivePoints = 0
                        Objective = NextStage(Objective)
                    End If
                Case TutorialStages.Climbing
                    Instruction("Think of the player character as being on a layer{0}closer to the screen than the world is.{0}If you move across a block, you will climb it, at a '45 degree' angle.", Player)
                    If ObjectivePoints >= 20 Then
                        ObjectivePoints = 0
                        Objective = NextStage(Objective)
                    End If
                Case TutorialStages.Destruction
                    Instruction("OK. To break blocks around you, use the wasd keys.{0}You can only break blocks which are 'touching' you.", Player)
                    If ObjectivePoints >= 5 Then
                        ObjectivePoints = 0
                        Objective = NextStage(Objective)
                    End If
                Case TutorialStages.BlockSelection
                    Instruction("Now you should have 5 or more blocks. Look at the bottom strip of the screen:{0}Press a number key to select a [block] to build with.", Player)
                    If ObjectivePoints >= 1 Then
                        ObjectivePoints = 0
                        Objective = NextStage(Objective)
                    End If
                Case TutorialStages.Addition
                    Instruction("To place one, hold Shift and use the wasd keys.{0}You can use Caps Lock for long periods of building.", Player)
                    If ObjectivePoints >= 5 Then
                        ObjectivePoints = 0
                        Objective = NextStage(Objective)
                    End If
                Case TutorialStages.Monsters
                    Instruction("A horde of Zombies is coming! If one touches you twice, you die.{0}Fighting back hasn't been added yet :(", Player)
                    If ObjectivePoints >= 20 Then
                        Instruction("This is essentially the end of the tutorial.{0}Press r at any time to end the game.", Player)
                    End If
            End Select

            If Objective >= TutorialStages.Monsters Then
                AddMonster(Monsters, NumberOfMonstersInPlay, MonsterType.Zombie, World)
            End If

            Console.ResetColor()
            Player.DisplayInventory(World, BlockGraphics)

            PutCursor(Player.X + 1, Player.Y)
            Console.ResetColor()

            Dim Key As ConsoleKeyInfo
            Key = Console.ReadKey

            'Overwrite keypress
            DrawBlock(Player.X + 1S, Player.Y, World, BlockGraphics)

            'Key controls
            Dim xChange As Short = 0
            Dim X, Y As Short
            Dim SanitizedKey As String = Right(Key.KeyChar.ToString, 1)
            Dim TurnSpent As Boolean = True 'The only case in which the turn is not spent is a block selection.

            If IsNumeric(SanitizedKey) Then
                SelectedBlock = CType(SanitizedKey, BlockType)
                TurnSpent = False

                If Objective = TutorialStages.BlockSelection Then
                    ObjectivePoints += 2S
                End If

            Else
                Select Case Key.KeyChar.ToString.ToLower

                    'Quit or restart
                    Case "r"
                        Player.HP = 0

                        'Move left and right
                    Case "m"
                        xChange = 1
                    Case "n"
                        xChange = -1

                        'Build and destroy
                    Case "w"
                        X = Player.X
                        Y = (Player.Y - 1S)
                    Case "a"
                        X = Player.X - 1S
                        Y = Player.Y
                    Case "s"
                        X = Player.X
                        Y = (Player.Y + 1S)
                    Case "d"
                        X = Player.X + 1S
                        Y = Player.Y

                End Select
            End If


            If "mn".Contains(Key.KeyChar.ToString.ToLower) Then
                Player.Move(xChange, World, BlockGraphics)
                If Objective = TutorialStages.Movement Or Objective = TutorialStages.Climbing Or Objective = TutorialStages.Monsters Then
                    ObjectivePoints += 1S
                End If
            End If


            Dim AntiY As Short = World.Height - Y

            If "wasd".Contains(Key.KeyChar.ToString) And Objective >= TutorialStages.Destruction Then

                Player.Gain(World.Blocks(X, AntiY), 1)

                'Learner is completing objective if destroying any solid block.
                If Objective = TutorialStages.Destruction And (Not Traversable(World.Blocks(X, AntiY))) Then
                    ObjectivePoints += 1S
                End If

                If AnyAdjacentBlockIs(BlockType.Water, X, AntiY, World) Then
                    World.Blocks(X, AntiY) = BlockType.Water
                Else
                    World.Blocks(X, AntiY) = BlockType.Air
                End If

                DrawBlock(X, Y, World, BlockGraphics)

                World.FloodAround(X, Player.Y, BlockGraphics)
                World.UpdateLighting()

                Player.Move(0, World, BlockGraphics)

            End If

            If "WASD".Contains(Key.KeyChar.ToString) And Objective >= TutorialStages.Addition Then

                Dim AllowBlockPlacement As Boolean = False

                Dim Accessible As Boolean
                Try
                    Accessible = Traversable(World.Blocks(X, AntiY))
                Catch
                    Accessible = False
                End Try

                If Accessible Then
                    AllowBlockPlacement = True
                Else
                    'To the top, left, or right, we shall not allow this.
                    If Key.Key = ConsoleKey.S Then 'If its the block below you, step up on top of placed block
                        Y = Player.Y
                        AntiY = World.Height - Y
                        If Traversable(World.Blocks(X, AntiY)) Then
                            AllowBlockPlacement = True
                        End If
                    End If
                End If

                If AllowBlockPlacement And Player.Has(SelectedBlock, 1) Then

                    AntiY = World.Height - Y 'Recalculate for any changes.

                    World.Blocks(X, AntiY) = SelectedBlock
                    Player.Spend(World.Blocks(X, AntiY), 1)

                    If Objective = TutorialStages.Addition Then
                        ObjectivePoints += 1S
                    End If

                    DrawBlock(X, Y, World, BlockGraphics)
                    Player.Move(0, World, BlockGraphics)

                End If

            End If

            'Move monsters if that's ok.
            If NumberOfMonstersInPlay > 0 And TurnSpent And Objective >= TutorialStages.Monsters Then
                For Each Enemy As Entity In Monsters

                    'Move first
                    If Int(Rnd() * 3) <> 0 Then

                        Dim EnemyXChange As Short = 0
                        If Player.X > Enemy.X Then
                            EnemyXChange = 1
                        ElseIf Player.X < Enemy.X Then
                            EnemyXChange = -1
                        Else
                            EnemyXChange = 0
                        End If
                        Enemy.Move(EnemyXChange, World, BlockGraphics)

                        'Mauling you?
                        If Enemy.X = Player.X And Enemy.Y = Player.Y Then
                            If Player.HP > 0 Then
                                Player.Damage(1)
                            End If
                        End If

                    End If

                    'Then, if it can attack, do so.

                Next
            End If

            PutCursor(Player.X + 1, Player.Y)
            'Console.ReadLine()

        Loop

        PutCursor(0, 0)
        Console.ResetColor()
        Console.WriteLine("You're dead. You had 2HP at the start... did I mention that?")
        Console.WriteLine("Press Enter to regenerate, or Q followed by Enter to quit.")

        Dim Selection As String = Console.ReadLine()
        If Selection.ToLower = "q" Then
            Mode = StateOfPlay.Menu
        End If

    End Sub

    Sub Instruction(ByVal Text As String, ByVal Player As Player)

        PutCursor(0, 0)
        Console.ResetColor()

        Console.WriteLine(Space(79))
        Console.WriteLine(Space(79))
        Console.WriteLine(Space(79))
        PutCursor(0, 0)

        Console.WriteLine(Text, vbNewLine)

        PutCursor(Player.X + 1, Player.Y)

    End Sub

    Enum MonsterType
        Zombie
    End Enum

    Sub AddMonster(ByRef Monsters() As Entity, ByRef NumberOfMonstersInPlay As Short, ByVal Type As MonsterType, ByVal World As Map)

        NumberOfMonstersInPlay += CShort(1)

        'Where to spawn
        Dim SpawnInTheEast As Boolean = (Int(Rnd() * 2) = 0)
        Dim SpawnX As Short
        If SpawnInTheEast Then
            'EAST
            SpawnX = 0
        Else
            'WEST
            SpawnX = World.Width
        End If

        'Design Monster Here
        Dim Candidate As Entity = New Entity(Type.ToString, Type.ToString.Substring(0, 1), SpawnX, 1, 1)

        'Creation
        Dim This As Short = CShort(NumberOfMonstersInPlay - 1)
        Array.Resize(Monsters, NumberOfMonstersInPlay)
        Monsters(This) = Candidate
        Monsters(This).Fall(World)

    End Sub

    Function Below(ByVal X As Short, ByVal Y As Short, ByVal World As Map) As Coordinate

        Dim yBelow As Short = CShort(Math.Max(0, Y - 1))
        Dim CoordBelow As Coordinate = New Coordinate(X, yBelow)
        Return CoordBelow

    End Function

    Function AdjacentBlocks(ByVal X As Short, ByVal Y As Short, ByVal World As Map) As Coordinate()
        Dim Coordinates(4) As Coordinate
        Dim xLeft As Short = CShort(Math.Max(0, X - 1))
        Dim xRight As Short = CShort(Math.Min(World.Width, X + 1))
        Dim yAbove As Short = CShort(Math.Min(World.Height, Y + 1))
        Dim yBelow As Short = CShort(Math.Max(0, Y - 1))

        Coordinates(0) = New Coordinate(xLeft, Y)
        Coordinates(1) = New Coordinate(xRight, Y)
        Coordinates(2) = New Coordinate(X, yBelow)
        Coordinates(3) = New Coordinate(X, yAbove)

        Return Coordinates

    End Function

    Function AnyAdjacentBlockIs(ByVal BlockType As BlockType, ByVal X As Short, ByVal Y As Short, ByVal World As Map) As Boolean

        Dim Result As Boolean = False
        Dim Coordinates() As Coordinate = AdjacentBlocks(X, Y, World)

        For Each Adjacent As Coordinate In Coordinates
            If World.Blocks(Adjacent.X, Adjacent.Y) = BlockType.Water Then
                Result = True
            End If
        Next

        Return Result
    End Function

    Function CreateTestGraphics() As BlockGraphic()

        Dim Blocks(8) As BlockGraphic
        For i As Byte = 0 To 8
            Blocks(i) = New BlockGraphic(CType(i, BlockType), "X"c, ConsoleColor.Magenta, ConsoleColor.Black)
        Next

        Return Blocks

    End Function

    Function CreateGraphics(ByVal Player As Player) As BlockGraphic()

        Dim Blocks(9) As BlockGraphic
        Blocks(BlockType.Air) = New BlockGraphic(BlockType.Air, " "c, ConsoleColor.Black, ConsoleColor.Black)
        Blocks(BlockType.Cave) = New BlockGraphic(BlockType.Cave, "."c, ConsoleColor.DarkGray, ConsoleColor.Black)
        Blocks(BlockType.Land) = New BlockGraphic(BlockType.Land, "-"c, ConsoleColor.Green, ConsoleColor.DarkGreen)
        Blocks(BlockType.Leaves) = New BlockGraphic(BlockType.Leaves, "."c, ConsoleColor.DarkGreen, ConsoleColor.Green)
        Blocks(BlockType.Rock) = New BlockGraphic(BlockType.Rock, "-"c, ConsoleColor.Gray, ConsoleColor.DarkGray)
        Blocks(BlockType.Seabed) = New BlockGraphic(BlockType.Seabed, "-"c, ConsoleColor.DarkGreen, ConsoleColor.DarkCyan)
        Blocks(BlockType.Trunk) = New BlockGraphic(BlockType.Trunk, " "c, ConsoleColor.DarkYellow, ConsoleColor.DarkYellow)
        Blocks(BlockType.Water) = New BlockGraphic(BlockType.Water, "~"c, ConsoleColor.Blue, ConsoleColor.DarkBlue)
        Blocks(BlockType.Void) = New BlockGraphic(BlockType.Void, "X"c, ConsoleColor.Magenta, ConsoleColor.Black)

        Return Blocks

    End Function

    Sub DrawWorld(ByVal Width As Short, ByVal Height As Short, ByVal World(,) As BlockType, ByVal Blocks() As BlockGraphic)

        For y As Short = Height To 0 Step -1

            Dim LineHasContent As Boolean = False

            For x As Short = 0 To Width
                If World(x, y) <> BlockType.Air Then
                    LineHasContent = True
                End If
            Next

            If LineHasContent Then
                For x As Short = 0 To Width

                    Console.BackgroundColor = Blocks(World(x, y)).BackColour
                    Console.ForegroundColor = Blocks(World(x, y)).ForeColour
                    Console.Write(Blocks(World(x, y)).Character)

                Next
            Else
                Console.WriteLine()
                'MsgBox(y & " has no content")
            End If

        Next

    End Sub
    Sub DrawWorldPart(ByVal Width As Short, ByVal Height As Short, ByVal World(,) As BlockType, ByVal Blocks() As BlockGraphic)

        For y As Short = Height To 0 Step -1
            Console.CursorTop = Height - y
            For x As Short = 0 To Width
                Console.CursorLeft = x
                Console.BackgroundColor = Blocks(World(x, y)).BackColour
                Console.ForegroundColor = Blocks(World(x, y)).ForeColour
                Console.Write(Blocks(World(x, y)).Character)
            Next

        Next

    End Sub

    Sub DrawBlock(ByVal x As Short, ByVal y As Short, ByVal World As Map, ByVal Blocks() As BlockGraphic)

        Dim MapY As Short = World.Height - y

        PutCursor(x, y)
        Console.BackgroundColor = Blocks(World.Blocks(x, MapY)).BackColour
        Console.ForegroundColor = Blocks(World.Blocks(x, MapY)).ForeColour
        Console.Write(Blocks(World.Blocks(x, MapY)).Character)

    End Sub
    Sub DrawBlockA(ByVal x As Short, ByVal AntiY As Short, ByVal World As Map, ByVal Blocks() As BlockGraphic)

        PutCursor(x, AntiY)
        Dim MapY As Short = World.Height - AntiY
        Console.BackgroundColor = Blocks(World.Blocks(x, MapY)).BackColour
        Console.ForegroundColor = Blocks(World.Blocks(x, MapY)).ForeColour
        Console.Write(Blocks(World.Blocks(x, MapY)).Character)

    End Sub
    Sub PutCursor(ByVal Left As Integer, ByVal Top As Integer)
        Console.SetCursorPosition(Left, Top)
    End Sub
    Function SafeShort(ByVal anInput As String) As Short
        If IsNumeric(anInput) Then
            Return CShort(anInput)
        Else
            Return -1
        End If
    End Function
End Module

Module MapGenerator

    Structure Map

        Dim Height As Short
        Dim Width As Short
        Dim WaterLevel As Short
        Dim Blocks(,) As BlockType
        Dim NumberOfTrees As Short
        Dim NumberOfCaves As Short
        Dim MeasureOfWater As Short
        Dim AmountOfDrySoil As Short

        Function Ground(ByVal x As Short, ByVal YourY As Short)

            For y As Short = Height - YourY To 0 Step -1
                If Not Traversable(Blocks(x, y)) Then
                    Return Height - (y + 1)
                End If
            Next

            Throw New ApplicationException("There is no ground.")

        End Function

        Sub New(ByVal Start As Short, ByVal Height As Short, ByVal Width As Short, ByVal WaterLevel As Short)

            Me.Height = Height
            Me.Width = Width
            Me.WaterLevel = WaterLevel
            ReDim Me.Blocks(Width, Height)

        End Sub
        Sub ClearBlocks()

            For x As Short = 0 To Width
                For y As Short = 0 To Height
                    Me.Blocks(x, y) = BlockType.Air
                Next
            Next

        End Sub

        Sub FloodAround(ByVal X As Short, ByVal Y As Short, ByVal Graphics() As BlockGraphic)

            Dim Coordinates() As Coordinate = AdjacentBlocks(X, Y, Me)

            For ThisCoordinate As Byte = 0 To 3

                Dim ThisX As Short = Coordinates(ThisCoordinate).X
                Dim ThisY As Short = Me.Height - Coordinates(ThisCoordinate).Y

                If Me.Blocks(ThisX, ThisY) = BlockType.Water Then

                    For FloodCoordinate As Byte = 0 To 3
                        Dim FloodX As Short = Coordinates(FloodCoordinate).X
                        Dim FloodY As Short = Coordinates(FloodCoordinate).Y
                        Dim FloodAntiY As Short = Me.Height - Coordinates(FloodCoordinate).Y

                        If FloodAntiY <= WaterLevel And Me.Blocks(FloodX, FloodAntiY) = BlockType.Air Then
                            Me.Blocks(FloodX, FloodAntiY) = BlockType.Water
                            DrawBlockA(FloodX, FloodY, Me, Graphics)
                            Me.FloodAround(FloodX, FloodY, Graphics)
                        End If

                    Next

                End If

            Next

        End Sub

        Sub UpdateLighting()

            For x As Short = 0 To Me.Width
                For y As Short = Me.Height To 0 Step -1
                    If Me.Blocks(x, y) Then

                    End If
                Next
            Next

        End Sub
        Function Generate(ByVal Start As Short, ByVal Distance As Short)

            ClearBlocks()
            Me.MeasureOfWater = 0
            Me.NumberOfCaves = 0
            Me.NumberOfTrees = 0
            Me.AmountOfDrySoil = 0

            Dim Elevation As Short = 5
            Dim Gradient As Short = 0

            'Work from left to right
            For x As Short = Start To Start + Distance

                Elevation = LinearElevation(Elevation)

                'Land and aquifiers
                For y As Short = 0 To Elevation
                    If y > WaterLevel Then
                        Me.Blocks(x, y) = BlockType.Land
                        If AmountOfDrySoil < 32767 Then
                            AmountOfDrySoil += 1
                        End If
                    Else
                        Me.Blocks(x, y) = BlockType.Seabed
                    End If

                Next

                'Water
                For y As Short = Elevation + 1 To WaterLevel
                    Me.Blocks(x, y) = BlockType.Water
                    If MeasureOfWater < 32767 Then
                        MeasureOfWater += 1
                    End If
                Next

                'Caves
                If Elevation > 6 Then
                    Dim CaveMedian As Short = Elevation / 2
                    Dim CaveFloor As Short = CaveMedian - Int(Rnd() * (CaveMedian / 2))
                    Dim CaveRoof As Short = CaveMedian + Int(Rnd() * (CaveMedian / 2))
                    For y As Short = CaveRoof To CaveFloor Step -1
                        Me.Blocks(x, y) = BlockType.Cave
                    Next
                    Me.Blocks(x, CaveFloor - 1) = BlockType.Rock
                    If NumberOfCaves < 32767 Then
                        NumberOfCaves += 1
                    End If
                End If

                'Trees
                If x Mod 5 = 0 And Int(Rnd() * 3) = 0 And Elevation > WaterLevel Then
                    If NumberOfTrees < 32767 Then
                        NumberOfTrees += 1
                    End If
                    Dim TreeBase As Short = Elevation + 1
                    Dim TreeHeight As Byte = Int(Rnd() * 5)
                    Dim TreeTop As Short = TreeBase + TreeHeight
                    TreeTop = Math.Min(Me.Height - 1, TreeTop)
                    Dim Branches As Short = TreeBase + Math.Max(1, TreeHeight / 2)

                    For y As Short = TreeBase To TreeTop
                        Me.Blocks(x, y) = BlockType.Trunk
                    Next

                    Dim TreeHalfWidth As Short = 1
                    Select Case TreeHeight
                        Case Is < 1
                            TreeHalfWidth = 0
                        Case 1 To 2
                            TreeHalfWidth = 1
                        Case Is > 2
                            TreeHalfWidth = 2
                    End Select

                    'Don't go outside the x-confines of the screen.
                    Dim LeftEdge As Short = Math.Max(0, x - TreeHalfWidth)
                    Dim RightEdge As Short = Math.Min(Me.Width, x + TreeHalfWidth)

                    For ty As Short = Branches To TreeTop + 1
                        For tx As Short = LeftEdge To RightEdge

                            If tx = x Then 'Middle of tree

                                If ty > TreeTop Then
                                    Me.Blocks(tx, ty) = BlockType.Leaves
                                End If

                            Else
                                Me.Blocks(tx, ty) = BlockType.Leaves
                            End If

                        Next
                    Next

                    'For tallest trees
                    If TreeHeight >= 3 Then 'Maybe trim corners
                        Dim ShallTrimTop As Boolean = (Int(Rnd() * 2) = 0)
                        Dim ShallTrimBottom As Boolean = (Int(Rnd() * 2) = 0)

                        If x - TreeHalfWidth > 0 Then
                            If ShallTrimTop Then Me.Blocks(x - TreeHalfWidth, TreeTop + 1) = BlockType.Air
                            If ShallTrimBottom Then Me.Blocks(x - TreeHalfWidth, Branches) = BlockType.Air
                        End If
                        If x + TreeHalfWidth < Me.Width Then
                            If ShallTrimTop Then Me.Blocks(x + TreeHalfWidth, TreeTop + 1) = BlockType.Air
                            If ShallTrimBottom Then Me.Blocks(x + TreeHalfWidth, Branches) = BlockType.Air
                        End If
                    End If

                End If

            Next

            Return Elevation

        End Function
        Function LinearElevation(ByVal Elevation As Short)

            Dim Sky As Short = Height * 0.75

            Dim NewElevation As Short
            NewElevation = Elevation - 1 + Int(Rnd() * 3)
            If NewElevation < 0 Then
                NewElevation = 0
            ElseIf NewElevation >= Sky Then
                NewElevation = Sky
            End If
            Return NewElevation

        End Function
        Function GradientElevation(ByVal Elevation As Short, ByVal Gradient As Short)

            Dim Sky As Short = Height / 2

            'Vary terrain gradient
            Gradient = VaryGradient(Gradient)

            'Dampen gradient at join with skybox
            If Elevation = Sky Then
                If Gradient > 0 Then
                    Gradient = 0
                Else
                    Gradient = -1
                End If
            ElseIf Elevation <= 0 Then
                Gradient = 1
            End If

            'Prevent skybox intrusion
            Elevation = Math.Min(Elevation + Gradient, Sky)
            'Prevent ground intrusion
            Elevation = Math.Max(Elevation, 0)

            Return Elevation

        End Function

        Function VaryGradient(ByVal Gradient As Short)

            Dim Limit As Short = -1

            Gradient += (Int(Rnd() * ((4 * Math.Abs(Limit)) - 1)) - 1)
            If Gradient > Math.Abs(Limit) Then
                Gradient = Math.Abs(Limit)
            ElseIf Gradient < Limit Then
                Gradient = Limit
            End If

            Return Gradient

        End Function

        Sub GenerateRichWorld(ByVal Water As Short, ByVal Caves As Short, ByVal Trees As Short, ByVal DrySoil As Short)

            Console.Clear()
            Console.WriteLine("Generating appropriate map...")
            Dim NumberOfAttempts As Integer = 0
            Console.Write("Attempt ")

            Do Until (Me.MeasureOfWater >= Water) And (Me.NumberOfCaves >= Caves) And (Me.NumberOfTrees > Trees) And (Me.AmountOfDrySoil > DrySoil)
                NumberOfAttempts += 1
                Me.Generate(0, Me.Width)
                Console.Write(NumberOfAttempts)
                Console.CursorLeft -= NumberOfAttempts.ToString.Length
            Loop

            Console.CursorTop += 1
            Console.Write("Ready! Press Enter.")
            Console.ReadLine()

        End Sub

    End Structure

End Module

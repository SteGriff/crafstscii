Module EntityClass
    Class Entity

        Dim _Name As String
        Dim _Character As Char
        Dim _x As Short
        Dim _y As Short
        Dim _LastX As Short
        Dim _LastY As Short
        Dim _HP As Byte
        Dim _MHP As Byte

        Sub New(ByVal Name As String, ByVal Character As String, ByVal X As Short, ByVal Y As Short, ByVal MHP As Byte)

            Me.Name = Name
            Me.Character = Character
            Me.X = X
            Me.Y = Y
            Me.LastX = X
            Me.LastY = Y
            Me.MHP = MHP
            Me.HP = MHP

        End Sub
        Sub Draw(ByVal World As Map, ByVal Blocks() As BlockGraphic)

            'Undraw last
            DrawBlock(Me.LastX, Me.LastY, World, Blocks)

            'Go to where we need to paint
            PutCursor(X, Y)
            '...and do so
            Console.ForegroundColor = ConsoleColor.White
            Console.BackgroundColor = Blocks(World.Blocks(X, World.Height - Y)).BackColour
            Console.Write(Character)

        End Sub
        Sub Fall(ByVal World As Map)

            Try
                Me.Y = World.Ground(Me.X, Me.Y)
            Catch ex As Exception
                Me.Y = World.Height
            End Try

        End Sub
        Sub Move(ByVal xChange As Short, ByVal World As Map, ByVal Blocks() As BlockGraphic)

            'Store details about the place we are at
            Me.LastX = X
            Me.LastY = Y

            Dim NewX As Short = Me.X + xChange
            If NewX < 0 Then NewX = 0
            If NewX >= World.Width Then NewX = World.Width - 1

            Me.X = NewX
            Me.Fall(World)

            Me.Draw(World, Blocks)

        End Sub
        Sub Damage(ByVal Points As Byte)
            Me.HP -= Points
        End Sub
        Property Name() As String
            Get
                Return _Name
            End Get
            Set(ByVal value As String)
                _Name = value
            End Set
        End Property

        Property Character() As Char
            Get
                Return _Character
            End Get
            Set(ByVal value As Char)
                _Character = value
            End Set
        End Property

        Property X() As Short
            Get
                Return _x
            End Get
            Set(ByVal value As Short)
                _x = value
            End Set
        End Property

        Property Y() As Short
            Get
                Return _y
            End Get
            Set(ByVal value As Short)
                _y = value
            End Set
        End Property

        Property LastX() As Short
            Get
                Return _LastX
            End Get
            Set(ByVal value As Short)
                _LastX = value
            End Set
        End Property

        Property LastY() As Short
            Get
                Return _LastY
            End Get
            Set(ByVal value As Short)
                _LastY = value
            End Set
        End Property

        Property HP() As Byte
            Get
                Return _HP
            End Get
            Set(ByVal value As Byte)
                _HP = value
            End Set
        End Property

        Property MHP() As Byte
            Get
                Return _MHP
            End Get
            Set(ByVal value As Byte)
                _MHP = value
            End Set
        End Property
    End Class

End Module

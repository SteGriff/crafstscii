Public Structure BlockGraphic

    Dim Type As BlockType
    Dim Character As Char
    Dim ForeColour As ConsoleColor
    Dim BackColour As ConsoleColor

    Sub New(ByVal Type As BlockType, ByVal Character As Char, ByVal Fore As ConsoleColor, ByVal Back As ConsoleColor)

        Me.Type = Type
        Me.Character = Character
        Me.ForeColour = Fore
        Me.BackColour = Back

    End Sub
End Structure
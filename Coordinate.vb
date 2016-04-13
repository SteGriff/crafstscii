Public Structure Coordinate
    Dim X As Short
    Dim Y As Short
    Sub New(ByVal X As Short, ByVal Y As Short)
        Me.X = X
        Me.Y = Y
    End Sub
    Shared Operator =(ByVal a As Coordinate, ByVal b As Coordinate) As Boolean
        Return (a.X = b.X) AndAlso (a.Y = b.Y)
    End Operator
    Shared Operator <>(ByVal a As Coordinate, ByVal b As Coordinate) As Boolean
        Return (a.X <> b.X) AndAlso (a.Y <> b.Y)
    End Operator

End Structure

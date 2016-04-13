Public Class Player
    Inherits Entity

    Dim Inventory(8) As InventoryItem

    Sub New(ByVal Name As String, ByVal Character As String, ByVal X As Short, ByVal Y As Short)
        MyBase.New(Name, Character, X, Y, 2)
        For i As Byte = 0 To 7
            Inventory(i) = New InventoryItem(CType(i, BlockType), 0)
        Next
    End Sub

    Sub Gain(ByVal Material As BlockType, ByVal Quantity As Short)
        Me.Inventory(Material).Quantity += Quantity
    End Sub

    Sub Spend(ByVal Material As BlockType, ByVal Quantity As Short)
        Me.Inventory(Material).Quantity -= Quantity
    End Sub

    Function Has(ByVal Material As BlockType, ByVal Quantity As Short) As Boolean

        If Inventory(Material).Quantity >= Quantity Then
            Return True
        Else
            Return False
        End If

    End Function
    Sub DisplayInventory(ByVal World As Map, ByVal BlockGraphics() As BlockGraphic)

        PutCursor(0, World.Height + 1)
        For This As Byte = 1 To 7

            Console.ForegroundColor = BlockGraphics(Inventory(This).Material).ForeColour
            Console.Write("[{0}]{1}: {2}{3}", This, Inventory(This).Material.ToString, Inventory(This).Quantity, vbTab)
            If This Mod 4 = 0 Then
                Console.Write(vbNewLine)
            End If
        Next

    End Sub

End Class

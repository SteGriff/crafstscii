Public Structure InventoryItem
    Dim Material As BlockType
    Dim Quantity As Short
    Sub New(ByVal Material As BlockType, ByVal Quantity As Short)
        Me.Material = Material
        Me.Quantity = Quantity
    End Sub
End Structure

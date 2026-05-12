using DndHotkey.Core;

namespace DndHotkey.App;

internal interface IDndController
{
    DndState GetState();

    DndState SetState(DndState state);

    DndState Toggle();
}

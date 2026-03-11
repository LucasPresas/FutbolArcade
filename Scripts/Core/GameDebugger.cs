using Godot;
using System;

public partial class GameDebugger : Node
{
    // Usamos _Input para que detecte la pulsación incluso si el juego está en pausa
    public override void _Input(InputEvent @event)
    {
        // Verificamos si la acción "RestartGame" fue presionada
        // El nombre debe coincidir exactamente con el de tu captura del Input Map
        if (@event.IsActionPressed("RestartGame"))
        {
            RestartGame();
        }
    }

    private void RestartGame()
    {
        // Mensaje escandaloso en la consola para no olvidarlo
        GD.Print("---------------------------------------------------------");
        GD.PrintErr("!!! DEBUG: NIVEL REINICIADO - BORRAR ESTE NODO AL FINAL !!!");
        GD.Print("---------------------------------------------------------");

        // Recargamos la escena actual
        GetTree().ReloadCurrentScene();
    }
}
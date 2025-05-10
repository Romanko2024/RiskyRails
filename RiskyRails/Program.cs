using RiskyRails;
using System;

try
{
    using var game = new Game1();
    game.Run();
}
catch (Exception ex)
{
    System.Diagnostics.Debug.WriteLine($"Критична помилка: {ex}");
}

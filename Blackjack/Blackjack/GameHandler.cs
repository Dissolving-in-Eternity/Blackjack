using System.Collections.Generic;

namespace Blackjack.Blackjack
{
    public delegate void GameHandler(string s, decimal m, List<Hand> hands, House house);
}
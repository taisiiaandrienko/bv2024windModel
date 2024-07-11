using System.Collections.Generic;

namespace BV2024WindModel.Abstractions
{
    public class Vessel
    {
        public double DeckHeight { get; }
        public double Alpha { get; }
        public List<Building> Buildings { get; }
        public Vessel(double deckHeight, List<Building> buildings, double alpha)
        {
            DeckHeight = deckHeight;
            Buildings = buildings;
            Alpha = alpha;
        }
    }

}

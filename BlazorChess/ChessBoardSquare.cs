
namespace ChessEngine
{
    internal struct Square
    {
        internal Piece? piece;

        internal string bgColour = "#667788";

        public Square()
        {
            bgColour = "#DDDDDD";
            piece = null;
        }

        internal void changeColour(string colour)
        {
            bgColour = colour;
        }
    }
}

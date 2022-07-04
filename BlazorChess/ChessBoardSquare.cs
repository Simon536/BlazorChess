
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

        internal Square(Piece piece1)
        {
            piece = new Piece(piece1);
            bgColour = "#DDDDDD";
        }

        internal void changeColour(string colour)
        {
            bgColour = colour;
        }

        internal string getColour()
        {
            return bgColour;
        }
    }
}

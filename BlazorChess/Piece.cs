
namespace ChessEngine
{
    public enum ChessPieceColour
    {
        White,
        Black
    }

    public enum ChessPieceType
    {
        King,
        Queen,
        Rook,
        Bishop,
        Knight,
        Pawn,
        None
    }

    internal sealed class Piece
    {
        internal ChessPieceColour pieceColour;
        internal ChessPieceType pieceType;

        internal short pieceValue;

        internal short attackedValue;
        internal short defendedValue;

        internal short pieceActionValue;

        internal bool moved;

        internal Stack<byte> validMoves;
        internal int lastValidMoveCount;

        /// <summary>
        /// Used to copy a piece
        /// </summary>
        /// <param name="piece"></param>
        internal Piece(Piece piece)
        {
            pieceColour = piece.pieceColour;
            pieceType = piece.pieceType;
            moved = piece.moved;
            pieceValue = piece.pieceValue;

            if (piece.validMoves != null)
            {
                lastValidMoveCount = piece.validMoves.Count;
            }
        }

        internal Piece(ChessPieceType chessPiece, ChessPieceColour chessPieceColour)
        {
            pieceType = chessPiece;
            pieceColour = chessPieceColour;

            validMoves = new Stack<byte>();

            pieceValue = calculatePieceValue(pieceType);
        }

        private static short calculatePieceValue(ChessPieceType pieceType)
        {
            switch (pieceType)
            {
                case ChessPieceType.Pawn:
                    {
                        return 100;
                    }
                case ChessPieceType.Knight:
                    {
                        //A Knight is worth 3 Pawns
                        return 300;
                    }
                case ChessPieceType.Bishop:
                    {
                        //A Bishop is worth slightly more than 3 Pawns
                        return 310;
                    }
                case ChessPieceType.Rook:
                    {
                        //A Rook is worth 5 Pawns
                        return 500;
                    }
                case ChessPieceType.Queen:
                    {
                        //The Queen is worth 10 Pawns
                        return 1000;
                    }
                case ChessPieceType.King:
                    {
                        //The King's value is the maximum possible value
                        return short.MaxValue;
                    }

                default:
                    {
                        //An empty square is worth nothing
                        return 0;
                    }
            }
        }

        public static ChessPieceColour oppositeColour(ChessPieceColour colour)
        {
            if (colour == ChessPieceColour.Black)
            {
                return ChessPieceColour.White;
            }
            else
            {
                return ChessPieceColour.Black;
            }
        }
    }
}

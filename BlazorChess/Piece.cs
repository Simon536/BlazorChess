
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
        internal int ZobHashValue;

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
            ZobHashValue = piece.ZobHashValue;

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
            ZobHashValue = calculateZobHashValue(chessPiece, chessPieceColour);
        }


        /// <summary>
        /// Use this to get the piece values for Zobrist Hashing
        /// </summary>
        /// <param name="pieceType"></param>
        /// <param name="pieceColour"></param>
        /// <returns>The piece value</returns>
        public static int calculateZobHashValue(ChessPieceType pieceType, ChessPieceColour pieceColour)
        {
            switch (pieceType)
            {
                case ChessPieceType.Pawn:
                    {
                        if (pieceColour == ChessPieceColour.Black)
                        {
                            return 0;
                        }
                        else
                        {
                            return 64;
                        }
                    }
                case ChessPieceType.Rook:
                    {
                        if (pieceColour == ChessPieceColour.Black)
                        {
                            return 128;
                        }
                        else
                        {
                            return 192;
                        }
                    }
                case ChessPieceType.Knight:
                    {
                        if (pieceColour == ChessPieceColour.Black)
                        {
                            return 256;
                        }
                        else
                        {
                            return 320;
                        }
                    }
                case ChessPieceType.Bishop:
                    {
                        if (pieceColour == ChessPieceColour.Black)
                        {
                            return 384;
                        }
                        else
                        {
                            return 448;
                        }
                    }
                case ChessPieceType.Queen:
                    {
                        if (pieceColour == ChessPieceColour.Black)
                        {
                            return 512;
                        }
                        else
                        {
                            return 576;
                        }
                    }
                case ChessPieceType.King:
                    {
                        if (pieceColour == ChessPieceColour.Black)
                        {
                            return 640;
                        }
                        else
                        {
                            return 704;
                        }
                    }
                default:
                    {
                        return 0;
                    }
            }
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

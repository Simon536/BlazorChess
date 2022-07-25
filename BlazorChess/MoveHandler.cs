namespace ChessEngine
{
    public struct moveStruct
    {
        public ulong hash;
        public int score;
        public byte moveOrigin;
        public byte moveDestination;
        public sbyte depth;
    }

    internal static class MoveHandler
    {
        /// <summary>
        /// Please note: this is also used by the AI. Hashing is done here.
        /// </summary>
        /// <param name="board"></param>
        /// <param name="startingPos"></param>
        /// <param name="endingPos"></param>
        /// <returns></returns>
        public static bool movePiece(Board board, byte startingPos, byte endingPos)
        {
            if (board.Squares[startingPos].piece != null)
            {
                //  Reverse the colur to move in the Zobrist hash. (Remember: XOR is its own inverse.)
                board.zobHash ^= board.randomHash.Last();

                if (board.Squares[endingPos].piece != null)
                {
                    //  Remove the captured piece from the hash
                    board.zobHash ^= board.randomHash[board.Squares[endingPos].piece.ZobHashValue + endingPos];
                }

                //  Move piece
                Piece movingPiece = board.Squares[startingPos].piece;
                board.Squares[startingPos].piece = null;
                board.Squares[endingPos].piece = movingPiece;

                //  Handle hashing (undo starting pos and do ending pos)
                board.zobHash ^= board.randomHash[movingPiece.ZobHashValue + startingPos];
                board.zobHash ^= board.randomHash[movingPiece.ZobHashValue + endingPos];

                //  Handle promotions (White)
                if (endingPos < 8 && movingPiece.pieceType == ChessPieceType.Pawn)
                {
                    //  Undo hash
                    board.zobHash ^= board.randomHash[movingPiece.ZobHashValue + endingPos];
                    //  Convert pawn to queen
                    board.Squares[endingPos].piece.pieceType = ChessPieceType.Queen;
                    board.Squares[endingPos].piece.ZobHashValue = Piece.calculateZobHashValue(ChessPieceType.Queen, ChessPieceColour.White);
                    //  Rehash (hardcoded value for white queen)
                    board.zobHash ^= board.randomHash[576 + endingPos];
                }
                //  (Black)
                if (endingPos > 55 && movingPiece.pieceType == ChessPieceType.Pawn)
                {
                    //  Undo hash
                    board.zobHash ^= board.randomHash[movingPiece.ZobHashValue + endingPos];
                    //  Convert pawn to queen
                    board.Squares[endingPos].piece.pieceType = ChessPieceType.Queen;
                    board.Squares[endingPos].piece.ZobHashValue = Piece.calculateZobHashValue(ChessPieceType.Queen, ChessPieceColour.Black);
                    //  Rehash (hardcoded value for black queen)
                    board.zobHash ^= board.randomHash[512 + endingPos];
                }

                //  Check if the king moved. Will be used to determine if castling is legal.
                if (movingPiece.pieceType == ChessPieceType.King && movingPiece.pieceColour == ChessPieceColour.White)
                {
                    board.WhiteKingHasMoved = true;
                }
                if (movingPiece.pieceType == ChessPieceType.King && movingPiece.pieceColour == ChessPieceColour.Black)
                {
                    board.BlackKingHasMoved = true;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Also used by the board to test for check and mate
        /// </summary>
        /// <param name="b"></param>
        /// <param name="sPos"></param>
        /// <param name="ePos"></param>
        /// <param name="playerColour"></param>
        /// <returns>True if the move is valid (Does not test for check)</returns>
        public static bool validMove(Board b, byte sPos, byte ePos, ChessPieceColour playerColour)
        {
            Piece p = b.Squares[sPos].piece;
            Piece piece2Capture = b.Squares[ePos].piece;

            //  Find the row and column for the starting and ending positions
            sbyte sRow = (sbyte)(sPos / 8 + 1);
            sbyte sCol = (sbyte)((sPos % 8) + 1);
            sbyte eRow = (sbyte)(ePos / 8 + 1);
            sbyte eCol = (sbyte)((ePos % 8) + 1);

            if (p == null)
            {
                //  If the piece does not exist
                //MessageBox.Show("Piece does not exist. Try again...");
                return false;

            }

            //  If there is a piece on the ending square
            //  This is merely to handle some specific errors
            if (piece2Capture != null)
            {
                //  If a piece is trying to capture a piece of the same colour
                if (p.pieceColour == piece2Capture.pieceColour)
                {
                    return false;
                }
            }
            
            List<byte> vMoves = AI.getMoves(b, sPos);
            foreach (byte move in vMoves)
            {
                if (move == ePos)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

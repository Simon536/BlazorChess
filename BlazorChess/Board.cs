
namespace ChessEngine
{
    internal sealed class Board
    {
        internal Square[] Squares;

        internal int Score;

        internal bool BlackInCheck;
        internal bool BlackCheckMated;
        internal bool WhiteInCheck;
        internal bool WhiteCheckMated;
        internal bool StaleMate;

        internal bool BlackHasCastled;
        internal bool WhiteHasCastled;

        internal bool BlackKingHasMoved;
        internal bool WhiteKingHasMoved;

        internal bool EndGamePhase;

        internal ChessPieceColour WhosMove;

        internal int MoveCount;

        internal Board()
        {
            Squares = new Square[64];

            for (byte i = 0; i < 64; i++)
            {
                Squares[i] = new Square();
            }

            BlackKingHasMoved = false;
            WhiteKingHasMoved = false;
        }

        private Board(Square[] squares)
        {
            Squares = new Square[64];

            for (byte x = 0; x < 64; x++)
            {
                if (squares[x].piece != null)
                {
                    Squares[x].piece = new Piece(squares[x].piece);
                }
            }
        }

        internal Board FastCopy()
        {
            Board clonedBoard = new Board(Squares);

            clonedBoard.EndGamePhase = EndGamePhase;
            clonedBoard.WhosMove = WhosMove;
            clonedBoard.MoveCount = MoveCount;

            clonedBoard.WhiteKingHasMoved = WhiteKingHasMoved;
            clonedBoard.BlackKingHasMoved = BlackKingHasMoved;

            return clonedBoard;
        }

        /// <summary>
        /// Use this to calculate the board's score. The function will set the board's score directly
        /// </summary>
        internal void scoreBoard()
        {
            Score = 0;
            for (byte i = 0; i < 64; i++)
            {
                if (Squares[i].piece != null)
                {
                    int pieceVal = Squares[i].piece.pieceValue;
                    if (Squares[i].piece.pieceColour == ChessPieceColour.White)
                    {
                        Score += pieceVal;
                    }
                    if (Squares[i].piece.pieceColour == ChessPieceColour.Black)
                    {
                        Score -= pieceVal;
                    }
                }
            }

            //  Small bonus if the King has not yet moved. This is to prevent the engine randomly moving its king.
            if (!WhiteKingHasMoved)
            {
                Score += 15;
            }
            if (!BlackKingHasMoved)
            {
                Score -= 15;
            }

            //  Check logic
            /*
            testForCheck();

            if (WhiteInCheck)
            {
                Score -= 50;
            }
            if (BlackInCheck)
            {
                Score += 50;
            }
            */
            //  END check logic

            Piece p = Squares[27].piece;
            if (p != null)
            {
                if (p.pieceType == ChessPieceType.Pawn)
                {
                    if (p.pieceColour == ChessPieceColour.Black)
                        Score -= 10;
                    if (p.pieceColour == ChessPieceColour.White)
                        Score += 10;
                }
                if (p.pieceType == ChessPieceType.Knight)
                {
                    if (p.pieceColour == ChessPieceColour.Black)
                        Score -= 25;
                    if (p.pieceColour == ChessPieceColour.White)
                        Score += 25;
                }
            }
            p = Squares[28].piece;
            if (p != null)
            {
                if (p.pieceType == ChessPieceType.Pawn)
                {
                    if (p.pieceColour == ChessPieceColour.Black)
                        Score -= 10;
                    if (p.pieceColour == ChessPieceColour.White)
                        Score += 10;
                }
                if (p.pieceType == ChessPieceType.Knight)
                {
                    if (p.pieceColour == ChessPieceColour.Black)
                        Score -= 25;
                    if (p.pieceColour == ChessPieceColour.White)
                        Score += 25;
                }
            }
            p = Squares[35].piece;
            if (p != null)
            {
                if (p.pieceType == ChessPieceType.Pawn)
                {
                    if (p.pieceColour == ChessPieceColour.Black)
                        Score -= 10;
                    if (p.pieceColour == ChessPieceColour.White)
                        Score += 10;
                }
                if (p.pieceType == ChessPieceType.Knight)
                {
                    if (p.pieceColour == ChessPieceColour.Black)
                        Score -= 25;
                    if (p.pieceColour == ChessPieceColour.White)
                        Score += 25;
                }
            }
            p = Squares[36].piece;
            if (p != null)
            {
                if (p.pieceType == ChessPieceType.Pawn)
                {
                    if (p.pieceColour == ChessPieceColour.Black)
                        Score -= 10;
                    if (p.pieceColour == ChessPieceColour.White)
                        Score += 10;
                }
                if (p.pieceType == ChessPieceType.Knight)
                {
                    if (p.pieceColour == ChessPieceColour.Black)
                        Score -= 25;
                    if (p.pieceColour == ChessPieceColour.White)
                        Score += 25;
                }
            }
        }

        /// <summary>
        /// Alters the internal variable
        /// </summary>
        internal void testForEndGamePhase()
        {
            EndGamePhase = false;

            //  Note: There are 32 pieces in the beginning
            int pieceCount = 0;

            for (byte i = 0; i < 64; i++)
            {
                if (Squares[i].piece != null)
                {
                    pieceCount++;
                }
            }

            if (pieceCount < 10)
            {
                EndGamePhase = true;
            }
        }

        /// <summary>
        /// Use for white only!
        /// </summary>
        internal void testForStalemate()
        {
            //  Reset stalemate
            StaleMate = true;

            //  Find all pieces belonging to Player
            List<byte> myPositions = AI.getPiecePositions(this, ChessPieceColour.White);

            //  Generate all possible moves
            List<Tuple<byte, byte>> possibleMoves = AI.getPossibleMoves(this, myPositions);

            if (possibleMoves.Count < 1)
            {
                StaleMate = true;
            }
            else
            {
                Board cpy = FastCopy();
                foreach (Tuple<byte, byte> move in possibleMoves)
                {
                    cpy = FastCopy();
                    MoveHandler.movePiece(cpy, move.Item1, move.Item2);
                    testForCheck();
                    if (WhiteInCheck)
                    {
                        continue;
                    }
                    else
                    {
                        StaleMate = false;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Use this to set the variables WhiteInCheck and BlackInCheck. DOES NOT TEST FOR CHECKMATE!
        /// </summary>
        internal void testForCheck()
        {
            //  Reset check
            WhiteInCheck = false;
            BlackInCheck = false;

            //  Find all of black's pieces
            List<byte> blackPieces = AI.getPiecePositions(this, ChessPieceColour.Black);

            //  Find white king
            byte whiteKing = 0;
            for (byte i = 0; i < 64; i++)
            {
                if (Squares[i].piece != null)
                {
                    if (Squares[i].piece.pieceType == ChessPieceType.King && Squares[i].piece.pieceColour == ChessPieceColour.White)
                    {
                        whiteKing = i;
                        break;
                    }
                }
            }

            //  Check if any black piece can attack the white king
            foreach (byte attacker in blackPieces)
            {
                bool attackSuccessful = MoveHandler.validMove(this, attacker, whiteKing, ChessPieceColour.Black);
                if (attackSuccessful)
                {
                    WhiteInCheck = true;

                    break;
                }
            }
            

            
            //  Find all of white's pieces
            List<byte> whitePieces = AI.getPiecePositions(this, ChessPieceColour.White);

            //  Find black king
            byte blackKing = 0;
            for (byte i = 0; i < 64; i++)
            {
                if (Squares[i].piece != null)
                {
                    if (Squares[i].piece.pieceType == ChessPieceType.King && Squares[i].piece.pieceColour == ChessPieceColour.Black)
                    {
                        blackKing = i;
                        break;
                    }
                }
            }

            //  Check if any white piece can attack the black king
            foreach (byte attacker in whitePieces)
            {
                bool attackSuccessful = MoveHandler.validMove(this, attacker, blackKing, ChessPieceColour.White);
                if (attackSuccessful)
                {
                    BlackInCheck = true;
                    break;
                }
            }
            
        }


        /// <summary>
        /// Now using Unicode :)
        /// </summary>
        /// <param name="piece"></param>
        /// <returns></returns>
        public static string GetPieceCode(Piece? piece)
        {
            if (piece == null)
            {
                return "\u3164";
            }
            switch (piece.pieceType)
            {
                case ChessPieceType.Pawn:
                    {
                        if (piece.pieceColour == ChessPieceColour.Black)
                        {
                            return "\u265f";
                        }
                        else
                        {
                            return "\u2659";
                        }
                    }
                case ChessPieceType.Rook:
                    {
                        if (piece.pieceColour == ChessPieceColour.Black)
                        {
                            return "\u265c";
                        }
                        else
                        {
                            return "\u2656";
                        }
                    }
                case ChessPieceType.Knight:
                    {
                        if (piece.pieceColour == ChessPieceColour.Black)
                        {
                            return "\u265e";
                        }
                        else
                        {
                            return "\u2658";
                        }
                    }
                case ChessPieceType.Bishop:
                    {
                        if (piece.pieceColour == ChessPieceColour.Black)
                        {
                            return "\u265d";
                        }
                        else
                        {
                            return "\u2657";
                        }
                    }
                case ChessPieceType.King:
                    {
                        if (piece.pieceColour == ChessPieceColour.Black)
                        {
                            return "\u265a";
                        }
                        else
                        {
                            return "\u2654";
                        }
                    }
                case ChessPieceType.Queen:
                    {
                        if (piece.pieceColour == ChessPieceColour.Black)
                        {
                            return "\u265b";
                        }
                        else
                        {
                            return "\u2655";
                        }
                    }
                default:
                    {
                        return "";
                    }
            }
        }

        /// <summary>
        /// Zob hash values are currently hardcoded here. Bear that in mind.
        /// </summary>
        /// <returns></returns>
        public static Board setStartingPosition()
        {
            Board b = new();

            //  Add pawns (the easiest)
            //  Remember to remove zero indexing
            for (byte i = 8; i < 16; i++)
            {
                b.Squares[i].piece = new Piece(ChessPieceType.Pawn, ChessPieceColour.Black);
            }

            for (byte i = 48; i < 56; i++)
            {
                b.Squares[i].piece = new Piece(ChessPieceType.Pawn, ChessPieceColour.White);
            }

            //  Add Rooks
            b.Squares[0].piece = new Piece(ChessPieceType.Rook, ChessPieceColour.Black);
            b.Squares[7].piece = new Piece(ChessPieceType.Rook, ChessPieceColour.Black);
            b.Squares[56].piece = new Piece(ChessPieceType.Rook, ChessPieceColour.White);
            b.Squares[63].piece = new Piece(ChessPieceType.Rook, ChessPieceColour.White);

            //  Add Knights
            b.Squares[1].piece = new Piece(ChessPieceType.Knight, ChessPieceColour.Black);
            b.Squares[6].piece = new Piece(ChessPieceType.Knight, ChessPieceColour.Black);
            b.Squares[57].piece = new Piece(ChessPieceType.Knight, ChessPieceColour.White);
            b.Squares[62].piece = new Piece(ChessPieceType.Knight, ChessPieceColour.White);

            //  Add Bishops
            b.Squares[2].piece = new Piece(ChessPieceType.Bishop, ChessPieceColour.Black);
            b.Squares[5].piece = new Piece(ChessPieceType.Bishop, ChessPieceColour.Black);
            b.Squares[58].piece = new Piece(ChessPieceType.Bishop, ChessPieceColour.White);
            b.Squares[61].piece = new Piece(ChessPieceType.Bishop, ChessPieceColour.White);

            //  Add Queens
            b.Squares[3].piece = new Piece(ChessPieceType.Queen, ChessPieceColour.Black);
            b.Squares[59].piece = new Piece(ChessPieceType.Queen, ChessPieceColour.White);

            //  Add Kings
            b.Squares[4].piece = new Piece(ChessPieceType.King, ChessPieceColour.Black);
            b.Squares[60].piece = new Piece(ChessPieceType.King, ChessPieceColour.White);

            return b;
        }
    }
}

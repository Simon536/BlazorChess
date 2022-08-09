
namespace ChessEngine
{
    internal sealed class Board
    {
        internal Square[] Squares;

        internal int Score;

        internal ulong pawns;
        internal ulong rooks;
        internal ulong knights;
        internal ulong bishops;
        internal ulong queens;
        internal ulong kings;
        internal ulong white;
        internal ulong occupied;

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

            clonedBoard.pawns = pawns;
            clonedBoard.rooks = rooks;
            clonedBoard.knights = knights;
            clonedBoard.bishops = bishops;
            clonedBoard.queens = queens;
            clonedBoard.kings = kings;
            clonedBoard.occupied = occupied;
            clonedBoard.white = white;

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
            if (utils.popCount(occupied) < 10)
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
                bool attackSuccessful = MoveHandler.validMove(this, attacker, whiteKing);
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
                bool attackSuccessful = MoveHandler.validMove(this, attacker, blackKing);
                if (attackSuccessful)
                {
                    BlackInCheck = true;
                    break;
                }
            }
            
        }

        internal void updateOccupied()
        {
            occupied = pawns | rooks | knights | bishops | queens | kings;
        }

        internal string GetPieceCode(int squareNum)
        {
            ulong mask = 1ul << squareNum;

            // Check if square is empty
            if ((occupied & mask) == 0)
            {
                return "\u3164";
            }

            // Check for white pieces
            if ((white & mask) != 0)
            {
                if ((pawns & mask) != 0)
                {
                    return "\u2659";
                }
                else if ((rooks & mask) != 0)
                {
                    return "\u2656";
                }
                else if ((knights & mask) != 0)
                {
                    return "\u2658";
                }
                else if ((bishops & mask) != 0)
                {
                    return "\u2657";
                }
                else if ((queens & mask) != 0)
                {
                    return "\u2655";
                }
                else if ((kings & mask) != 0)
                {
                    return "\u2654";
                }
                else return "";
            }
            else // It's a black piece
            {
                if ((pawns & mask) != 0)
                {
                    return "\u265f";
                }
                else if ((rooks & mask) != 0)
                {
                    return "\u265c";
                }
                else if ((knights & mask) != 0)
                {
                    return "\u265e";
                }
                else if ((bishops & mask) != 0)
                {
                    return "\u265d";
                }
                else if ((queens & mask) != 0)
                {
                    return "\u265b";
                }
                else if ((kings & mask) != 0)
                {
                    return "\u265a";
                }
                else return "";
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

        public static Board setStartingPosition()
        {
            Board b = new();

            //  Add pawns (the easiest)
            //  Remember to remove zero indexing
            for (byte i = 8; i < 16; i++)
            {
                b.Squares[i].piece = new Piece(ChessPieceType.Pawn, ChessPieceColour.Black);
                b.pawns = utils.setBit(b.pawns, i);
            }

            for (byte i = 48; i < 56; i++)
            {
                b.Squares[i].piece = new Piece(ChessPieceType.Pawn, ChessPieceColour.White);
                b.pawns = utils.setBit(b.pawns, i);
                b.white = utils.setBit(b.white, i);
            }

            //  Add Rooks
            b.Squares[0].piece = new Piece(ChessPieceType.Rook, ChessPieceColour.Black);
            b.Squares[7].piece = new Piece(ChessPieceType.Rook, ChessPieceColour.Black);
            b.rooks = utils.setBit(b.rooks, 0);
            b.rooks = utils.setBit(b.rooks, 7);
            b.Squares[56].piece = new Piece(ChessPieceType.Rook, ChessPieceColour.White);
            b.Squares[63].piece = new Piece(ChessPieceType.Rook, ChessPieceColour.White);
            b.rooks = utils.setBit(b.rooks, 56);
            b.rooks = utils.setBit(b.rooks, 63);
            b.white = utils.setBit(b.white, 56);
            b.white = utils.setBit(b.white, 63);

            //  Add Knights
            b.Squares[1].piece = new Piece(ChessPieceType.Knight, ChessPieceColour.Black);
            b.Squares[6].piece = new Piece(ChessPieceType.Knight, ChessPieceColour.Black);
            b.knights = utils.setBit(b.knights, 1);
            b.knights = utils.setBit(b.knights, 6);
            b.Squares[57].piece = new Piece(ChessPieceType.Knight, ChessPieceColour.White);
            b.Squares[62].piece = new Piece(ChessPieceType.Knight, ChessPieceColour.White);
            b.knights = utils.setBit(b.knights, 57);
            b.knights = utils.setBit(b.knights, 62);
            b.white = utils.setBit(b.white, 57);
            b.white = utils.setBit(b.white, 62);

            //  Add Bishops
            b.Squares[2].piece = new Piece(ChessPieceType.Bishop, ChessPieceColour.Black);
            b.Squares[5].piece = new Piece(ChessPieceType.Bishop, ChessPieceColour.Black);
            b.bishops = utils.setBit(b.bishops, 2);
            b.bishops = utils.setBit(b.bishops, 5);
            b.Squares[58].piece = new Piece(ChessPieceType.Bishop, ChessPieceColour.White);
            b.Squares[61].piece = new Piece(ChessPieceType.Bishop, ChessPieceColour.White);
            b.bishops = utils.setBit(b.bishops, 58);
            b.bishops = utils.setBit(b.bishops, 61);
            b.white = utils.setBit(b.white, 58);
            b.white = utils.setBit(b.white, 61);

            //  Add Queens
            b.Squares[3].piece = new Piece(ChessPieceType.Queen, ChessPieceColour.Black);
            b.queens = utils.setBit(b.queens, 3);
            b.Squares[59].piece = new Piece(ChessPieceType.Queen, ChessPieceColour.White);
            b.queens = utils.setBit(b.queens, 59);
            b.white = utils.setBit(b.white, 59);

            //  Add Kings
            b.Squares[4].piece = new Piece(ChessPieceType.King, ChessPieceColour.Black);
            b.kings = utils.setBit(b.kings, 4);
            b.Squares[60].piece = new Piece(ChessPieceType.King, ChessPieceColour.White);
            b.kings = utils.setBit(b.kings, 60);
            b.white = utils.setBit(b.white, 60);


            b.occupied = b.pawns | b.rooks | b.knights | b.bishops | b.queens | b.kings;

            return b;
        }
    }
}

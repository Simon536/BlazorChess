namespace ChessEngine
{
    public enum directions
    {
        North,
        South,
        West,
        East,
        NorthWest,
        NorthEast,
        SouthWest,
        SouthEast
    }

    public enum nameToIndex
    {
        A8, B8, C8, D8, E8, F8, G8, H8,
        A7, B7, C7, D7, E7, F7, G7, H7,
        A6, B6, C6, D6, E6, F6, G6, H6,
        A5, B5, C5, D5, E5, F5, G5, H5,
        A4, B4, C4, D4, E4, F4, G4, H4,
        A3, B3, C3, D3, E3, F3, G3, H3,
        A2, B2, C2, D2, E2, F2, G2, H2,
        A1, B1, C1, D1, E1, F1, G1, H1
    }

    internal static class MoveHandler
    {
        static ulong[,] rayAttacks = new ulong[8,64];

        private const ulong notA = 0xFEFEFEFEFEFEFEFE;
        private const ulong notH = 0x7F7F7F7F7F7F7F7F;

        /// <summary>
        /// Please note: this is also used by the AI
        /// </summary>
        /// <param name="board"></param>
        /// <param name="startingPos"></param>
        /// <param name="endingPos"></param>
        /// <returns></returns>
        public static bool movePiece(Board board, byte startingPos, byte endingPos)
        {
            if (board.Squares[startingPos].piece != null)
            {
                //  Move piece
                Piece movingPiece = board.Squares[startingPos].piece;
                board.Squares[startingPos].piece = null;
                board.Squares[endingPos].piece = movingPiece;

                board.pawns = utils.clearBit(board.pawns, endingPos);
                board.rooks = utils.clearBit(board.rooks, endingPos);
                board.knights = utils.clearBit(board.knights, endingPos);
                board.bishops = utils.clearBit(board.bishops, endingPos);
                board.queens = utils.clearBit(board.queens, endingPos);
                board.kings = utils.clearBit(board.kings, endingPos);

                switch (movingPiece.pieceType)
                {
                    case ChessPieceType.Pawn:
                        board.pawns = utils.setBit(board.pawns, endingPos);
                        board.pawns = utils.clearBit(board.pawns, startingPos);
                        break;
                    case ChessPieceType.Rook:
                        board.rooks = utils.setBit(board.rooks, endingPos);
                        board.rooks = utils.clearBit(board.rooks, startingPos);
                        break;
                    case ChessPieceType.Knight:
                        board.knights = utils.setBit(board.knights, endingPos);
                        board.knights = utils.clearBit(board.knights, startingPos);
                        break;
                    case ChessPieceType.Bishop:
                        board.bishops = utils.setBit(board.bishops, endingPos);
                        board.bishops = utils.clearBit(board.bishops, startingPos);
                        break;
                    case ChessPieceType.Queen:
                        board.queens = utils.setBit(board.queens, endingPos);
                        board.queens = utils.clearBit(board.queens, startingPos);
                        break;
                    case ChessPieceType.King:
                        board.kings = utils.setBit(board.kings, endingPos);
                        board.kings = utils.clearBit(board.kings, startingPos);
                        break;
                    default:
                        break;
                }

                if (movingPiece.pieceColour == ChessPieceColour.White)
                {
                    board.white = utils.setBit(board.white, endingPos);
                }
                else
                {
                    board.white = utils.clearBit(board.white, endingPos);
                }

                //  Handle promotions (White)
                if (endingPos < 8 && movingPiece.pieceType == ChessPieceType.Pawn)
                {
                    //  Convert pawn to queen
                    board.Squares[endingPos].piece.pieceType = ChessPieceType.Queen;
                }
                //  (Black)
                if (endingPos > 55 && movingPiece.pieceType == ChessPieceType.Pawn)
                {
                    //  Convert pawn to queen
                    board.Squares[endingPos].piece.pieceType = ChessPieceType.Queen;
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

                board.updateOccupied();

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

        public static ulong getRayAttacks(ulong occupied, directions dir, int square)
        {
            ulong attacks = rayAttacks[(int)dir, square];
            ulong blockers = attacks & occupied;
            int block = utils.bitScanForward(blockers | 0x8000000000000000); // Find location of first blocking piece
            return attacks ^ rayAttacks[(int)dir, block];
        }

        public static void initRayAttacks()
        {
            for (int sq = 0; sq < 64; sq++)
            {
                // North
                ulong attackmask = 0UL;
                for (int bit = sq - 8; bit >= 0; bit -= 8)
                {
                    attackmask |= (1ul << bit);
                }
                rayAttacks[(int)directions.North, sq] = attackmask;

                // South
                attackmask = 0UL;
                for (int bit = sq + 8; bit <= 63; bit += 8)
                {
                    attackmask |= (1ul << bit);
                }
                rayAttacks[(int)directions.South, sq] = attackmask;

                // East
                attackmask = 0UL;
                for (int bit = sq + 1; bit <= 63; bit++)
                {
                    if (((1ul << bit) & notA) != 0)
                    {
                        attackmask |= (1ul << bit);
                    }
                    else break;
                }
                rayAttacks[(int)directions.East, sq] = attackmask;

                // West
                attackmask = 0UL;
                for (int bit = sq - 1; bit >= 0; bit--)
                {
                    if (((1ul << bit) & notH) != 0)
                    {
                        attackmask |= (1ul << bit);
                    }
                    else break;
                }
                rayAttacks[(int)directions.West, sq] = attackmask;

                // NorthEast
                attackmask = 0UL;
                for (int bit = sq - 7; bit >= 0; bit -= 7)
                {
                    if (((1ul << bit) & notA) != 0)
                    {
                        attackmask |= (1ul << bit);
                    }
                    else break;
                }
                rayAttacks[(int)directions.NorthEast, sq] = attackmask;

                // NorthWest
                attackmask = 0UL;
                for (int bit = sq - 9; bit >= 0; bit -= 9)
                {
                    if (((1ul << bit) & notH) != 0)
                    {
                        attackmask |= (1ul << bit);
                    }
                    else break;
                }
                rayAttacks[(int)directions.NorthWest, sq] = attackmask;

                // SouthEast
                attackmask = 0UL;
                for (int bit = sq + 9; bit <= 63; bit += 9)
                {
                    if (((1ul << bit) & notA) != 0)
                    {
                        attackmask |= (1ul << bit);
                    }
                    else break;
                }
                rayAttacks[(int)directions.SouthEast, sq] = attackmask;

                // SouthWest
                attackmask = 0UL;
                for (int bit = sq + 7; bit <= 63; bit += 7)
                {
                    if (((1ul << bit) & notH) != 0)
                    {
                        attackmask |= (1ul << bit);
                    }
                    else break;
                }
                rayAttacks[(int)directions.SouthWest, sq] = attackmask;
            }

        }
    }
}

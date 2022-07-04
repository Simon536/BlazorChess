using System.Diagnostics;

namespace ChessEngine
{
    public enum moveType
    {
        Exact,
        Max,
        Min
    }

    internal sealed class AI
    {
        private static ulong sizeOfLookUpTable = 20000;

        public static Stopwatch stopwatch = new Stopwatch();
        public static Tuple<byte, byte> bMove;

        public static moveStruct[] moveLookupTable = new moveStruct[sizeOfLookUpTable];
        public static int numTimesMoveLookupWasUsed;

        public static int numMaxCutoffs;
        public static int numMinCutoffs;

        public static void move(Board board)
        {
            stopwatch.Reset();
            stopwatch.Start();

            numTimesMoveLookupWasUsed = 0;
            numMaxCutoffs = 0;
            numMinCutoffs = 0;

            //  Uncomment if you use option 1, below.
            //int s;

            board.testForEndGamePhase();

            if (!board.EndGamePhase)
            {
                //  Option 1. Slow
                //Tuple<byte, byte> bMove = recursiveEvaluator(board, 4, ChessPieceColour.Black, out s);

                //  Option 2. Fast
                alphaBetaEvaluator(board, 4, ChessPieceColour.Black, int.MinValue, int.MaxValue, out bMove);
                

                MoveHandler.movePiece(board, bMove.Item1, bMove.Item2);
            }
            else
            {
                //  Option 1. Slow
                //Tuple<byte, byte> bMove = recursiveEvaluator(board, 6, ChessPieceColour.Black, out s);

                //  Option 2. Fast
                alphaBetaEvaluator(board, 6, ChessPieceColour.Black, int.MinValue, int.MaxValue, out bMove);

                MoveHandler.movePiece(board, bMove.Item1, bMove.Item2);
            }

            board.WhosMove = ChessPieceColour.White;
            stopwatch.Stop();
        }

        private static Tuple<byte, byte> findBestMove(Board b, List<Tuple<byte, byte>> possibleMoves, ChessPieceColour colour, out int bestScore)
        {
            Board copyOfBoard;
            Tuple<byte, byte> bestMove = new Tuple<byte, byte>(0,0);
            bestScore = int.MaxValue;

            if (colour == ChessPieceColour.White)
            {
                bestScore = int.MinValue;
            }

            foreach (Tuple<byte, byte> move in possibleMoves)
            {
                copyOfBoard = b.FastCopy();
                MoveHandler.movePiece(copyOfBoard, move.Item1, move.Item2);
                copyOfBoard.scoreBoard();

                if (colour == ChessPieceColour.White && copyOfBoard.Score > bestScore)
                {
                    bestScore = copyOfBoard.Score;
                    bestMove = move;
                }
                if (colour == ChessPieceColour.Black && copyOfBoard.Score < bestScore)
                {
                    bestScore = copyOfBoard.Score;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        /// <summary>
        /// Only use this for black.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="myPieces"></param>
        /// <returns></returns>
        public static List<Tuple<byte, byte>> getPossibleMoves(Board b, List<byte> myPieces)
        {
            List<Tuple<byte, byte>> possibleMoves = new List<Tuple<byte, byte>>();

            foreach (byte i in myPieces)
            {
                List<byte> moves = getMoves(b, i);

                foreach (byte move in moves)
                {
                    //  Test for check
                    Board cpy = b.FastCopy();
                    MoveHandler.movePiece(cpy, i, move);
                    cpy.testForCheck();
                    if (!cpy.BlackInCheck)
                    {
                        possibleMoves.Add(new Tuple<byte, byte>(i, move));
                    }
                }
            }

            return possibleMoves;
        }

        /// <summary>
        /// Also used by the board to test for check and mate
        /// </summary>
        /// <param name="b"></param>
        /// <param name="colour"></param>
        /// <returns></returns>
        public static List<byte> getPiecePositions(Board b, ChessPieceColour colour)
        {
            List<byte> myPieces = new List<byte>(16);

            for (byte i = 0; i < 64; i++)
            {
                if (b.Squares[i].piece != null)
                {
                    if (b.Squares[i].piece.pieceColour == colour)
                    {
                        myPieces.Add(i);
                    }
                }
            }

            return myPieces;
        }

        /// <summary>
        /// Used to find all the positions a particular piece can move to. Helper function for getPossibleMoves, which gets all of the possible moves for a colour. Also used by MoveHandler.validMove for certain pieces.
        /// </summary>
        /// <param name="b">The chessboard to look for moves on.</param>
        /// <param name="pos">The square number of the piece to be moved.</param>
        /// <returns>A list of bytes.</returns>
        public static List<byte> getMoves(Board b, byte pos)
        {
            if (b.Squares[pos].piece != null)
            {
                List<byte> possibleMoves = new List<byte>();
                ChessPieceType pieceType = b.Squares[pos].piece.pieceType;
                ChessPieceColour pieceColour = b.Squares[pos].piece.pieceColour;

                sbyte row = (sbyte)(pos / 8 + 1);
                sbyte col = (sbyte)((pos % 8) + 1);

                //  PAWN
                if (pieceType == ChessPieceType.Pawn)
                {
                    if (pieceColour == ChessPieceColour.White)
                    {
                        //  If it is the pawns first move
                        if (pos >= 48 && pos < 56)
                        {
                            //  If there is nothing in front of the pawn
                            if (b.Squares[(byte)(pos - 8)].piece == null)
                            {
                                possibleMoves.Add((byte)(pos - 8));

                                if (b.Squares[(byte)(pos - 16)].piece == null)
                                {
                                    possibleMoves.Add((byte)(pos - 16));
                                }
                            }
                            //  If there are pieces which can be captured
                            if (b.Squares[(byte)(pos - 7)].piece != null && col != 8)
                            {
                                if (b.Squares[(byte)(pos - 7)].piece.pieceColour != pieceColour)
                                    possibleMoves.Add((byte)(pos - 7));
                            }
                            if (b.Squares[(byte)(pos - 9)].piece != null && col != 1)
                            {
                                if (b.Squares[(byte)(pos - 9)].piece.pieceColour != pieceColour)
                                    possibleMoves.Add((byte)(pos - 9));
                            }

                            return possibleMoves;
                        }

                        //  If it isn't the pawn's first move
                        //  If there is nothing in front of the pawn
                        if (b.Squares[(byte)(pos - 8)].piece == null)
                        {
                            possibleMoves.Add((byte)(pos - 8));
                        }
                        //  If there are pieces which can be captured
                        if (col != 8 && b.Squares[(byte)(pos - 7)].piece != null)
                        {
                            if (b.Squares[(byte)(pos - 7)].piece.pieceColour != pieceColour)
                                possibleMoves.Add((byte)(pos - 7));
                        }
                        //  Note to self: It is important to check the column first here, else this will fail on square 8.
                        if (col != 1 && b.Squares[(byte)(pos - 9)].piece != null)
                        {
                            if (b.Squares[(byte)(pos - 9)].piece.pieceColour != pieceColour)
                                possibleMoves.Add((byte)(pos - 9));
                        }
                        return possibleMoves;

                    }

                    if (pieceColour == ChessPieceColour.Black)
                    {
                        //  If it is the pawns first move
                        if (pos >= 8 && pos < 16)
                        {
                            //  If there is nothing in front of the pawn
                            if (b.Squares[(byte)(pos + 8)].piece == null)
                            {
                                possibleMoves.Add((byte)(pos + 8));

                                if (b.Squares[(byte)(pos + 16)].piece == null)
                                {
                                    possibleMoves.Add((byte)(pos + 16));
                                }
                            }
                            //  If there are pieces which can be captured
                            if (b.Squares[(byte)(pos + 7)].piece != null && col != 1)
                            {
                                if (b.Squares[(byte)(pos + 7)].piece.pieceColour != pieceColour)
                                    possibleMoves.Add((byte)(pos + 7));
                            }
                            if (b.Squares[(byte)(pos + 9)].piece != null && col != 8)
                            {
                                if (b.Squares[(byte)(pos + 9)].piece.pieceColour != pieceColour)
                                    possibleMoves.Add((byte)(pos + 9));
                            }

                            return possibleMoves;
                        }

                        //  If it isn't the pawn's first move
                        //  If there is nothing in front of the pawn
                        if (b.Squares[(byte)(pos + 8)].piece == null)
                        {
                            possibleMoves.Add((byte)(pos + 8));
                        }
                        //  If there are pieces which can be captured
                        if (col != 1 && b.Squares[(byte)(pos + 7)].piece != null)
                        {
                            if (b.Squares[(byte)(pos + 7)].piece.pieceColour != pieceColour)
                                possibleMoves.Add((byte)(pos + 7));
                        }
                        //  Note: It is important to check the column first here, else square 55 will throw an exception.
                        if (col != 8 && b.Squares[(byte)(pos + 9)].piece != null)
                        {
                            if (b.Squares[(byte)(pos + 9)].piece.pieceColour != pieceColour)
                                possibleMoves.Add((byte)(pos + 9));
                        }

                        return possibleMoves;

                    }
                }

                //  ROOK
                if (pieceType == ChessPieceType.Rook)
                {
                    byte finalPos;

                    // Possible moves in the row (part 1)
                    for (sbyte i = (sbyte)(col + 1); i <= 8; i++)
                    {
                        finalPos = (byte)(row * 8 - (8 - i) - 1);
                        //  If the is a piece at the final position
                        if (b.Squares[finalPos].piece != null)
                        {
                            //  If the piece belongs to the other team
                            if (b.Squares[finalPos].piece.pieceColour != b.Squares[pos].piece.pieceColour)
                                possibleMoves.Add(finalPos);
                            break;
                        }
                        //  If there is not a piece
                        else
                        {
                            possibleMoves.Add(finalPos);
                        }
                    }
                    // Possible moves in the row (part 2)
                    for (sbyte i = (sbyte)(col - 1); i >= 1; i--)
                    {
                        finalPos = (byte)(row * 8 - (8 - i) - 1);
                        //  If the is a piece at the final position
                        if (b.Squares[finalPos].piece != null)
                        {
                            //  If the piece belongs to the other team
                            if (b.Squares[finalPos].piece.pieceColour != b.Squares[pos].piece.pieceColour)
                                possibleMoves.Add(finalPos);
                            break;
                        }
                        //  If there is not a piece
                        else
                        {
                            possibleMoves.Add(finalPos);
                        }
                    }

                    // Possible moves in the column (part 1)
                    for (sbyte i = (sbyte)(row + 1); i <= 8; i++)
                    {
                        finalPos = (byte)(i * 8 - (8 - col) - 1);
                        //  If the is a piece at the final position
                        if (b.Squares[finalPos].piece != null)
                        {
                            //  If the piece belongs to the other team
                            if (b.Squares[finalPos].piece.pieceColour != b.Squares[pos].piece.pieceColour)
                                possibleMoves.Add(finalPos);
                            break;
                        }
                        //  If there is not a piece
                        else
                        {
                            possibleMoves.Add(finalPos);
                        }
                    }
                    // Possible moves in the column (part 2)
                    for (sbyte i = (sbyte)(row - 1); i >= 1; i--)
                    {
                        finalPos = (byte)(i * 8 - (8 - col) - 1);
                        //  If the is a piece at the final position
                        if (b.Squares[finalPos].piece != null)
                        {
                            //  If the piece belongs to the other team
                            if (b.Squares[finalPos].piece.pieceColour != b.Squares[pos].piece.pieceColour)
                                possibleMoves.Add(finalPos);
                            break;
                        }
                        //  If there is not a piece
                        else
                        {
                            possibleMoves.Add(finalPos);
                        }
                    }

                    return possibleMoves;
                }
                //  END OF ROOK

                //  KNIGHT

                //  Knight logic appears to be broken!
                if (pieceType == ChessPieceType.Knight)
                {
                    byte finalPos;
                    sbyte nuRow;
                    sbyte nuCol;

                    //  There are eight possible moves for a knight

                    //  KNIGHT MOVE 1
                    nuCol = (sbyte)(col + 1);
                    nuRow = (sbyte)(row + 2);
                    if (nuCol <= 8 && nuRow <= 8)
                    {
                        finalPos = coordsToNum(nuRow, nuCol);
                        if (b.Squares[finalPos].piece != null && b.Squares[finalPos].piece.pieceColour != pieceColour)
                        {
                            possibleMoves.Add(finalPos);
                        }
                        if (b.Squares[finalPos].piece == null)
                        {
                            possibleMoves.Add(finalPos);
                        }
                    }

                    //  KNIGHT MOVE 2
                    nuCol = (sbyte)(col + 2);
                    nuRow = (sbyte)(row + 1);
                    if (nuCol <= 8 && nuRow <= 8)
                    {
                        finalPos = coordsToNum(nuRow, nuCol);
                        if (b.Squares[finalPos].piece != null && b.Squares[finalPos].piece.pieceColour != pieceColour)
                        {
                            possibleMoves.Add(finalPos);
                        }
                        if (b.Squares[finalPos].piece == null)
                        {
                            possibleMoves.Add(finalPos);
                        }
                    }

                    //  KNIGHT MOVE 3
                    nuCol = (sbyte)(col - 1);
                    nuRow = (sbyte)(row - 2);
                    if (nuCol >= 1 && nuRow >= 1)
                    {
                        finalPos = coordsToNum(nuRow, nuCol);
                        if (b.Squares[finalPos].piece != null && b.Squares[finalPos].piece.pieceColour != pieceColour)
                        {
                            possibleMoves.Add(finalPos);
                        }
                        if (b.Squares[finalPos].piece == null)
                        {
                            possibleMoves.Add(finalPos);
                        }
                    }

                    //  KNIGHT MOVE 4
                    nuCol = (sbyte)(col - 2);
                    nuRow = (sbyte)(row - 1);
                    if (nuCol >= 1 && nuRow >= 1)
                    {
                        finalPos = coordsToNum(nuRow, nuCol);
                        if (b.Squares[finalPos].piece != null && b.Squares[finalPos].piece.pieceColour != pieceColour)
                        {
                            possibleMoves.Add(finalPos);
                        }
                        if (b.Squares[finalPos].piece == null)
                        {
                            possibleMoves.Add(finalPos);
                        }
                    }

                    //  KNIGHT MOVE 5
                    nuCol = (sbyte)(col + 1);
                    nuRow = (sbyte)(row - 2);
                    if (nuCol <= 8 && nuRow >= 1)
                    {
                        finalPos = coordsToNum(nuRow, nuCol);
                        if (b.Squares[finalPos].piece != null && b.Squares[finalPos].piece.pieceColour != pieceColour)
                        {
                            possibleMoves.Add(finalPos);
                        }
                        if (b.Squares[finalPos].piece == null)
                        {
                            possibleMoves.Add(finalPos);
                        }
                    }

                    //  KNIGHT MOVE 6
                    nuCol = (sbyte)(col + 2);
                    nuRow = (sbyte)(row - 1);
                    if (nuCol <= 8 && nuRow >= 1)
                    {
                        finalPos = coordsToNum(nuRow, nuCol);
                        if (b.Squares[finalPos].piece != null && b.Squares[finalPos].piece.pieceColour != pieceColour)
                        {
                            possibleMoves.Add(finalPos);
                        }
                        if (b.Squares[finalPos].piece == null)
                        {
                            possibleMoves.Add(finalPos);
                        }
                    }

                    //  KNIGHT MOVE 7
                    nuCol = (sbyte)(col - 1);
                    nuRow = (sbyte)(row + 2);
                    if (nuCol >= 1 && nuRow <= 8)
                    {
                        finalPos = coordsToNum(nuRow, nuCol);
                        if (b.Squares[finalPos].piece != null && b.Squares[finalPos].piece.pieceColour != pieceColour)
                        {
                            possibleMoves.Add(finalPos);
                        }
                        if (b.Squares[finalPos].piece == null)
                        {
                            possibleMoves.Add(finalPos);
                        }
                    }

                    //  KNIGHT MOVE 8
                    nuCol = (sbyte)(col - 2);
                    nuRow = (sbyte)(row + 1);
                    if (nuCol >= 1 && nuRow <= 8)
                    {
                        finalPos = coordsToNum(nuRow, nuCol);
                        if (b.Squares[finalPos].piece != null && b.Squares[finalPos].piece.pieceColour != pieceColour)
                        {
                            possibleMoves.Add(finalPos);
                        }
                        if (b.Squares[finalPos].piece == null)
                        {
                            possibleMoves.Add(finalPos);
                        }
                    }

                    return possibleMoves;
                }

                //  BISHOP
                if (pieceType == ChessPieceType.Bishop)
                {
                    byte finalPos;

                    //  Possible moves along up-right diagonal
                    for (sbyte i = 1; i < 8; i++)
                    {
                        if (row - i >= 1 && col + i <= 8)
                        {
                            finalPos = coordsToNum((sbyte)(row - i), (sbyte)(col + i));

                            if (b.Squares[finalPos].piece == null)
                            {
                                //  There is no piece on the square
                                possibleMoves.Add(finalPos);
                            }
                            else
                            {
                                //  There is a piece on the square
                                if (b.Squares[finalPos].piece.pieceColour != pieceColour)
                                {
                                    //  The piece can be captured
                                    possibleMoves.Add(finalPos);
                                    break;
                                }
                                else
                                {
                                    //  The piece is the same colour, so break immediately
                                    break;
                                }
                            }
                        }
                    }

                    //  Possible moves along up-left diagonal
                    for (sbyte i = 1; i < 8; i++)
                    {
                        if (row - i >= 1 && col - i >= 1)
                        {
                            finalPos = coordsToNum((sbyte)(row - i), (sbyte)(col - i));

                            if (b.Squares[finalPos].piece == null)
                            {
                                //  There is no piece on the square
                                possibleMoves.Add(finalPos);
                            }
                            else
                            {
                                //  There is a piece on the square
                                if (b.Squares[finalPos].piece.pieceColour != pieceColour)
                                {
                                    //  The piece can be captured
                                    possibleMoves.Add(finalPos);
                                    break;
                                }
                                else
                                {
                                    //  The piece is the same colour, so break immediately
                                    break;
                                }
                            }
                        }
                    }

                    //  Possible moves along down-right diagonal
                    for (sbyte i = 1; i < 8; i++)
                    {
                        if (row + i <= 8 && col + i <= 8)
                        {
                            finalPos = coordsToNum((sbyte)(row + i), (sbyte)(col + i));

                            if (b.Squares[finalPos].piece == null)
                            {
                                //  There is no piece on the square
                                possibleMoves.Add(finalPos);
                            }
                            else
                            {
                                //  There is a piece on the square
                                if (b.Squares[finalPos].piece.pieceColour != pieceColour)
                                {
                                    //  The piece can be captured
                                    possibleMoves.Add(finalPos);
                                    break;
                                }
                                else
                                {
                                    //  The piece is the same colour, so break immediately
                                    break;
                                }
                            }
                        }
                    }

                    //  Possible moves along down-left diagonal
                    for (sbyte i = 1; i < 8; i++)
                    {
                        if (row + i <= 8 && col - i >= 1)
                        {
                            finalPos = coordsToNum((sbyte)(row + i), (sbyte)(col - i));

                            if (b.Squares[finalPos].piece == null)
                            {
                                //  There is no piece on the square
                                possibleMoves.Add(finalPos);
                            }
                            else
                            {
                                //  There is a piece on the square
                                if (b.Squares[finalPos].piece.pieceColour != pieceColour)
                                {
                                    //  The piece can be captured
                                    possibleMoves.Add(finalPos);
                                    break;
                                }
                                else
                                {
                                    //  The piece is the same colour, so break immediately
                                    break;
                                }
                            }
                        }
                    }
                }

                //  QUEEN
                if (pieceType == ChessPieceType.Queen)
                {
                    byte finalPos;

                    // Possible moves in the row (part 1)
                    for (sbyte i = (sbyte)(col + 1); i <= 8; i++)
                    {
                        finalPos = coordsToNum(row, i);
                        //  If the is a piece at the final position
                        if (b.Squares[finalPos].piece != null)
                        {
                            //  If the piece belongs to the other team
                            if (b.Squares[finalPos].piece.pieceColour != b.Squares[pos].piece.pieceColour)
                                possibleMoves.Add(finalPos);
                            break;
                        }
                        //  If there is not a piece
                        else
                        {
                            possibleMoves.Add(finalPos);
                        }
                    }
                    // Possible moves in the row (part 2)
                    for (sbyte i = (sbyte)(col - 1); i >= 1; i--)
                    {
                        finalPos = coordsToNum(row, i);
                        //  If the is a piece at the final position
                        if (b.Squares[finalPos].piece != null)
                        {
                            //  If the piece belongs to the other team
                            if (b.Squares[finalPos].piece.pieceColour != b.Squares[pos].piece.pieceColour)
                                possibleMoves.Add(finalPos);
                            break;
                        }
                        //  If there is not a piece
                        else
                        {
                            possibleMoves.Add(finalPos);
                        }
                    }

                    // Possible moves in the column (part 1)
                    for (sbyte i = (sbyte)(row + 1); i <= 8; i++)
                    {
                        finalPos = (byte)(i * 8 - (8 - col) - 1);
                        //  If the is a piece at the final position
                        if (b.Squares[finalPos].piece != null)
                        {
                            //  If the piece belongs to the other team
                            if (b.Squares[finalPos].piece.pieceColour != b.Squares[pos].piece.pieceColour)
                                possibleMoves.Add(finalPos);
                            break;
                        }
                        //  If there is not a piece
                        else
                        {
                            possibleMoves.Add(finalPos);
                        }
                    }
                    // Possible moves in the column (part 2)
                    for (sbyte i = (sbyte)(row - 1); i >= 1; i--)
                    {
                        finalPos = (byte)(i * 8 - (8 - col) - 1);
                        //  If the is a piece at the final position
                        if (b.Squares[finalPos].piece != null)
                        {
                            //  If the piece belongs to the other team
                            if (b.Squares[finalPos].piece.pieceColour != b.Squares[pos].piece.pieceColour)
                                possibleMoves.Add(finalPos);
                            break;
                        }
                        //  If there is not a piece
                        else
                        {
                            possibleMoves.Add(finalPos);
                        }
                    }

                    //  Possible moves along up-right diagonal
                    for (sbyte i = 1; i < 8; i++)
                    {
                        if (row - i >= 1 && col + i <= 8)
                        {
                            finalPos = coordsToNum((sbyte)(row - i), (sbyte)(col + i));

                            if (b.Squares[finalPos].piece == null)
                            {
                                //  There is no piece on the square
                                possibleMoves.Add(finalPos);
                            }
                            else
                            {
                                //  There is a piece on the square
                                if (b.Squares[finalPos].piece.pieceColour != pieceColour)
                                {
                                    //  The piece can be captured
                                    possibleMoves.Add(finalPos);
                                    break;
                                }
                                else
                                {
                                    //  The piece is the same colour, so break immediately
                                    break;
                                }
                            }
                        }
                    }

                    //  Possible moves along up-left diagonal
                    for (sbyte i = 1; i < 8; i++)
                    {
                        if (row - i >= 1 && col - i >= 1)
                        {
                            finalPos = coordsToNum((sbyte)(row - i), (sbyte)(col - i));

                            if (b.Squares[finalPos].piece == null)
                            {
                                //  There is no piece on the square
                                possibleMoves.Add(finalPos);
                            }
                            else
                            {
                                //  There is a piece on the square
                                if (b.Squares[finalPos].piece.pieceColour != pieceColour)
                                {
                                    //  The piece can be captured
                                    possibleMoves.Add(finalPos);
                                    break;
                                }
                                else
                                {
                                    //  The piece is the same colour, so break immediately
                                    break;
                                }
                            }
                        }
                    }

                    //  Possible moves along down-right diagonal
                    for (sbyte i = 1; i < 8; i++)
                    {
                        if (row + i <= 8 && col + i <= 8)
                        {
                            finalPos = coordsToNum((sbyte)(row + i), (sbyte)(col + i));

                            if (b.Squares[finalPos].piece == null)
                            {
                                //  There is no piece on the square
                                possibleMoves.Add(finalPos);
                            }
                            else
                            {
                                //  There is a piece on the square
                                if (b.Squares[finalPos].piece.pieceColour != pieceColour)
                                {
                                    //  The piece can be captured
                                    possibleMoves.Add(finalPos);
                                    break;
                                }
                                else
                                {
                                    //  The piece is the same colour, so break immediately
                                    break;
                                }
                            }
                        }
                    }

                    //  Possible moves along down-left diagonal
                    for (sbyte i = 1; i < 8; i++)
                    {
                        if (row + i <= 8 && col - i >= 1)
                        {
                            finalPos = coordsToNum((sbyte)(row + i), (sbyte)(col - i));

                            if (b.Squares[finalPos].piece == null)
                            {
                                //  There is no piece on the square
                                possibleMoves.Add(finalPos);
                            }
                            else
                            {
                                //  There is a piece on the square
                                if (b.Squares[finalPos].piece.pieceColour != pieceColour)
                                {
                                    //  The piece can be captured
                                    possibleMoves.Add(finalPos);
                                    break;
                                }
                                else
                                {
                                    //  The piece is the same colour, so break immediately
                                    break;
                                }
                            }
                        }
                    }

                    return possibleMoves;
                }
                //  END OF QUEEN

                //  KING
                if (pieceType == ChessPieceType.King)
                {
                    //  Eight possible moves

                    //  First three moves for king
                    if (row != 1)
                    {
                        for (byte i = 7; i <= 9; i++)
                        {
                            if (i == 7 && col == 8)
                            {
                                continue;
                            }
                            if (i == 9 && col == 1)
                            {
                                continue;
                            }

                            byte nuPos = (byte)(pos - i);
                            if (b.Squares[nuPos].piece != null)
                            {
                                if (b.Squares[nuPos].piece.pieceColour != pieceColour)
                                {
                                    possibleMoves.Add(nuPos);
                                }
                            }
                            else
                            {
                                possibleMoves.Add(nuPos);
                            }
                        }
                    }

                    //  Second three moves for king
                    if (row != 8)
                    {
                        for (byte i = 7; i <= 9; i++)
                        {
                            if (i == 7 && col == 1)
                            {
                                continue;
                            }
                            if (i == 9 && col == 8)
                            {
                                continue;
                            }

                            byte nuPos = (byte)(pos + i);
                            if (b.Squares[nuPos].piece != null)
                            {
                                if (b.Squares[nuPos].piece.pieceColour != pieceColour)
                                {
                                    possibleMoves.Add(nuPos);
                                }
                            }
                            else
                            {
                                possibleMoves.Add(nuPos);
                            }
                        }
                    }

                    //  Seventh move for king
                    if (col != 8)
                    {
                        byte nuPos = (byte)(pos + 1);
                        if (b.Squares[nuPos].piece != null)
                        {
                            if (b.Squares[nuPos].piece.pieceColour != pieceColour)
                            {
                                possibleMoves.Add(nuPos);
                            }
                        }
                        else
                        {
                            possibleMoves.Add(nuPos);
                        }
                    }

                    //  Eighth move for king
                    if (col != 1)
                    {
                        byte nuPos = (byte)(pos - 1);
                        if (b.Squares[nuPos].piece != null)
                        {
                            if (b.Squares[nuPos].piece.pieceColour != pieceColour)
                            {
                                possibleMoves.Add(nuPos);
                            }
                        }
                        else
                        {
                            possibleMoves.Add(nuPos);
                        }
                    }
                }

                return possibleMoves;
            }

            return new List<byte>();
        }

        /// <summary>
        /// Calls itself.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="depth"></param>
        /// <param name="colourToMove"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        private static Tuple<byte, byte> recursiveEvaluator(Board b, sbyte depth, ChessPieceColour colourToMove, out int score)
        {
            List<byte> piecePos = getPiecePositions(b, colourToMove);
            List<Tuple<byte, byte>> moves = getPossibleMoves(b, piecePos);

            if (colourToMove == ChessPieceColour.White)
            {
                score = int.MinValue;
            }
            else
            {
                score = int.MaxValue;
            }

            //  The score of the move being evaluated
            int currentScore;
            Tuple<byte, byte> bestMove = new Tuple<byte, byte>(0,0);

            if (depth > 1) {
                //  First check for a matching hash
                if (moveLookupTable[b.zobHash % sizeOfLookUpTable].hash == b.zobHash && moveLookupTable[b.zobHash % sizeOfLookUpTable].depth >= depth)
                {
                    numTimesMoveLookupWasUsed++;
                    moveStruct thisMove = moveLookupTable[b.zobHash % sizeOfLookUpTable];

                    if (thisMove.score > score && colourToMove == ChessPieceColour.White)
                    {
                        score = thisMove.score;
                        bestMove = new Tuple<byte, byte>(thisMove.moveOrigin, thisMove.moveDestination);
                    }
                    if (thisMove.score < score && colourToMove == ChessPieceColour.Black)
                    {
                        score = thisMove.score;
                        bestMove = new Tuple<byte, byte>(thisMove.moveOrigin, thisMove.moveDestination);
                    }
                }

                //  If no hash, then evaluate, and save a hash
                else
                {
                    foreach (Tuple<byte, byte> move in moves)
                    {
                        Board cpy = b.FastCopy();
                        MoveHandler.movePiece(cpy, move.Item1, move.Item2);
                        recursiveEvaluator(cpy, (sbyte)(depth - 1), Piece.oppositeColour(colourToMove), out currentScore);

                        //  Test for check, mate, and stalemate
                        cpy.testForCheck();
                        //  Note: it is now white's move
                        if (colourToMove == ChessPieceColour.Black && cpy.WhiteInCheck)
                        {
                            //  Test for checkmate
                            List<byte> piecePos2 = getPiecePositions(cpy, colourToMove);
                            List<Tuple<byte, byte>> moves2 = getPossibleMoves(cpy, piecePos2);

                            //  Set white checkmated to true, before testing.
                            cpy.WhiteCheckMated = true;

                            foreach (Tuple<byte, byte> move2 in moves2)
                            {
                                Board cpy2 = cpy.FastCopy();
                                MoveHandler.movePiece(cpy2, move2.Item1, move2.Item2);

                                cpy2.testForCheck();

                                if (cpy2.WhiteInCheck)
                                {
                                    continue;
                                }
                                else
                                {
                                    cpy.WhiteCheckMated = false;
                                    break;
                                }
                            }

                            if (cpy.WhiteCheckMated)
                            {
                                score = -10000;
                            }
                        }
                        //  Note: it is now black's move
                        if (colourToMove == ChessPieceColour.White && cpy.BlackInCheck)
                        {
                            //  Test for checkmate
                            List<byte> piecePos2 = getPiecePositions(cpy, colourToMove);
                            List<Tuple<byte, byte>> moves2 = getPossibleMoves(cpy, piecePos2);

                            //  Set black checkmated to true, before testing.
                            cpy.BlackCheckMated = true;

                            foreach (Tuple<byte, byte> move2 in moves2)
                            {
                                Board cpy2 = cpy.FastCopy();
                                MoveHandler.movePiece(cpy2, move2.Item1, move2.Item2);

                                cpy2.testForCheck();

                                if (cpy2.BlackInCheck)
                                {
                                    continue;
                                }
                                else
                                {
                                    cpy.BlackCheckMated = false;
                                    break;
                                }
                            }

                            if (cpy.BlackCheckMated)
                            {
                                score = 10000;
                            }
                        }

                        if (currentScore > score && colourToMove == ChessPieceColour.White)
                        {
                            score = currentScore;
                            bestMove = move;
                        }
                        if (currentScore < score && colourToMove == ChessPieceColour.Black)
                        {
                            score = currentScore;
                            bestMove = move;
                        }
                    }

                    //  Update hash data
                    moveStruct thisMove;
                    thisMove.depth = depth;
                    thisMove.hash = b.zobHash;
                    thisMove.score = score;
                    thisMove.moveOrigin = bestMove.Item1;
                    thisMove.moveDestination = bestMove.Item2;

                    moveLookupTable[b.zobHash % sizeOfLookUpTable] = thisMove;
                }
            }
            //  If depth == 1
            if (depth == 1)
            {
                //  Depth 1 search
                findBestMove(b, moves, colourToMove, out score);
            }

            return bestMove;
        }

        private static int alphaBetaEvaluator(Board b, sbyte depth, ChessPieceColour colourToMove, int min, int max, out Tuple<byte, byte> bestMove)
        {
            bestMove = new Tuple<byte, byte>(0, 0);

            if (depth == 0)
            {
                bestMove = null;
                b.scoreBoard();
                return b.Score;
            }
            else
            {
                List<byte> piecePos = getPiecePositions(b, colourToMove);
                List<Tuple<byte, byte>> moves = getPossibleMoves(b, piecePos);

                //  The score of the move being evaluated
                int currentScore;

                //  Perhaps we should time this?
                b.testForCheck();

                if (b.WhiteInCheck)
                {
                    //  Test for checkmate here
                }
                if (b.BlackInCheck)
                {
                    //  Test for checkmate here
                }

                //  Look at every possible move
                foreach (Tuple<byte, byte> move in moves)
                {
                    //  Copy the board, then do the move
                    Board cpy = b.FastCopy();
                    MoveHandler.movePiece(cpy, move.Item1, move.Item2);

                    Tuple<byte, byte> tempMove;

                    currentScore = alphaBetaEvaluator(cpy, (sbyte)(depth - 1), Piece.oppositeColour(colourToMove), min, max, out tempMove);

                    if (colourToMove == ChessPieceColour.White)
                    {
                        if (currentScore >= max)
                        {
                            bestMove = move;
                            return max;
                        }
                        if (currentScore > min)
                        {
                            min = currentScore;
                            bestMove = move;
                        }
                    }
                    if (colourToMove == ChessPieceColour.Black)
                    {
                        if (currentScore <= min)
                        {
                            bestMove = move;
                            return min;
                        }
                        if (currentScore < max)
                        {
                            max = currentScore;
                            bestMove = move;
                        }
                    }
                }

                if (colourToMove == ChessPieceColour.White)
                {
                    return min;
                }
                else
                {
                    return max;
                }
            }
        }

        /////////////////////////
        //  Utility functions  //
        /////////////////////////

        /// <summary>
        /// Simple utility function.
        /// </summary>
        /// <param name="row">The row number (1 - 8)</param>
        /// <param name="col">The column number (1 - 8)</param>
        /// <returns>The square number (0 - 63)</returns>
        private static byte coordsToNum(sbyte row, sbyte col)
        {
            byte square = (byte)(row * 8 - (8 - col) - 1);
            return square;
        }
        /// <summary>
        /// Simple utility function.
        /// </summary>
        /// <param name="row">The row number (1 - 8)</param>
        /// <param name="col">The column number (1 - 8)</param>
        /// <returns>The square number (0 - 63)</returns>
        private static byte coordsToNum(byte row, byte col)
        {
            byte square = (byte)(row * 8 - (8 - col) - 1);
            return square;
        }
    }
}

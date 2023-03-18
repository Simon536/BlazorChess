using System.Diagnostics;

namespace ChessEngine
{

    internal sealed class AI
    {
        public static Stopwatch stopwatch = new Stopwatch();
        public static Tuple<byte, byte>? bMove;

        public static async Task move(Board board)
        {
            stopwatch.Reset();
            stopwatch.Start();

            board.testForEndGamePhase();

            if (!board.EndGamePhase)
            {
                Console.WriteLine("Beginning move search...");
                Tuple<int, Tuple<byte, byte>> result = await alphaBetaEvaluator(board, 4, ChessPieceColour.Black, int.MinValue, int.MaxValue);
                bMove = result.Item2;
                MoveHandler.movePiece(board, bMove.Item1, bMove.Item2);
            }
            else
            {
                bMove = alphaBetaEvaluator(board, 6, ChessPieceColour.Black, int.MinValue, int.MaxValue).Result.Item2;
                MoveHandler.movePiece(board, bMove.Item1, bMove.Item2);
            }

            Console.WriteLine("AI has completed move search...");

            board.WhosMove = ChessPieceColour.White;
            stopwatch.Stop();
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


        private static async Task<Tuple<int, Tuple<byte, byte>>> alphaBetaEvaluator(Board b, sbyte depth, ChessPieceColour colourToMove, int min, int max)
        {
            if (depth > 3){
                Console.WriteLine("About to delay...");
                await renderDelay();
            }

            Tuple<byte, byte> bestMove = new Tuple<byte, byte>(0, 0);

            if (depth == 0)
            {
                b.scoreBoard();
                return new Tuple<int, Tuple<byte, byte>>(b.Score, bestMove);
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
                    //  TODO: Test for checkmate here
                }
                if (b.BlackInCheck)
                {
                    //  TODO: Test for checkmate here
                }

                //  Look at every possible move
                foreach (Tuple<byte, byte> move in moves)
                {
                    //  Copy the board, then do the move
                    Board cpy = b.FastCopy();
                    MoveHandler.movePiece(cpy, move.Item1, move.Item2);

                    currentScore = alphaBetaEvaluator(cpy, (sbyte)(depth - 1), Piece.oppositeColour(colourToMove), min, max).Result.Item1;

                    if (colourToMove == ChessPieceColour.White)
                    {
                        if (currentScore >= max)
                        {
                            bestMove = move;
                            return new Tuple<int, Tuple<byte, byte>>(max, bestMove);
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
                            return new Tuple<int, Tuple<byte, byte>>(min, bestMove);
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
                    return new Tuple<int, Tuple<byte, byte>>(min, bestMove);
                }
                else
                {
                    return new Tuple<int, Tuple<byte, byte>>(max, bestMove);
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

        private static async Task<int> renderDelay()
        {
            await Task.Delay(1);
            return 0;
        }
    }
}

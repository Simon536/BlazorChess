
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

        //  Used for transposition table
        internal ulong zobHash = 0;
        //  6 piece types for each side times 64 squares equals 768 (12 * 64). Add 1 so that you can show whose turn. 769.
        internal ulong[] randomHash = new ulong[769];

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

        /// <summary>
        /// Used to copy a board
        /// </summary>
        /// <param name="board"></param>
        internal Board(Board board)
        {
            Squares = new Square[64];

            for (byte x=0; x < 64; x++)
            {
                if (board.Squares[x].piece != null)
                {
                    Squares[x] = new Square(board.Squares[x].piece);
                }
            }

            EndGamePhase = board.EndGamePhase;

            WhiteHasCastled = board.WhiteHasCastled;
            BlackHasCastled = board.BlackHasCastled;

            BlackInCheck = board.BlackInCheck;
            WhiteInCheck = board.WhiteInCheck;

            StaleMate = board.StaleMate;

            WhiteCheckMated = board.WhiteCheckMated;
            BlackCheckMated = board.BlackCheckMated;

            BlackKingHasMoved = board.BlackKingHasMoved;
            WhiteKingHasMoved = board.WhiteKingHasMoved;

            WhosMove = board.WhosMove;

            Score = board.Score;

            MoveCount = board.MoveCount;
        }

        internal Board(int score) : this()
        {
            Score = score;
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
            //clonedBoard.BlackHasCastled = BlackHasCastled;
            //clonedBoard.WhiteHasCastled = WhiteHasCastled;

            clonedBoard.WhiteKingHasMoved = WhiteKingHasMoved;
            clonedBoard.BlackKingHasMoved = BlackKingHasMoved;

            clonedBoard.zobHash = zobHash;
            clonedBoard.randomHash = randomHash;

            return clonedBoard;
        }

        internal byte getSquare(string blockName)
        {
            //  Remove 'b' from string
            blockName = blockName.Remove(0, 1);

            //  Convert string to byte
            byte squareNum = Convert.ToByte(blockName);

            //  Return the square
            return squareNum;
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

        internal static void initRandHash(Board b)
        {
            //  It is better to init using the same seed every time.
            Random randomNumGen = new Random(123456);
            byte[] buffer = new byte[8]; ;

            for (int i = 0; i < b.randomHash.GetLength(0); i++)
            {
                randomNumGen.NextBytes(buffer);
                b.randomHash[i] = BitConverter.ToUInt64(buffer, 0);
            }
        }

        //  Legacy. Uses console.

        /*internal static void PrintBoard(Board board)
        {
            Console.Clear();

            string str = "";
            int square = 0;
            byte bgColour = 0;
            ChessPieceColour pieceColour = ChessPieceColour.White;

            board.testForCheck();
            if (board.BlackInCheck)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Black is in check!");
            }
            if (board.WhiteInCheck)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("White is in check!");
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Moves: " + board.MoveCount);
            board.scoreBoard();
            Console.WriteLine("Approx score: " + board.Score);
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.White;

            for (byte i = 0; i < 64; i++)
            {
                if (bgColour == 1)
                {
                    //  If it is a new row
                    if (square == 0)
                    {
                        Console.BackgroundColor = ConsoleColor.Gray;
                        bgColour = 1;
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        bgColour = 0;
                    }
                }
                else
                {
                    //  If it is a new row
                    if (square == 0)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        bgColour = 0;
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Gray;
                        bgColour = 1;
                    }
                }
                if (board.Squares[i].piece != null)
                {
                    str = GetPieceCode(board.Squares[i].piece);

                    //  Handle piece colour logic
                    pieceColour = board.Squares[i].piece.pieceColour;
                    if (pieceColour == ChessPieceColour.White)
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                    else
                        Console.ForegroundColor = ConsoleColor.Black;

                    square++;
                    if (square < 8)
                    {
                        Console.Write(str);
                    }
                    if (square == 8)
                    {
                        Console.WriteLine(str);
                        square = 0;
                    }
                }
                if (board.Squares[i].piece == null)
                {
                    str = " ";
                    square++;
                    if (square < 8)
                    {
                        Console.Write(str);
                    }
                    if (square == 8)
                    {
                        Console.WriteLine(str);
                        square = 0;
                    }
                }
            }

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }
        */

        /// <summary>
        /// Now using Unicode :)
        /// </summary>
        /// <param name="piece"></param>
        /// <returns></returns>
        public static string GetPieceCode(Piece? piece)
        {
            if (piece == null)
            {
                return "-";
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
        /// Get the letter that represents a specific piece.
        /// </summary>
        /// <param name="piece"></param>
        /// <returns></returns>
        public static string GetPieceLetter(Piece piece)
        {
            switch (piece.pieceType)
            {
                case ChessPieceType.Pawn:
                    {
                        if (piece.pieceColour == ChessPieceColour.Black)
                        {
                            return "p";
                        }
                        else
                        {
                            return "P";
                        }
                    }
                case ChessPieceType.Rook:
                    {
                        if (piece.pieceColour == ChessPieceColour.Black)
                        {
                            return "r";
                        }
                        else
                        {
                            return "R";
                        }
                    }
                case ChessPieceType.Knight:
                    {
                        if (piece.pieceColour == ChessPieceColour.Black)
                        {
                            return "n";
                        }
                        else
                        {
                            return "N";
                        }
                    }
                case ChessPieceType.Bishop:
                    {
                        if (piece.pieceColour == ChessPieceColour.Black)
                        {
                            return "b";
                        }
                        else
                        {
                            return "B";
                        }
                    }
                case ChessPieceType.King:
                    {
                        if (piece.pieceColour == ChessPieceColour.Black)
                        {
                            return "k";
                        }
                        else
                        {
                            return "K";
                        }
                    }
                case ChessPieceType.Queen:
                    {
                        if (piece.pieceColour == ChessPieceColour.Black)
                        {
                            return "q";
                        }
                        else
                        {
                            return "Q";
                        }
                    }
                default:
                    {
                        return "";
                    }
            }
        }

        public static Board loadBoardFromString(string stringToLoad)
        {
            Board b = new Board();

            initRandHash(b);

            byte square = 0;

            foreach (char c in stringToLoad)
            {
                //  If the character is a number
                if (char.IsDigit(c))
                {
                    //  Move the 'pointer' by the required number of squares.
                    byte squaresToMove;
                    byte.TryParse(c.ToString(), out squaresToMove);
                    square += squaresToMove;
                }
                else
                {
                    if (c != '\r' && c != '\n')
                    {
                        //  Need to get the piece
                        Piece tempPiece = new Piece(c.ToString());
                        b.Squares[square].piece = tempPiece;
                        square++;
                    }
                }

            }

            return b;
        }

        /// <summary>
        /// Zob hash values are currently hardcoded here. Bear that in mind.
        /// </summary>
        /// <returns></returns>
        public static Board setStartingPosition()
        {
            Board b = new();

            initRandHash(b);

            //  Add pawns (the easiest)
            //  For the hash, the pawn's value falls within 0-63 (black) or 64-127 (white)
            //  Remember to remove zero indexing
            for (byte i = 8; i < 16; i++)
            {
                b.Squares[i].piece = new Piece(ChessPieceType.Pawn, ChessPieceColour.Black);
                b.zobHash ^= b.randomHash[0 + i];
            }

            for (byte i = 48; i < 56; i++)
            {
                b.Squares[i].piece = new Piece(ChessPieceType.Pawn, ChessPieceColour.White);
                b.zobHash ^= b.randomHash[64 + i];
            }

            //  Add Rooks
            //  For the hash, the pawn's value falls within 128-191 (black) or 192-255 (white)
            b.Squares[0].piece = new Piece(ChessPieceType.Rook, ChessPieceColour.Black);
            b.zobHash ^= b.randomHash[128 + 0];
            b.Squares[7].piece = new Piece(ChessPieceType.Rook, ChessPieceColour.Black);
            b.zobHash ^= b.randomHash[128 + 7];
            b.Squares[56].piece = new Piece(ChessPieceType.Rook, ChessPieceColour.White);
            b.zobHash ^= b.randomHash[192 + 56];
            b.Squares[63].piece = new Piece(ChessPieceType.Rook, ChessPieceColour.White);
            b.zobHash ^= b.randomHash[192 + 63];

            //  Add Knights
            //  For the hash, the knight's value falls within 256-319 (black) or 320-383 (white)
            b.Squares[1].piece = new Piece(ChessPieceType.Knight, ChessPieceColour.Black);
            b.zobHash ^= b.randomHash[256 + 1];
            b.Squares[6].piece = new Piece(ChessPieceType.Knight, ChessPieceColour.Black);
            b.zobHash ^= b.randomHash[256 + 6];
            b.Squares[57].piece = new Piece(ChessPieceType.Knight, ChessPieceColour.White);
            b.zobHash ^= b.randomHash[320 + 57];
            b.Squares[62].piece = new Piece(ChessPieceType.Knight, ChessPieceColour.White);
            b.zobHash ^= b.randomHash[320 + 62];

            //  Add Bishops
            //  For the hash, the bishop's value falls within 384-447 (black) or 448-511 (white)
            b.Squares[2].piece = new Piece(ChessPieceType.Bishop, ChessPieceColour.Black);
            b.zobHash ^= b.randomHash[384 + 2];
            b.Squares[5].piece = new Piece(ChessPieceType.Bishop, ChessPieceColour.Black);
            b.zobHash ^= b.randomHash[384 + 5];
            b.Squares[58].piece = new Piece(ChessPieceType.Bishop, ChessPieceColour.White);
            b.zobHash ^= b.randomHash[448 + 58];
            b.Squares[61].piece = new Piece(ChessPieceType.Bishop, ChessPieceColour.White);
            b.zobHash ^= b.randomHash[448 + 61];

            //  Add Queens
            //  For the hash, the queen's value falls within 512-575 (black) or 576-639 (white)
            b.Squares[3].piece = new Piece(ChessPieceType.Queen, ChessPieceColour.Black);
            b.zobHash ^= b.randomHash[512 + 3];
            b.Squares[59].piece = new Piece(ChessPieceType.Queen, ChessPieceColour.White);
            b.zobHash ^= b.randomHash[576 + 59];

            //  Add Kings
            //  For the hash, the king's value falls within 640-703 (black) or 704-767 (white)
            b.Squares[4].piece = new Piece(ChessPieceType.King, ChessPieceColour.Black);
            b.zobHash ^= b.randomHash[640 + 4];
            b.Squares[60].piece = new Piece(ChessPieceType.King, ChessPieceColour.White);
            b.zobHash ^= b.randomHash[704 + 60];

            return b;
        }
    }
}
